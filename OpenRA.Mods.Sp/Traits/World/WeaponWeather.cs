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
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	[TraitLocation(SystemActors.World)]
	[Desc("Create a map-wide weather based on weapons.")]
	class WeaponWeatherInfo : ConditionalTraitInfo, IRulesetLoaded
	{
		[WeaponReference]
		[FieldLoader.Require]
		[Desc("Has to be defined in weapons.yaml as well.")]
		public readonly string[] Weapons = Array.Empty<string>();

		[Desc("How many weapons should be fired per 1000 map cells (on average).")]
		public readonly int[] Amount = { 1 };

		[Desc("Firing interval.")]
		public readonly int[] Interval = { 1 };

		public readonly WDist Altitude = WDist.Zero;

		[Desc("Should this storm be associated with the owner player?")]
		public readonly bool HasOwner = false;

		public WeaponInfo[] WeaponInfos { get; private set; }

		public override void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			base.RulesetLoaded(rules, ai);

			WeaponInfos = Weapons.Select(w =>
			{
				var weaponToLower = w.ToLowerInvariant();
				if (!rules.Weapons.TryGetValue(weaponToLower, out var weapon))
					throw new YamlException($"Weapons Ruleset does not contain an entry '{weaponToLower}'");
				return weapon;
			}).ToArray();
		}

		public override object Create(ActorInitializer init) { return new WeaponWeather(this); }
	}

	class WeaponWeather : ConditionalTrait<WeaponWeatherInfo>, ITick
	{
		World world;
		bool firstTick = true;
		Actor firer;

		[Sync]
		public int Interval { get; private set; }

		[Sync]
		public int Amount { get; private set; }

		public WeaponWeather(WeaponWeatherInfo info)
			: base(info) { }

		void ITick.Tick(Actor self)
		{
			if (firstTick)
			{
				world = self.World;
				firer = Info.HasOwner ? self.Owner.PlayerActor : world.WorldActor;

				firstTick = false;
				Interval = Info.Interval.Length == 2
					? world.SharedRandom.Next(Info.Interval[0], Info.Interval[1])
					: Info.Interval[0];
			}

			if (IsTraitDisabled || --Interval > 0)
				return;

			Interval = Info.Interval.Length == 2
				? world.SharedRandom.Next(Info.Interval[0], Info.Interval[1])
				: Info.Interval[0];

			Amount = Info.Amount.Length == 2
				? world.SharedRandom.Next(Info.Amount[0], Info.Amount[1])
				: Info.Amount[0];

			for (var i = 0; i < Amount; i++)
			{
				var tpos = world.Map.CenterOfCell(world.Map.ChooseRandomCell(world.SharedRandom))
					+ new WVec(WDist.Zero, WDist.Zero, Info.Altitude);
				var target = Target.FromPos(tpos);

				var weapon = Info.WeaponInfos.Random(world.SharedRandom);

				var projectileArgs = new ProjectileArgs
				{
					Weapon = weapon,
					Facing = default,
					CurrentMuzzleFacing = () => default,

					DamageModifiers = new int[] { 100 },

					InaccuracyModifiers = new int[] { 100 },

					RangeModifiers = new int[] { 100 },
					Source = tpos,
					CurrentSource = () => tpos,
					SourceActor = firer,
					GuidedTarget = target,
					PassiveTarget = target.CenterPosition
				};

				if (projectileArgs.Weapon.Projectile != null)
				{
					var projectile = projectileArgs.Weapon.Projectile.Create(projectileArgs);
					if (projectile != null)
						world.AddFrameEndTask(w => w.Add(projectile));
				}

				if (weapon.Report != null && weapon.Report.Length > 0)
				{
					if (weapon.AudibleThroughFog || (!self.World.ShroudObscures(tpos) && !self.World.FogObscures(tpos)))
						Game.Sound.Play(SoundType.World, weapon.Report, world, tpos, null, weapon.SoundVolume);
				}
			}
		}
	}
}
