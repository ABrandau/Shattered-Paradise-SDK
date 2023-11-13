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
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Warheads;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Warheads
{
	public class ScrinEssenceHitWarhead : Warhead, IRulesetLoaded<WeaponInfo>
	{
		[WeaponReference]
		[FieldLoader.Require]
		[Desc("Has to be defined in weapons.yaml as well.")]
		public readonly string Weapon = null;

		public readonly string WeaponName = "primary";

		[Desc("Target types that will not be considered first.")]
		public readonly BitSet<TargetableType> SecondaryTargets = default;

		[Desc("Amount of shrapnels thrown.")]
		public readonly int Amount = 1;

		[Desc("What diplomatic stances can be targeted by the shrapnel.")]
		public readonly PlayerRelationship AimTargetStances = PlayerRelationship.Ally | PlayerRelationship.Neutral | PlayerRelationship.Enemy;

		[Desc("Should the weapons be fired around the intended target or at the explosion's epicenter.")]
		public readonly bool AroundTarget = false;

		WeaponInfo weapon;

		public void RulesetLoaded(Ruleset rules, WeaponInfo info)
		{
			if (!rules.Weapons.TryGetValue(Weapon.ToLowerInvariant(), out weapon))
				throw new YamlException($"Weapons Ruleset does not contain an entry '{Weapon.ToLowerInvariant()}'");
		}

		public override void DoImpact(in Target target, WarheadArgs args)
		{
			var firedBy = args.SourceActor;
			if (!target.IsValidFor(firedBy))
				return;

			var world = firedBy.World;
			var map = world.Map;

			var epicenter = AroundTarget && args.WeaponTarget.Type != TargetType.Invalid
				? args.WeaponTarget.CenterPosition
				: target.CenterPosition;

			var availableTargetActors = world.FindActorsOnCircle(epicenter, weapon.Range)
				.Where(x =>
				{
					if (!weapon.IsValidAgainst(Target.FromActor(x), firedBy.World, firedBy) || !AimTargetStances.HasRelationship(firedBy.Owner.RelationshipWith(x.Owner)))
						return false;

					var activeShapes = x.TraitsImplementing<HitShape>().Where(Exts.IsTraitEnabled);
					if (!activeShapes.Any())
						return false;

					var distance = activeShapes.Min(t => t.DistanceFromEdge(x, epicenter));

					if (distance < weapon.Range)
						return true;

					return false;
				}).ToArray();

			var preferedTargetActors = availableTargetActors.Where(x => !SecondaryTargets.Overlaps(x.GetEnabledTargetTypes())).Shuffle(world.SharedRandom).ToList();

			var amount = 0;
			for (; amount < Amount && amount < preferedTargetActors.Count; amount++)
				GenerateWeapon(firedBy, preferedTargetActors[amount], epicenter, target);

			if (Amount <= amount)
				return;
			else
				amount = Amount - amount;

			var otherTargetActors = availableTargetActors.Where(x => SecondaryTargets.Overlaps(x.GetEnabledTargetTypes())).Shuffle(world.SharedRandom).ToList();
			for (var i = 0; i < amount && i < otherTargetActors.Count; i++)
				GenerateWeapon(firedBy, otherTargetActors[i], epicenter, target);
		}

		void GenerateWeapon(Actor firedBy, Actor victim, WPos epicenter, in Target target)
		{
			var shrapnelTarget = Target.FromActor(victim);

			if (shrapnelTarget.Type == TargetType.Invalid)
				return;

			var shrapnelFacing = (shrapnelTarget.CenterPosition - epicenter).Yaw;

			// Lambdas can't use 'in' variables, so capture a copy for later
			var centerPosition = target.CenterPosition;

			var projectileArgs = new ProjectileArgs
			{
				Weapon = weapon,
				Facing = shrapnelFacing,
				CurrentMuzzleFacing = () => shrapnelFacing,

				DamageModifiers = !firedBy.IsDead ? firedBy.TraitsImplementing<IFirepowerModifier>()
					.Select(a => a.GetFirepowerModifier(WeaponName)).ToArray() : Array.Empty<int>(),

				InaccuracyModifiers = Array.Empty<int>(),

				RangeModifiers = Array.Empty<int>(),

				Source = target.CenterPosition,
				CurrentSource = () => centerPosition,
				SourceActor = firedBy,
				GuidedTarget = shrapnelTarget,
				PassiveTarget = shrapnelTarget.CenterPosition
			};

			if (projectileArgs.Weapon.Projectile != null)
			{
				var projectile = projectileArgs.Weapon.Projectile.Create(projectileArgs);
				if (projectile != null)
					firedBy.World.AddFrameEndTask(w => w.Add(projectile));

				if (projectileArgs.Weapon.Report != null && projectileArgs.Weapon.Report.Length > 0)
				{
					var pos = target.CenterPosition;
					if (projectileArgs.Weapon.AudibleThroughFog || (!firedBy.World.ShroudObscures(pos) && !firedBy.World.FogObscures(pos)))
						Game.Sound.Play(SoundType.World, projectileArgs.Weapon.Report, firedBy.World, pos, null, projectileArgs.Weapon.SoundVolume);
				}
			}
		}
	}
}
