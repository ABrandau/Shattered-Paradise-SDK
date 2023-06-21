#region Copyright & License Information
/*
 * Copyright 2007-2022 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Mods.AS.Traits;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	[Desc("Change the health of SharedPassenger actors when they are in typical SharedCargo.")]
	public sealed class ChangeSharedPassengerHealthInfo : PausableConditionalTraitInfo, Requires<SharedCargoManagerInfo>
	{
		[Desc("`SharedCargoManager.Type` that this actor shares its passengers.")]
		public readonly string ShareType = "tunnel";

		[Desc("Absolute amount of health points added in each step.",
			"Use negative values to apply damage.")]
		public readonly int Step = 5;

		[Desc("Relative percentages of health added in each step.",
			"Use negative values to apply damage.",
			"When both values are defined, their summary will be applied.")]
		public readonly int PercentageStep = 0;

		[Desc("Time in ticks to wait between each health modification.")]
		public readonly int Delay = 5;

		[Desc("Heal if current health is below this percentage of full health.")]
		public readonly int StartIfBelow = 50;

		[Desc("Apply the health change when encountering these damage types.")]
		public readonly BitSet<DamageType> DamageTypes = default;

		public override object Create(ActorInitializer init) { return new ChangeSharedPassengerHealth(this); }
	}

	public sealed class ChangeSharedPassengerHealth : PausableConditionalTrait<ChangeSharedPassengerHealthInfo>, INotifyCreated, ITick
	{
		SharedCargoManager manager;

		[Sync]
		int ticks;

		public ChangeSharedPassengerHealth(ChangeSharedPassengerHealthInfo info)
		: base(info) { }

		protected override void Created(Actor self)
		{
			manager = self.Owner.PlayerActor.TraitsImplementing<SharedCargoManager>().Where(m => m.Info.Type == Info.ShareType).First();
		}

		public void Tick(Actor self)
		{
			if (IsTraitDisabled || IsTraitPaused)
				return;

			if (--ticks <= 0)
			{
				ticks = Info.Delay;

				foreach (var p in manager.Cargo)
					ChangePassengerHealth(p);
			}
		}

		void ChangePassengerHealth(Actor actor)
		{
			var health = actor.TraitOrDefault<IHealth>();

			if (health == null)
				return;

			// Cast to long to avoid overflow when multiplying by the health
			var maxHP = (long)health.MaxHP;

			if (health.HP >= Info.StartIfBelow * maxHP / 100)
				return;

			actor.InflictDamage(actor, new Damage((int)-(Info.Step + Info.PercentageStep * maxHP / 100), Info.DamageTypes));
		}
	}
}
