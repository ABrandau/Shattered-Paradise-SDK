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

using OpenRA.GameRules;
using OpenRA.Mods.SP.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Sp.Traits
{
	class ExplodeWeaponOnTeleportRA2Info : TraitInfo, IRulesetLoaded
	{
		[Desc("Effect only affects this teleport type.")]
		public readonly string TeleportType = "RA2ChronoPower";

		[WeaponReference]
		[FieldLoader.Require]
		[Desc("Default weapon to use for explosion.")]
		public readonly string ImpactWeapon = null;

		[WeaponReference]
		[FieldLoader.Require]
		[Desc("Default weapon to use for explosion. Use Weapon if not set.")]
		public readonly string TeleportWeapon = null;

		public WeaponInfo ImpactWeaponInfo { get; private set; }
		public WeaponInfo TeleportWeaponInfo { get; private set; }

		[Desc("Weapon offset relative to actor's position.")]
		public readonly WVec LocalOffset = WVec.Zero;

		public override object Create(ActorInitializer init) { return new ExplodeWeaponOnTeleportRA2(init.Self, this); }

		public void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			if (!string.IsNullOrEmpty(ImpactWeapon))
			{
				var weaponToLower = ImpactWeapon.ToLowerInvariant();
				if (!rules.Weapons.TryGetValue(weaponToLower, out var weapon))
					throw new YamlException($"Weapons Ruleset does not contain an entry '{weaponToLower}'");
				ImpactWeaponInfo = weapon;
			}

			if (!string.IsNullOrEmpty(TeleportWeapon))
			{
				var weaponToLower = TeleportWeapon.ToLowerInvariant();
				if (!rules.Weapons.TryGetValue(weaponToLower, out var weapon))
					throw new YamlException($"Weapons Ruleset does not contain an entry '{weaponToLower}'");
				TeleportWeaponInfo = weapon;
			}
		}
	}

	sealed class ExplodeWeaponOnTeleportRA2 : IOnSuccessfulTeleportRA2
	{
		readonly ExplodeWeaponOnTeleportRA2Info info;
		readonly Actor self;

		public ExplodeWeaponOnTeleportRA2(Actor self, ExplodeWeaponOnTeleportRA2Info info)
		{
			this.info = info;
			this.self = self;
		}

		void IOnSuccessfulTeleportRA2.OnSuccessfulTeleport(string type, WPos oldPos, WPos newPos)
		{
			if (type != info.TeleportType)
				return;

			// Generate a weapon on the place of impact, Generate a weapon on the place of teleport
			var weapon = info.TeleportWeaponInfo;
			var weapon2 = info.ImpactWeaponInfo;
			var firer = self;

			self.World.AddFrameEndTask(w =>
			{
				if (weapon.Report != null && weapon.Report.Length > 0)
				{
					if (weapon.AudibleThroughFog || (!self.World.ShroudObscures(newPos) && !self.World.FogObscures(newPos)))
						Game.Sound.Play(SoundType.World, weapon.Report, self.World, newPos, null, weapon.SoundVolume);
				}

				weapon.Impact(Target.FromPos(newPos), firer);

				if (weapon2.Report != null && weapon2.Report.Length > 0)
				{
					if (weapon2.AudibleThroughFog || (!self.World.ShroudObscures(oldPos) && !self.World.FogObscures(oldPos)))
						Game.Sound.Play(SoundType.World, weapon2.Report, self.World, oldPos, null, weapon2.SoundVolume);
				}

				weapon2.Impact(Target.FromPos(oldPos), firer);
			});
		}
	}
}
