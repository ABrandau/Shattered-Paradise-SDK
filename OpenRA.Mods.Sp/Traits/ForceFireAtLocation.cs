#region Copyright & License Information
/*
 * Copyright 2007-2019 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Sp.Traits
{
	[Desc("Hack: used for veinhole.")]
	sealed class ForceFireAtLocationInfo : ConditionalTraitInfo, Requires<AttackBaseInfo>
	{
		[Desc("Fire offset relative to actor's position. No rotation.")]
		public readonly WVec LocalOffset = WVec.Zero;

		public override object Create(ActorInitializer init) { return new ForceFireAtLocation(this); }
	}

	sealed class ForceFireAtLocation : ConditionalTrait<ForceFireAtLocationInfo>, INotifyCreated
	{
		readonly ForceFireAtLocationInfo info;
		AttackBase[] attackBases;

		public ForceFireAtLocation(ForceFireAtLocationInfo info)
			: base(info)
		{
			this.info = info;
		}

		protected override void Created(Actor self)
		{
			attackBases = self.TraitsImplementing<AttackBase>().ToArray();
			base.Created(self);
		}

		protected override void TraitEnabled(Actor self)
		{
			foreach (var ab in attackBases)
			{
				if (ab.IsTraitDisabled)
					continue;

				ab.AttackTarget(Target.FromPos(self.CenterPosition + info.LocalOffset), AttackSource.Default, false, true, true);
			}

			base.TraitEnabled(self);
		}

		protected override void TraitDisabled(Actor self)
		{
			foreach (var ab in attackBases)
			{
				if (ab.IsTraitDisabled)
					continue;

				ab.OnStopOrder(self);
			}

			base.TraitDisabled(self);
		}
	}
}
