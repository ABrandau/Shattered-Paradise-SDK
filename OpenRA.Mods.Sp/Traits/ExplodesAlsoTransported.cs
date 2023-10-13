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

using OpenRA.GameRules;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	[Desc("This actor explodes when killed, also inside transport.")]
	public class ExplodesAlsoTransportedInfo : ConditionalTraitInfo
	{
		[WeaponReference]
		[FieldLoader.Require]
		[Desc("Default weapon to use for explosion.")]
		public readonly string Weapon = null;

		[Desc("Default weapon to use for explosion. Use Weapon if not set.")]
		public readonly string WeaponWhenTransport = null;

		[Desc("DeathType(s) that trigger the explosion. Leave empty to always trigger an explosion.")]
		public readonly BitSet<DamageType> DeathTypes = default;

		[Desc("Who is counted as source of damage for explosion.",
			"Possible values are Self and Killer.")]
		public readonly DamageSource DamageSource = DamageSource.Self;

		[Desc("Possible values are CenterPosition (explosion at the actors' center) and ",
			"Footprint (explosion on each occupied cell).")]
		public readonly ExplosionType Type = ExplosionType.CenterPosition;

		[Desc("Offset of the explosion from the center of the exploding actor (or cell).")]
		public readonly WVec Offset = WVec.Zero;

		public WeaponInfo WeaponInfo { get; private set; }
		public WeaponInfo WeaponWhenTransportInfo { get; private set; }

		public override object Create(ActorInitializer init) { return new ExplodesAlsoTransported(this, init.Self); }
		public override void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			if (!string.IsNullOrEmpty(Weapon))
			{
				var weaponToLower = Weapon.ToLowerInvariant();
				if (!rules.Weapons.TryGetValue(weaponToLower, out var weapon))
					throw new YamlException($"Weapons Ruleset does not contain an entry '{weaponToLower}'");
				WeaponInfo = weapon;
			}

			if (!string.IsNullOrEmpty(WeaponWhenTransport))
			{
				var weaponToLower = WeaponWhenTransport.ToLowerInvariant();
				if (rules.Weapons.TryGetValue(weaponToLower, out var weapon))
					WeaponWhenTransportInfo = weapon;
			}

			base.RulesetLoaded(rules, ai);
		}
	}

	public class ExplodesAlsoTransported : ConditionalTrait<ExplodesAlsoTransportedInfo>, INotifyKilled
	{
		BuildingInfo buildingInfo;

		public ExplodesAlsoTransported(ExplodesAlsoTransportedInfo info, Actor self)
			: base(info) { }

		protected override void Created(Actor self)
		{
			buildingInfo = self.Info.TraitInfoOrDefault<BuildingInfo>();

			base.Created(self);
		}

		void INotifyKilled.Killed(Actor self, AttackInfo e)
		{
			if (IsTraitDisabled)
				return;

			if (!Info.DeathTypes.IsEmpty && !e.Damage.DamageTypes.Overlaps(Info.DeathTypes))
				return;

			WPos? impactPos = null;
			var weapon = Info.WeaponInfo;
			if (!self.IsInWorld)
			{
				weapon = Info.WeaponWhenTransportInfo ?? Info.WeaponInfo;

				// We only explode when our transport in world
				foreach (var carryable in self.TraitsImplementing<Carryable>())
				{
					var carrier = carryable.Carrier;
					if (carrier == null || !carrier.IsInWorld)
						continue;
					var carryall = carrier.TraitOrDefault<Carryall>();
					if (carryall != null && carryall.State == Carryall.CarryallState.Carrying)
					{
						impactPos = carrier.CenterPosition + carryall.CarryableOffset + Info.Offset;
						break;
					}
				}

				if (!impactPos.HasValue)
				{
					foreach (var pass in self.TraitsImplementing<Passenger>())
					{
						var transport = pass.Transport;
						if (transport == null || !transport.IsInWorld)
							continue;
						var cargo = transport.TraitOrDefault<Cargo>();
						if (cargo != null && cargo.PassengerCount > 0)
						{
							impactPos = transport.CenterPosition + Info.Offset;
							break;
						}
					}
				}
			}
			else
				impactPos = self.CenterPosition + Info.Offset;

			if (!impactPos.HasValue)
				return;

			if (weapon.Report != null && weapon.Report.Length > 0)
			{
				var pos = self.CenterPosition;
				if (weapon.AudibleThroughFog || (!self.World.ShroudObscures(pos) && !self.World.FogObscures(pos)))
					Game.Sound.Play(SoundType.World, weapon.Report, self.World, pos, null, weapon.SoundVolume);
			}

			var source = Info.DamageSource == DamageSource.Self ? self : e.Attacker;
			if (Info.Type == ExplosionType.Footprint && buildingInfo != null)
			{
				var cells = buildingInfo.OccupiedTiles(self.Location);
				foreach (var c in cells)
					weapon.Impact(Target.FromPos(self.World.Map.CenterOfCell(c) + Info.Offset), source);

				return;
			}

			// Use .FromPos since this actor is killed. Cannot use Target.FromActor
			weapon.Impact(Target.FromPos(impactPos.Value), source);
		}
	}
}
