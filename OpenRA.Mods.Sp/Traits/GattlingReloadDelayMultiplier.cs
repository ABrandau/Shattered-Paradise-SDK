#region Copyright & License Information
/*
 * Copyright 2007-2020 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.TA.Traits
{
	[Desc("Modifies the reload time of weapons fired by this actor as the weapon firing.")]
	public class GattlingReloadDelayMultiplierInfo : PausableConditionalTraitInfo
	{
		[Desc("Max Percentage modifier to apply.")]
		public readonly int MaxModifier = 100;

		[Desc("Min Percentage modifier to apply.")]
		public readonly int MinModifier = 25;

		[Desc("How many time trigger the Cool Down when not attack.")]
		public readonly int CoolDownDelay = 20;

		[Desc("The change on reload modifier when not attack.")]
		public readonly int CoolDownChange = 1;

		[Desc("The change on reload modifier when attack.")]
		public readonly int HeatUpChange = -1;

		public override object Create(ActorInitializer init) { return new GattlingReloadDelayMultiplier(this); }
	}

	public class GattlingReloadDelayMultiplier : PausableConditionalTrait<GattlingReloadDelayMultiplierInfo>, IReloadModifier, INotifyAttack, ITick
	{
		int currentModifier;
		int cooldown;

		public GattlingReloadDelayMultiplier(GattlingReloadDelayMultiplierInfo info)
			: base(info)
		{
			currentModifier = info.MaxModifier;
		}

		void ITick.Tick(Actor self)
		{
			if (cooldown <= 0)
				currentModifier += Info.CoolDownChange;
			else
			{
				currentModifier += Info.HeatUpChange;
				cooldown--;
			}

			if (currentModifier > Info.MaxModifier) currentModifier = Info.MaxModifier;
			else if (currentModifier < Info.MinModifier) currentModifier = Info.MinModifier;
		}

		void INotifyAttack.Attacking(Actor self, in Target target, Armament a, Barrel barrel)
		{
			if (IsTraitDisabled || IsTraitPaused)
				return;

			cooldown = Info.CoolDownDelay;
		}

		void INotifyAttack.PreparingAttack(Actor self, in Target target, Armament a, Barrel barrel) { }

		int IReloadModifier.GetReloadModifier() { return IsTraitDisabled ? 100 : currentModifier; }
	}
}
