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
using OpenRA.Graphics;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.SP.Render
{
	[Desc("Renders a decorative animation on units and buildings.")]
	public class WithRandomIdleOverlayInfo : PausableConditionalTraitInfo, IRenderActorPreviewSpritesInfo, Requires<RenderSpritesInfo>, Requires<BodyOrientationInfo>
	{
		[Desc("Image used for this decoration. Defaults to the actor's type.")]
		public readonly string Image = null;

		[SequenceReference(nameof(Image), allowNullImage: true)]
		[Desc("Sequence name to use")]
		public readonly string[] Sequences = { "idle-overlay" };

		[Desc("Position relative to body")]
		public readonly WVec Offset = WVec.Zero;

		[PaletteReference(nameof(IsPlayerPalette))]
		[Desc("Custom palette name")]
		public readonly string Palette = null;

		[Desc("Custom palette is a player palette BaseName")]
		public readonly bool IsPlayerPalette = false;

		public readonly bool IsDecoration = false;

		public override object Create(ActorInitializer init) { return new WithRandomIdleOverlay(init.Self, this); }

		public IEnumerable<IActorPreview> RenderPreviewSprites(ActorPreviewInitializer init, string image, int facings, PaletteReference p)
		{
			if (!EnabledByDefault)
				yield break;

			if (Palette != null)
				p = init.WorldRenderer.Palette(IsPlayerPalette ? Palette + init.Get<OwnerInit>().InternalName : Palette);

			Func<WAngle> facing;
			var dynamicfacingInit = init.GetOrDefault<DynamicFacingInit>();
			if (dynamicfacingInit != null)
				facing = dynamicfacingInit.Value;
			else
			{
				var f = init.GetValue<FacingInit, WAngle>(WAngle.Zero);
				facing = () => f;
			}

			var anim = new Animation(init.World, Image ?? image, facing)
			{
				IsDecoration = IsDecoration
			};

			anim.PlayRepeating(RenderSprites.NormalizeSequence(anim, init.GetDamageState(), Sequences[0]));

			var body = init.Actor.TraitInfo<BodyOrientationInfo>();
			WRot Orientation() => body.QuantizeOrientation(WRot.FromYaw(facing()), facings);
			WVec Offset() => body.LocalToWorld(this.Offset.Rotate(Orientation()));
			int ZOffset()
			{
				var tmpOffset = Offset();
				return tmpOffset.Y + tmpOffset.Z + 1;
			}

			yield return new SpriteActorPreview(anim, Offset, ZOffset, p);
		}
	}

	public class WithRandomIdleOverlay : PausableConditionalTrait<WithRandomIdleOverlayInfo>, INotifyDamageStateChanged
	{
		readonly Animation overlay;

		public WithRandomIdleOverlay(Actor self, WithRandomIdleOverlayInfo info)
			: base(info)
		{
			var rs = self.Trait<RenderSprites>();
			var body = self.Trait<BodyOrientation>();

			var image = info.Image ?? rs.GetImage(self);
			overlay = new Animation(self.World, image, () => IsTraitPaused)
			{
				IsDecoration = info.IsDecoration
			};

			overlay.PlayRepeating(RenderSprites.NormalizeSequence(overlay, self.GetDamageState(), info.Sequences.Random(self.World.SharedRandom)));

			var anim = new AnimationWithOffset(overlay,
				() => body.LocalToWorld(info.Offset.Rotate(body.QuantizeOrientation(self.Orientation))),
				() => IsTraitDisabled,
				p => RenderUtils.ZOffsetFromCenter(self, p, 1));

			rs.Add(anim, info.Palette, info.IsPlayerPalette);
		}

		void INotifyDamageStateChanged.DamageStateChanged(Actor self, AttackInfo e)
		{
			overlay.ReplaceAnim(RenderSprites.NormalizeSequence(overlay, e.DamageState, overlay.CurrentSequence.Name));
		}
	}
}
