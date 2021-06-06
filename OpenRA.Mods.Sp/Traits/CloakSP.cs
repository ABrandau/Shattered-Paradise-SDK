#region Copyright & License Information
/*
 * Copyright 2007-2020 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Sp.Traits
{
	[Desc("This unit can cloak and uncloak in specific situations.")]
	public class CloakSPInfo : CloakInfo
	{
		[Desc("Which image to use for effect when enter/exit cloak.")]
		public readonly string EffectImage = null;

		[Desc("Which sequence to use for effect when enter cloak.")]
		[SequenceReference(nameof(EffectImage), allowNullImage: true)]
		public readonly string EnterEffectSequence = null;

		[Desc("Which sequence to use for effect when exit cloak..")]
		[SequenceReference(nameof(EffectImage), allowNullImage: true)]
		public readonly string ExitEffectSequence = null;

		[PaletteReference(nameof(IsPlayerPalette))]
		public readonly string EffectPalette = "effect";
		public readonly bool IsEffectPlayerPalette = false;

		[Desc("Offset for effect when enter/exit cloak.")]
		public readonly WVec EffectOffset = WVec.Zero;

		public override object Create(ActorInitializer init) { return new CloakSP(this); }
	}

	public class CloakSP : PausableConditionalTrait<CloakSPInfo>, IRenderModifier, INotifyDamage, INotifyUnload, INotifyDemolition, INotifyInfiltration,
		INotifyAttack, ITick, IVisibilityModifier, IRadarColorModifier, INotifyCreated, INotifyHarvesterAction, INotifyBeingResupplied
	{
		[Sync]
		int remainingTime;

		bool isDocking;

		CPos? lastPos;
		bool wasCloaked = false;
		bool firstTick = true;
		int cloakedToken = Actor.InvalidConditionToken;

		public CloakSP(CloakSPInfo info)
			: base(info)
		{
			remainingTime = info.InitialDelay;
		}

		protected override void Created(Actor self)
		{
			if (Cloaked)
			{
				wasCloaked = true;
				if (cloakedToken == Actor.InvalidConditionToken)
					cloakedToken = self.GrantCondition(Info.CloakedCondition);
			}

			base.Created(self);
		}

		public bool Cloaked => !IsTraitDisabled && !IsTraitPaused && remainingTime <= 0;

		public void Uncloak() { Uncloak(Info.CloakDelay); }

		public void Uncloak(int time) { remainingTime = Math.Max(remainingTime, time); }

		void INotifyAttack.Attacking(Actor self, in Target target, Armament a, Barrel barrel) { if (Info.UncloakOn.HasFlag(UncloakType.Attack)) Uncloak(); }

		void INotifyAttack.PreparingAttack(Actor self, in Target target, Armament a, Barrel barrel) { }

		void INotifyDamage.Damaged(Actor self, AttackInfo e)
		{
			if (e.Damage.Value == 0)
				return;

			var type = e.Damage.Value < 0
				? (e.Attacker == self ? UncloakType.SelfHeal : UncloakType.Heal)
				: UncloakType.Damage;
			if (Info.UncloakOn.HasFlag(type))
				Uncloak();
		}

		IEnumerable<IRenderable> IRenderModifier.ModifyRender(Actor self, WorldRenderer wr, IEnumerable<IRenderable> r)
		{
			if (remainingTime > 0 || IsTraitDisabled || IsTraitPaused)
				return r;

			if (Cloaked && IsVisible(self, self.World.RenderPlayer))
			{
				var palette = string.IsNullOrEmpty(Info.Palette) ? null : Info.IsPlayerPalette ? wr.Palette(Info.Palette + self.Owner.InternalName) : wr.Palette(Info.Palette);
				if (palette == null)
					return r;
				else
					return r.Select(a => !a.IsDecoration && a is IPalettedRenderable ? ((IPalettedRenderable)a).WithPalette(palette) : a);
			}
			else
				return SpriteRenderable.None;
		}

		IEnumerable<Rectangle> IRenderModifier.ModifyScreenBounds(Actor self, WorldRenderer wr, IEnumerable<Rectangle> bounds)
		{
			return bounds;
		}

		void ITick.Tick(Actor self)
		{
			if (!IsTraitDisabled && !IsTraitPaused)
			{
				if (remainingTime > 0 && !isDocking)
					remainingTime--;

				if (Info.UncloakOn.HasFlag(UncloakType.Move) && (lastPos == null || lastPos.Value != self.Location))
				{
					Uncloak();
					lastPos = self.Location;
				}
			}

			var isCloaked = Cloaked;
			if (isCloaked && !wasCloaked)
			{
				if (cloakedToken == Actor.InvalidConditionToken)
					cloakedToken = self.GrantCondition(Info.CloakedCondition);

				// Sounds shouldn't play if the actor starts cloaked
				if (!(firstTick && Info.InitialDelay == 0))
				{
					var pos = self.CenterPosition;
					if (Info.AudibleThroughFog || (!self.World.ShroudObscures(pos) && !self.World.FogObscures(pos)))
						Game.Sound.Play(SoundType.World, Info.CloakSound, pos, Info.SoundVolume);

					if (Info.EffectImage != null && Info.EnterEffectSequence != null)
					{
						self.World.AddFrameEndTask(w => w.Add(new SpriteEffect(
							pos + Info.EffectOffset, w, Info.EffectImage, Info.EnterEffectSequence, Info.EffectPalette)));
					}
				}
			}
			else if (!isCloaked && wasCloaked)
			{
				if (cloakedToken != Actor.InvalidConditionToken)
					cloakedToken = self.RevokeCondition(cloakedToken);

				if (!(firstTick && Info.InitialDelay == 0))
				{
					var pos = self.CenterPosition;
					if (Info.AudibleThroughFog || (!self.World.ShroudObscures(pos) && !self.World.FogObscures(pos)))
						Game.Sound.Play(SoundType.World, Info.UncloakSound, pos, Info.SoundVolume);

					if (Info.EffectImage != null && Info.ExitEffectSequence != null)
					{
						self.World.AddFrameEndTask(w => w.Add(new SpriteEffect(
							pos + Info.EffectOffset, w, Info.EffectImage, Info.ExitEffectSequence, Info.EffectPalette)));
					}
				}
			}

			wasCloaked = isCloaked;
			firstTick = false;
		}

		protected override void TraitEnabled(Actor self)
		{
			remainingTime = Info.InitialDelay;
		}

		protected override void TraitDisabled(Actor self) { Uncloak(); }

		public bool IsVisible(Actor self, Player viewer)
		{
			if (!Cloaked || self.Owner.IsAlliedWith(viewer))
				return true;

			return self.World.ActorsWithTrait<DetectCloaked>().Any(a => a.Actor.Owner.IsAlliedWith(viewer)
				&& Info.CloakTypes.Overlaps(a.Trait.Info.CloakTypes)
				&& (self.CenterPosition - a.Actor.CenterPosition).LengthSquared <= a.Trait.Range.LengthSquared);
		}

		Color IRadarColorModifier.RadarColorOverride(Actor self, Color color)
		{
			if (self.Owner == self.World.LocalPlayer && Cloaked)
				color = Color.FromArgb(128, color);

			return color;
		}

		void INotifyHarvesterAction.MovingToResources(Actor self, CPos targetCell) { }

		void INotifyHarvesterAction.MovingToRefinery(Actor self, Actor refineryActor, bool forceDelivery) { }

		void INotifyHarvesterAction.MovementCancelled(Actor self) { }

		void INotifyHarvesterAction.Harvested(Actor self, string resourceType) { }

		void INotifyHarvesterAction.Docked()
		{
			if (Info.UncloakOn.HasFlag(UncloakType.Dock))
			{
				isDocking = true;
				Uncloak();
			}
		}

		void INotifyHarvesterAction.Undocked()
		{
			isDocking = false;
		}

		void INotifyUnload.Unloading(Actor self)
		{
			if (Info.UncloakOn.HasFlag(UncloakType.Unload))
				Uncloak();
		}

		void INotifyDemolition.Demolishing(Actor self)
		{
			if (Info.UncloakOn.HasFlag(UncloakType.Demolish))
				Uncloak();
		}

		void INotifyInfiltration.Infiltrating(Actor self)
		{
			if (Info.UncloakOn.HasFlag(UncloakType.Infiltrate))
				Uncloak();
		}

		void INotifyBeingResupplied.StartingResupply(Actor self, Actor host)
		{
			if (Info.UncloakOn.HasFlag(UncloakType.Dock))
			{
				isDocking = true;
				Uncloak();
			}
		}

		void INotifyBeingResupplied.StoppingResupply(Actor self, Actor host)
		{
			if (Info.UncloakOn.HasFlag(UncloakType.Dock))
				isDocking = false;
		}
	}
}
