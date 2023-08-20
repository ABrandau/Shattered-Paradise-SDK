#region Copyright & License Information
/*
 * Copyright 2015- OpenRA.Mods.AS Developers (see AUTHORS)
 * This file is a part of a third-party plugin for OpenRA, which is
 * free software. It is made available to you under the terms of the
 * GNU General Public License as published by the Free Software
 * Foundation. For more information, see COPYING.
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
		readonly WeaponWeatherInfo info;

		World world;
		bool firstTick = true;
		Actor firer;

		[Sync]
		public int Interval { get; private set; }

		[Sync]
		public int Amount { get; private set; }

		public WeaponWeather(WeaponWeatherInfo info)
			: base(info)
		{
			this.info = info;
		}

		void ITick.Tick(Actor self)
		{
			if (firstTick)
			{
				world = self.World;
				firer = info.HasOwner ? self.Owner.PlayerActor : world.WorldActor;

				firstTick = false;
				Interval = info.Interval.Length == 2
					? world.SharedRandom.Next(info.Interval[0], info.Interval[1])
					: info.Interval[0];
			}

			if (IsTraitDisabled || --Interval > 0)
				return;

			Interval = info.Interval.Length == 2
				? world.SharedRandom.Next(info.Interval[0], info.Interval[1])
				: info.Interval[0];

			Amount = info.Amount.Length == 2
				? world.SharedRandom.Next(info.Amount[0], info.Amount[1])
				: info.Amount[0];

			for (var i = 0; i < Amount; i++)
			{
				var tpos = world.Map.CenterOfCell(world.Map.ChooseRandomCell(world.SharedRandom))
					+ new WVec(WDist.Zero, WDist.Zero, info.Altitude);
				var target = Target.FromPos(tpos);

				var weapon = info.WeaponInfos.Random(world.SharedRandom);

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

				if (weapon.Report != null && weapon.Report.Any())
				{
					if (weapon.AudibleThroughFog || (!self.World.ShroudObscures(tpos) && !self.World.FogObscures(tpos)))
						Game.Sound.Play(SoundType.World, weapon.Report, world, tpos, null, weapon.SoundVolume);
				}
			}
		}
	}
}
