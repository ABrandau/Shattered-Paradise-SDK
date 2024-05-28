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
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits.Render
{
	[Desc("Play an animation when a unit exits or blocks the exit after production finished.")]
	sealed class WithProductionDoorOverlaySPInfo : ConditionalTraitInfo, IRenderActorPreviewSpritesInfo, Requires<RenderSpritesInfo>, Requires<BodyOrientationInfo>, Requires<BuildingInfo>
	{
		[SequenceReference]
		public readonly string Sequence = "build-door";

		[PaletteReference(nameof(IsPlayerPalette))]
		[Desc("The palette used for `Sequence`.")]
		public readonly string Palette = null;

		[Desc("Custom death animation palette is a player palette BaseName")]
		public readonly bool IsPlayerPalette = false;

		public readonly int ZOffsetModifierWhenProduce = 50;

		public IEnumerable<IActorPreview> RenderPreviewSprites(ActorPreviewInitializer init, string image, int facings, PaletteReference p)
		{
			var anim = new Animation(init.World, image);
			anim.PlayFetchIndex(RenderSprites.NormalizeSequence(anim, init.GetDamageState(), Sequence), () => 0);

			var bi = init.Actor.TraitInfo<BuildingInfo>();
			var offset = bi.CenterOffset(init.World).Y + ZOffsetModifierWhenProduce;
			yield return new SpriteActorPreview(anim, () => WVec.Zero, () => offset, p);
		}

		public override object Create(ActorInitializer init) { return new WithProductionDoorOverlaySP(init.Self, this); }
	}

	sealed class WithProductionDoorOverlaySP : ConditionalTrait<WithProductionDoorOverlaySPInfo>, ITick, INotifyProduction, INotifyDamageStateChanged
	{
		readonly Animation door;
		readonly Func<Actor, bool> validExitingActor;
		int desiredFrame;
		CPos openExit;
		Actor exitingActor;

		public WithProductionDoorOverlaySP(Actor self, WithProductionDoorOverlaySPInfo info)
			: base(info)
		{
			var renderSprites = self.Trait<RenderSprites>();
			validExitingActor = a => a.IsInWorld && a.Location == openExit && (a.CurrentActivity?.ActivitiesImplementing<Mobile.ReturnToCellActivity>().Any() ?? false);

			door = new Animation(self.World, renderSprites.GetImage(self));
			door.PlayFetchDirection(RenderSprites.NormalizeSequence(door, self.GetDamageState(), info.Sequence),
				() => desiredFrame - door.CurrentFrame);

			var anim = new AnimationWithOffset(door, null, () => IsTraitDisabled,
				p => RenderUtils.ZOffsetFromCenter(self, p, 1) + (exitingActor != null && validExitingActor(exitingActor) ? Info.ZOffsetModifierWhenProduce : 1));
			if (string.IsNullOrEmpty(info.Palette))
				renderSprites.Add(anim);
			else
				renderSprites.Add(anim, info.Palette, info.IsPlayerPalette);
		}

		void ITick.Tick(Actor self)
		{
			if (exitingActor == null)
				return;

			if (!validExitingActor(exitingActor))
			{
				desiredFrame = 0;
				exitingActor = null;
			}
		}

		void INotifyDamageStateChanged.DamageStateChanged(Actor self, AttackInfo e)
		{
			if (door.CurrentSequence != null)
				door.ReplaceAnim(RenderSprites.NormalizeSequence(door, e.DamageState, door.CurrentSequence.Name));
		}

		void INotifyProduction.UnitProduced(Actor self, Actor other, CPos exit)
		{
			if (other.TraitOrDefault<IPositionable>() == null)
				return;

			openExit = exit;
			exitingActor = other;
			desiredFrame = door.CurrentSequence.Length - 1;
		}
	}
}
