#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	[Desc("Attach this to actors which should regenerate or lose health points over time.")]
	sealed class DamageOnCreationInfo : TraitInfo, Requires<IHealthInfo>
	{
		[Desc("Absolute amount of health points added in each step.",
			"Use negative values to apply damage.")]
		public readonly int Step = 0;

		[Desc("Relative percentages of health added in each step.",
			"Use negative values to apply damage.",
			"When both values are defined, their summary will be applied.")]
		public readonly int PercentageStep = 0;

		[Desc("Apply the health change when encountering these damage types.")]
		public readonly BitSet<DamageType> DamageTypes = default;

		public override object Create(ActorInitializer init) { return new DamageOnCreation(init.Self, this); }
	}

	sealed class DamageOnCreation : ISync, ITick
	{
		readonly IHealth health;
		readonly DamageOnCreationInfo info;
		bool damaged;

		public DamageOnCreation(Actor self, DamageOnCreationInfo info)
		{
			this.info = info;
			health = self.Trait<IHealth>();
		}

		public void Tick(Actor self)
		{
			// Cast to long to avoid overflow when multiplying by the health
			if (!damaged)
			{
				damaged = true;
				self.InflictDamage(self, new Damage((int)-(info.Step + info.PercentageStep * (long)health.MaxHP / 100), info.DamageTypes));
			}
		}
	}
}
