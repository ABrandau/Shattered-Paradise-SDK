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

using System;
using System.Collections.Generic;
using OpenRA.GameRules;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	[Desc("Trigger an weapon when a support power is triggered. Mainly for visual effect")]
	public class WithSupportPowerActivationExplodeWeaponInfo : ConditionalTraitInfo
	{
		[WeaponReference]
		[FieldLoader.Require]
		[Desc("Has to be defined in weapons.yaml as well.")]
		public readonly string Weapon = null;

		public WeaponInfo WeaponInfo { get; private set; }

		[Desc("Weapon offset relative to actor's position.")]
		public readonly WVec LocalOffset = WVec.Zero;

		[Desc("Weapon Yaw relative to actor's position.")]
		public readonly WAngle FireYaw = WAngle.Zero;

		[Desc("Weapon hit offset relative to actor's position.")]
		public readonly WVec HitOffset = WVec.Zero;

		public override object Create(ActorInitializer init) { return new WithSupportPowerActivationExplodeWeapon(init.Self, this); }

		public override void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			base.RulesetLoaded(rules, ai);

			var weaponToLower = Weapon.ToLowerInvariant();
			if (!rules.Weapons.TryGetValue(weaponToLower, out var weaponInfo))
				throw new YamlException("Weapons Ruleset does not contain an entry '{0}'".F(weaponToLower));

			WeaponInfo = weaponInfo;
		}
	}

	public class WithSupportPowerActivationExplodeWeapon : ConditionalTrait<WithSupportPowerActivationExplodeWeaponInfo>, INotifySupportPower, ITick
	{
		readonly WithSupportPowerActivationExplodeWeaponInfo info;
		readonly WeaponInfo weapon;
		readonly BodyOrientation body;
		bool shouldAcitate;
		int fireDelay;
		int burst;

		readonly List<(int Delay, Action Action)> delayedActions = new List<(int Delay, Action Action)>();

		public WithSupportPowerActivationExplodeWeapon(Actor self, WithSupportPowerActivationExplodeWeaponInfo info)
			: base(info)
		{
			this.info = info;

			weapon = info.WeaponInfo;
			burst = weapon.Burst;
			body = self.TraitOrDefault<BodyOrientation>();
		}

		void INotifySupportPower.Charged(Actor self) { }

		void INotifySupportPower.Activated(Actor self)
		{
			shouldAcitate = true;
			burst = weapon.Burst;
		}

		public void Tick(Actor self)
		{
			if (!shouldAcitate || IsTraitDisabled)
				return;

			for (var i = 0; i < delayedActions.Count; i++)
			{
				var x = delayedActions[i];
				if (--x.Delay <= 0)
					x.Action();
				delayedActions[i] = x;
			}

			delayedActions.RemoveAll(a => a.Delay <= 0);

			if (--fireDelay < 0)
			{
				var localoffset = body != null
					? body.LocalToWorld(info.LocalOffset.Rotate(body.QuantizeOrientation(self.Orientation)))
					: info.LocalOffset;

				var hitOffset = body != null
					? body.LocalToWorld(info.HitOffset.Rotate(body.QuantizeOrientation(self.Orientation)))
					: info.HitOffset;

				var args = new ProjectileArgs
				{
					Weapon = weapon,
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

					if (burst == args.Weapon.Burst && args.Weapon.StartBurstReport != null && args.Weapon.StartBurstReport.Length > 0)
					{
						var pos = self.CenterPosition;
						if (args.Weapon.AudibleThroughFog || (!self.World.ShroudObscures(pos) && !self.World.FogObscures(pos)))
							Game.Sound.Play(SoundType.World, args.Weapon.StartBurstReport, self.World, pos, null, args.Weapon.SoundVolume);
					}
				}

				if (--burst > 0)
				{
					if (weapon.BurstDelays.Length == 1)
						fireDelay = weapon.BurstDelays[0];
					else
						fireDelay = weapon.BurstDelays[weapon.Burst - (burst + 1)];
				}
				else
				{
					fireDelay = 0;
					burst = weapon.Burst;
					shouldAcitate = false;

					if (weapon.AfterFireSound != null && weapon.AfterFireSound.Length != 0)
					{
						ScheduleDelayedAction(weapon.AfterFireSoundDelay, () =>
						{
							var pos = self.CenterPosition;
							if (weapon.AudibleThroughFog || (!self.World.ShroudObscures(pos) && !self.World.FogObscures(pos)))
								Game.Sound.Play(SoundType.World, weapon.AfterFireSound.Random(self.World.SharedRandom), pos, weapon.SoundVolume);
						});
					}
				}
			}
		}

		protected void ScheduleDelayedAction(int t, Action a)
		{
			if (t > 0)
				delayedActions.Add((t, a));
			else
				a();
		}

		protected virtual WRot CalculateMuzzleOrientation(Actor self)
		{
			return WRot.FromYaw(Info.FireYaw).Rotate(self.Orientation);
		}

		protected override void TraitDisabled(Actor self)
		{
			shouldAcitate = false;
			base.TraitDisabled(self);
		}
	}
}
