using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRA.GameRules;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Sp.Traits
{
	[Desc("Launch weapon or/and generate sprite effect when created or deploying.")]
	class WithMakeExplodeWeaponInfo : TraitInfo, IRulesetLoaded
	{
		[WeaponReference]
		[FieldLoader.AllowEmptyEntries]
		[Desc("Has to be defined in weapons.yaml as well. Ignore bursts.")]
		public readonly string Weapon = null;

		public WeaponInfo WeaponInfo { get; private set; }

		[Desc("Weapon offset relative to actor's position.")]
		public readonly WVec LocalOffset = WVec.Zero;

		[Desc("Weapon Yaw relative to actor's position.")]
		public readonly WAngle FireYaw = WAngle.Zero;

		[Desc("Weapon hit offset relative to actor's position.")]
		public readonly WVec HitOffset = WVec.Zero;

		[Desc("Trigger the weapon when create")]
		public readonly bool ExplodesWhenCreated = true;

		[Desc("Trigger the weapon even when skip MakeAnimation")]
		public readonly bool ExplodesEvenWhenSkipMakeAnimation = true;

		[Desc("Trigger the weapon when deploy")]
		public readonly bool ExplodesWhenDeploy = true;

		[Desc("Trigger the weapon when undeploy")]
		public readonly bool ExplodesWhenUndeploy = true;

		[Desc("Image containing launch effect sequence.")]
		public readonly string LaunchEffectImage = null;

		[Desc("Launch effect sequence to play.")]
		[SequenceReference(nameof(LaunchEffectImage), allowNullImage: true)]
		public readonly string LaunchEffectSequence = null;

		[Desc("Palette to use for launch effect.")]
		[PaletteReference]
		public readonly string LaunchEffectPalette = "effect";

		public override object Create(ActorInitializer init) { return new WithMakeExplodeWeapon(init, this); }

		public void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			if (Weapon == null)
				return;

			var weaponToLower = Weapon.ToLowerInvariant();
			if (!rules.Weapons.TryGetValue(weaponToLower, out var weaponInfo))
				throw new YamlException($"Weapons Ruleset does not contain an entry '{weaponToLower}'");

			WeaponInfo = weaponInfo;
		}
	}

	class WithMakeExplodeWeapon : INotifyCreated, INotifyDeployTriggered
	{
		WithMakeExplodeWeaponInfo info;
		readonly bool skipMakeAnimation;
		BodyOrientation body;
		bool hasWeapon;
		bool hasLaunchEffect;

		public WithMakeExplodeWeapon(ActorInitializer init, WithMakeExplodeWeaponInfo info)
		{
			this.info = info;
			skipMakeAnimation = init.Contains<SkipMakeAnimsInit>(info);
			body = init.Self.TraitOrDefault<BodyOrientation>();
			hasWeapon = info.Weapon != null;
			hasLaunchEffect = !string.IsNullOrEmpty(info.LaunchEffectImage) && !string.IsNullOrEmpty(info.LaunchEffectSequence);
		}

		public void Created(Actor self)
		{
			if (!info.ExplodesWhenCreated || (skipMakeAnimation && !info.ExplodesEvenWhenSkipMakeAnimation))
				return;

			LaunchWeapon(self);
		}

		void LaunchWeapon(Actor self)
		{
			var localoffset = body != null
					? body.LocalToWorld(info.LocalOffset.Rotate(body.QuantizeOrientation(self.Orientation)))
					: info.LocalOffset;

			var muzzleFacing = CalculateMuzzleOrientation(self).Yaw;

			if (hasLaunchEffect)
			{
				self.World.AddFrameEndTask(w => w.Add(new SpriteEffect(self.CenterPosition + localoffset, muzzleFacing, self.World,
					info.LaunchEffectImage, info.LaunchEffectSequence, info.LaunchEffectPalette)));
			}

			if (!hasWeapon)
				return;

			var hitOffset = body != null
				? body.LocalToWorld(info.HitOffset.Rotate(body.QuantizeOrientation(self.Orientation)))
				: info.HitOffset;

			var args = new ProjectileArgs
			{
				Weapon = info.WeaponInfo,
				Facing = muzzleFacing,
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

		void INotifyDeployTriggered.Deploy(Actor self, bool skipMakeAnim)
		{
			if(info.ExplodesWhenDeploy)
				LaunchWeapon(self);
		}

		void INotifyDeployTriggered.Undeploy(Actor self, bool skipMakeAnim)
		{
			if (info.ExplodesWhenUndeploy)
				LaunchWeapon(self);
		}
	}
}
