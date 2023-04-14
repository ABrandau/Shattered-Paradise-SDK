using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRA.GameRules;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Sp.Traits
{
	class WithMakeExplodeWeaponInfo : TraitInfo, IRulesetLoaded
	{
		[WeaponReference]
		[FieldLoader.Require]
		[Desc("Has to be defined in weapons.yaml as well. Ignore bursts.")]
		public readonly string Weapon = null;

		public WeaponInfo WeaponInfo { get; private set; }

		[Desc("Weapon offset relative to actor's position.")]
		public readonly WVec LocalOffset = WVec.Zero;

		[Desc("Weapon Yaw relative to actor's position.")]
		public readonly WAngle FireYaw = WAngle.Zero;

		[Desc("Weapon hit offset relative to actor's position.")]
		public readonly WVec HitOffset = WVec.Zero;

		[Desc("Delay the weapon when activate")]
		public readonly bool ExplodesEvenWhenSkipMakeAnimation = true;

		public override object Create(ActorInitializer init) { return new WithMakeExplodeWeapon(init, this); }

		public void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			var weaponToLower = Weapon.ToLowerInvariant();
			if (!rules.Weapons.TryGetValue(weaponToLower, out var weaponInfo))
				throw new YamlException("Weapons Ruleset does not contain an entry '{0}'".F(weaponToLower));

			WeaponInfo = weaponInfo;
		}
	}

	class WithMakeExplodeWeapon : INotifyCreated
	{
		WithMakeExplodeWeaponInfo info;
		readonly bool skipMakeAnimation;

		public WithMakeExplodeWeapon(ActorInitializer init, WithMakeExplodeWeaponInfo info)
		{
			this.info = info;
			skipMakeAnimation = init.Contains<SkipMakeAnimsInit>(info);
		}

		public void Created(Actor self)
		{
			if (skipMakeAnimation && !info.ExplodesEvenWhenSkipMakeAnimation)
				return;

			var body = self.TraitOrDefault<BodyOrientation>();

			var localoffset = body != null
					? body.LocalToWorld(info.LocalOffset.Rotate(body.QuantizeOrientation(self.Orientation)))
					: info.LocalOffset;

			var hitOffset = body != null
				? body.LocalToWorld(info.HitOffset.Rotate(body.QuantizeOrientation(self.Orientation)))
				: info.HitOffset;

			var args = new ProjectileArgs
			{
				Weapon = info.WeaponInfo,
				Facing = CalculateMuzzleOrientation(self).Yaw,
				CurrentMuzzleFacing = () => CalculateMuzzleOrientation(self).Yaw,

				DamageModifiers = new int[] { 100 },

				InaccuracyModifiers = new int[] { 100 },

				RangeModifiers = new int[] { 100 },

				Source = self.CenterPosition + localoffset,
				CurrentSource = () => self.CenterPosition + localoffset,
				SourceActor = self,
				PassiveTarget = self.CenterPosition + hitOffset
			};

			if (args.Weapon.Projectile != null)
			{
				var projectile = args.Weapon.Projectile.Create(args);
				if (projectile != null)
					self.World.Add(projectile);

				if (args.Weapon.Report != null && args.Weapon.Report.Length > 0)
				{
					var pos = self.CenterPosition;
					if (args.Weapon.AudibleThroughFog || (!self.World.ShroudObscures(pos) && !self.World.FogObscures(pos)))
						Game.Sound.Play(SoundType.World, args.Weapon.Report, self.World, pos, null, args.Weapon.SoundVolume);
				}

				if (args.Weapon.StartBurstReport != null && args.Weapon.StartBurstReport.Length > 0)
				{
					var pos = self.CenterPosition;
					if (args.Weapon.AudibleThroughFog || (!self.World.ShroudObscures(pos) && !self.World.FogObscures(pos)))
						Game.Sound.Play(SoundType.World, args.Weapon.StartBurstReport, self.World, pos, null, args.Weapon.SoundVolume);
				}
			}
		}

		protected virtual WRot CalculateMuzzleOrientation(Actor self)
		{
			return WRot.FromYaw(info.FireYaw).Rotate(self.Orientation);
		}
	}
}
