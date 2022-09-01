#region Copyright & License Information
/*
 * Copyright 2007-2019 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Sp.Traits
{
	public class FirestromSPInfo : ConditionalTraitInfo
	{
		[Desc("List of sounds that can be played on launching.")]
		public readonly string[] LaunchEffectSounds = null;

		[Desc("Interval of launching sound to play.")]
		public readonly int LaunchEffectSoundInterval = 4;

		[Desc("Volume the sound played when activated.")]
		public readonly float LaunchEffectSoundVolume = 1.5f;

		[Desc("Image containing firestorm effect sequence.")]
		public readonly string FirestormEffectImage = null;

		[SequenceReference(nameof(FirestormEffectImage), allowNullImage: true)]
		[Desc("Firestorm sequences to play.")]
		public readonly string[] FirestormEffectSequences = null;

		[Desc("Firestorm effect range.")]
		public readonly WDist FirestormEffectRange = new WDist(12288); // 12 cell

		[Desc("Amount of Firestorm effect.")]
		public readonly int FirestormEffectAmounts = 12; // 12 cell

		[Desc("Firestorm effect rotation angle speed.")]
		public readonly int FirestormEffectRotationAngle = 6;

		[Desc("Damage from firestorm")]
		public readonly int Damage = 200000;

		[Desc("Damage interval from firestorm")]
		public readonly int Damageinterval = 1;

		[PaletteReference]
		[Desc("Palette to use for firestorm effect.")]
		public readonly string FirestormEffectPalette = "effect";

		[Desc("What types of targets are affected.")]
		public readonly BitSet<TargetableType> ValidTargets = new BitSet<TargetableType>("Ground", "Water");

		[Desc("What types of targets are unaffected.", "Overrules ValidTargets.")]
		public readonly BitSet<TargetableType> InvalidTargets;

		[Desc("What player relationships are affected.")]
		public readonly PlayerRelationship ValidRelationships = PlayerRelationship.Ally | PlayerRelationship.Neutral | PlayerRelationship.Enemy;

		[Desc("Types of damage that firestorm causes. Leave empty for no damage types.")]
		public readonly BitSet<DamageType> DamageTypes = default(BitSet<DamageType>);

		[Desc("Damage percentage versus each armor type.")]
		public readonly Dictionary<string, int> Versus = new Dictionary<string, int>();

		[Desc("Damage min range.")]
		public readonly WDist DamageMinRange = new WDist(12032); // 11.75 cell

		[Desc("Damage max range.")]
		public readonly WDist DamageMaxRange = new WDist(12544); // 12.25 cell

		public override object Create(ActorInitializer init) { return new FirestromSP(init.Self, this); }
	}

	public class FirestromSP : ConditionalTrait<FirestromSPInfo>, ITick
	{
		int[] effectsOffet;
		readonly FirestromSPInfo info;
		World world;
		Actor self;
		int damageTicks;
		int soundTicks;

		public FirestromSP(Actor self, FirestromSPInfo info)
			: base(info)
		{
			this.info = info;

			effectsOffet = new int[info.FirestormEffectAmounts];
			var offset = 1024 / info.FirestormEffectAmounts;

			for (var i = 0; i < info.FirestormEffectAmounts; i++)
				effectsOffet[i] = i * offset;

			this.self = self;
			world = self.World;
		}

		void SpawnEffects()
		{
			for (var i = 0; i < info.FirestormEffectAmounts; i++)
			{
				var sequence = info.FirestormEffectSequences.RandomOrDefault(world.LocalRandom);
				if (sequence == null)
					break;
				var rotation = WRot.FromYaw(new WAngle(effectsOffet[i]));
				var targetpos = self.CenterPosition + new WVec(info.FirestormEffectRange.Length, 0, 0).Rotate(rotation);
				world.AddFrameEndTask(w => w.Add(new SpriteEffect(new WPos(targetpos.X, targetpos.Y, world.Map.CenterOfCell(world.Map.CellContaining(targetpos)).Z), w, info.FirestormEffectImage, sequence, info.FirestormEffectPalette)));

				effectsOffet[i] = effectsOffet[i] + info.FirestormEffectRotationAngle < 1024 ? effectsOffet[i] + info.FirestormEffectRotationAngle : (effectsOffet[i] + info.FirestormEffectRotationAngle) % 1024;
			}
		}

		void DealDamage()
		{
			var victims = world.FindActorsInCircle(self.CenterPosition, info.DamageMaxRange).Where(
				a => info.ValidRelationships.HasRelationship(self.Owner.RelationshipWith(a.Owner))
				&& (self.CenterPosition - a.CenterPosition).HorizontalLengthSquared >= info.DamageMinRange.LengthSquared); // this line has bug

			foreach (var v in victims)
			{
				// Cannot be damaged without a Health trait
				if (!v.Info.HasTraitInfo<IHealthInfo>())
					continue;

				var targetTypes = v.GetEnabledTargetTypes();

				if (info.ValidTargets.Overlaps(targetTypes) && !info.InvalidTargets.Overlaps(targetTypes))
				{
					if (info.Versus.Count == 0)
						continue;

					var closestActiveShape = v.TraitsImplementing<HitShape>().Where(Exts.IsTraitEnabled).MinByOrDefault(t => t.DistanceFromEdge(v, v.CenterPosition));

					// Cannot be damaged without an active HitShape
					if (closestActiveShape == null)
						continue;

					var armor = v.TraitsImplementing<Armor>()
						.Where(a => !a.IsTraitDisabled && a.Info.Type != null && info.Versus.ContainsKey(a.Info.Type) &&
							(closestActiveShape.Info.ArmorTypes.IsEmpty || closestActiveShape.Info.ArmorTypes.Contains(a.Info.Type)))
						.Select(a => info.Versus[a.Info.Type]);

					var damage = Common.Util.ApplyPercentageModifiers(info.Damage, armor);
					v.InflictDamage(self, new Damage(damage, info.DamageTypes));
				}
			}
		}

		protected override void TraitEnabled(Actor self)
		{
			damageTicks = 0;
			soundTicks = 0;
		}

		public void Tick(Actor self)
		{
			if (IsTraitDisabled)
				return;

			if (info.FirestormEffectImage != null)
				SpawnEffects();

			if (damageTicks-- <= 0)
			{
				DealDamage();
				damageTicks = info.Damageinterval;
			}

			if (soundTicks-- <= 0)
			{
				var launchSound = info.LaunchEffectSounds.RandomOrDefault(world.LocalRandom);
				if (launchSound != null && ((!world.ShroudObscures(self.CenterPosition) && !world.FogObscures(self.CenterPosition))))
					Game.Sound.Play(SoundType.World, launchSound, self.CenterPosition, info.LaunchEffectSoundVolume);
				soundTicks = info.LaunchEffectSoundInterval;
			}
		}
	}
}
