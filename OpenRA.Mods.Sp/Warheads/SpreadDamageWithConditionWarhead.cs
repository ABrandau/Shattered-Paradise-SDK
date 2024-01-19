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

using System.Collections.Generic;
using System.Linq;
using OpenRA.GameRules;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Warheads;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Warheads
{
	[Desc("Apply Damage/Condition in a specified range related with hitshape.")]
	public class SpreadDamageWithConditionWarhead : SpreadDamageWarhead
	{
		[FieldLoader.Require]
		[Desc("The condition to apply. Must be included in the target actor's ExternalConditions list.")]
		public readonly string Condition = null;

		[Desc("Duration of the condition (in ticks).")]
		public readonly int ConditionDuration = 0;

		[Desc("Condition duration percentage versus each armor type. 100% by default")]
		public readonly Dictionary<string, int> ConditionVersus = new();

		protected override void InflictDamage(Actor victim, Actor firedBy, HitShape shape, WarheadArgs args)
		{
			// Apply the condition first
			var duration = Util.ApplyPercentageModifiers(ConditionDuration, new int[] { ConditionDurationVersus(victim, shape) });
			if (duration > 0)
				victim.TraitsImplementing<ExternalCondition>()
					.FirstOrDefault(t => t.Info.Condition == Condition && t.CanGrantCondition(firedBy))
					?.GrantCondition(victim, firedBy, duration);

			// Then deals damage
			var damage = Util.ApplyPercentageModifiers(Damage, args.DamageModifiers.Append(DamageVersus(victim, shape, args)));
			victim.InflictDamage(firedBy, new Damage(damage, DamageTypes));
		}

		int ConditionDurationVersus(Actor victim, HitShape shape)
		{
			// If no DurationVersus values are defined, DamageVersus would return 100 anyway, so we might as well do that early.
			if (ConditionVersus.Count == 0)
				return 100;

			var armor = victim.TraitsImplementing<Armor>()
				.Where(a => !a.IsTraitDisabled && a.Info.Type != null && ConditionVersus.ContainsKey(a.Info.Type) &&
					(shape.Info.ArmorTypes.IsEmpty || shape.Info.ArmorTypes.Contains(a.Info.Type)))
				.Select(a => ConditionVersus[a.Info.Type]);

			return Util.ApplyPercentageModifiers(100, armor);
		}
	}
}
