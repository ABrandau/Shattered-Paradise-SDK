#region Copyright & License Information
/*
 * Copyright The OpenRA-SP Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	sealed class AutoDemolisherInfo : PausableConditionalTraitInfo, Requires<DemolitionInfo>
	{
		[Desc("Maximum range to scan for targets.")]
		public readonly WDist ScanRadius = WDist.FromCells(5);

		[Desc("Player relationships the owner of the actor needs to get targeted.")]
		public readonly PlayerRelationship TargetRelationships = PlayerRelationship.Enemy;

		public override object Create(ActorInitializer init) { return new AutoDemolisher(init.Self, this); }
	}

	sealed class AutoDemolisher : PausableConditionalTrait<AutoDemolisherInfo>, INotifyBecomingIdle
	{
		readonly Demolition demolish;
		Actor demolished;

		public AutoDemolisher(Actor self, AutoDemolisherInfo info)
			: base(info)
		{
			demolish = self.TraitsImplementing<Demolition>().First();
		}

		void INotifyBecomingIdle.OnBecomingIdle(Actor self)
		{
			if (IsTraitDisabled || IsTraitPaused)
				return;

			var targetActor = self.World.FindActorsInCircle(self.CenterPosition, Info.ScanRadius)
				.Where(a =>
				{
					if (a == self || a == demolished || a.IsDead || !a.IsInWorld || !Info.TargetRelationships.HasRelationship(self.Owner.RelationshipWith(a.Owner)) ||
					!a.TraitsImplementing<IDemolishable>().Any(d => d.IsValidTarget(a, self)))
						return false;

					var hasModifier = false;
					var visModifiers = a.TraitsImplementing<IVisibilityModifier>();
					foreach (var v in visModifiers)
					{
						if (v.IsVisible(a, self.Owner))
							return true;

						hasModifier = true;
					}

					return !hasModifier;
				}).ClosestToWithPathFrom(self);

			if (demolished != null)
				demolished = null;
			else
				demolished = targetActor;

			if (targetActor == null)
				return;

			self.QueueActivity(demolish.GetDemolishActivity(self, Target.FromActor(targetActor), demolish.Info.TargetLineColor));
		}
	}
}
