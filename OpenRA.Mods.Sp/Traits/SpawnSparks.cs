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

using System;
using System.Linq;
using OpenRA.GameRules;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Sp.Traits
{
	[Desc("Support spark weapons spawning or just simply generates an effect after an interval. Cheapest for Perf.")]
	sealed class SpawnSparksInfo : ConditionalTraitInfo, IRulesetLoaded
	{
		[Desc("Interval of each spark spawning.")]
		public readonly int Interval = 100;

		[Desc("Delay of first spark spawning.")]
		public readonly int FirstDelay = 0;

		[WeaponReference]
		[Desc("Has to be defined in weapons.yaml as well.")]
		public readonly string SparkWeapon = null;

		[Desc("Amount of weapons fired.")]
		public readonly int Amount = 0;

		public readonly bool ForceToGround = true;

		public readonly bool ResetReloadWhenEnabled = true;

		public WeaponInfo WeaponInfo { get; private set; }

		[Desc("Explosion offset relative to actor's position.")]
		public readonly WVec LocalOffset = WVec.Zero;

		[Desc("Image containing launch effect sequence.")]
		public readonly string LaunchEffectImage = null;

		[Desc("Launch effect sequence to play.")]
		[SequenceReference(nameof(LaunchEffectImage), allowNullImage: true)]
		public readonly string[] LaunchEffectSequences = null;

		[Desc("Palette to use for launch effect.")]
		[PaletteReference]
		public readonly string LaunchEffectPalette = "effect";

		public override object Create(ActorInitializer init) { return new SpawnSparks(this, init.Self); }

		public override void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			base.RulesetLoaded(rules, ai);

			if (Amount <= 0)
				return;

			if (SparkWeapon == null)
				throw new YamlException("When Amount > 0, Weapon must have a value");

			var weaponToLower = SparkWeapon.ToLowerInvariant();
			if (!rules.Weapons.TryGetValue(weaponToLower, out var weaponInfo))
				throw new YamlException($"Weapons Ruleset does not contain an entry '{weaponToLower}'");

			WeaponInfo = weaponInfo;
		}
	}

	sealed class SpawnSparks : ConditionalTrait<SpawnSparksInfo>, ITick
	{
		readonly WeaponInfo weapon;
		readonly BodyOrientation body;
		readonly bool hasWeapon;
		readonly bool hasLaunchEffect;

		int interval;

		public SpawnSparks(SpawnSparksInfo info, Actor self)
			: base(info)
		{
			hasWeapon = info.SparkWeapon != null && info.Amount > 0;
			weapon = info.WeaponInfo;
			body = self.TraitOrDefault<BodyOrientation>();
			hasLaunchEffect = !string.IsNullOrEmpty(info.LaunchEffectImage) && info.LaunchEffectSequences?.Length > 0;
			interval = info.FirstDelay;
		}

		void ITick.Tick(Actor self)
		{
			if (!self.IsInWorld || IsTraitDisabled || --interval > 0)
				return;

			var epicenter = self.CenterPosition + (body != null
				? body.LocalToWorld(Info.LocalOffset.Rotate(body.QuantizeOrientation(self.Orientation)))
				: Info.LocalOffset);
			var world = self.World;

			if (hasLaunchEffect)
			{
				self.World.AddFrameEndTask(w => w.Add(new SpriteEffect(epicenter, self.World,
					Info.LaunchEffectImage, Info.LaunchEffectSequences.Random(world.LocalRandom), Info.LaunchEffectPalette)));
			}

			if (!hasWeapon)
			{
				interval = Info.Interval;
				return;
			}

			var map = world.Map;
			var amount = Info.Amount;
			var offset = 1024 / amount;
			for (var i = 0; i < amount; i++)
			{
				var facing = new WAngle(i * offset);
				var rotation = WRot.FromYaw(facing);
				var targetpos = epicenter + new WVec(weapon.Range.Length, 0, 0).Rotate(rotation);
				var radiusTarget = Target.FromPos(new WPos(targetpos.X, targetpos.Y, Info.ForceToGround ? map.CenterOfCell(map.CellContaining(targetpos)).Z : targetpos.Z));

				var projectileArgs = new ProjectileArgs
				{
					Weapon = weapon,
					Facing = facing,
					CurrentMuzzleFacing = () => facing,

					DamageModifiers = Array.Empty<int>(),

					InaccuracyModifiers = Array.Empty<int>(),

					RangeModifiers = Array.Empty<int>(),
					Source = epicenter,
					CurrentSource = () => epicenter,
					SourceActor = self,
					GuidedTarget = radiusTarget,
					PassiveTarget = radiusTarget.CenterPosition
				};

				if (projectileArgs.Weapon.Projectile != null)
				{
					var projectile = projectileArgs.Weapon.Projectile.Create(projectileArgs);
					if (projectile != null)
						world.AddFrameEndTask(w => w.Add(projectile));
				}
			}

			if (weapon.Report != null && weapon.Report.Any())
			{
				if (weapon.AudibleThroughFog || (!self.World.ShroudObscures(epicenter) && !self.World.FogObscures(epicenter)))
					Game.Sound.Play(SoundType.World, weapon.Report, world, epicenter, null, weapon.SoundVolume);
			}

			interval = Info.Interval;
		}

		protected override void TraitEnabled(Actor self)
		{
			if (Info.ResetReloadWhenEnabled)
				interval = 0;
		}
	}
}
