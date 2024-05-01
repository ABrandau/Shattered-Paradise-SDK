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
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits.Render
{
	[Desc("Renders a decorative animation on units and buildings. Overlay switching controlled by " + nameof(PauseOnCondition) + ".")]
	public class WithSwitchableOverlayInfo : PausableConditionalTraitInfo, IRenderActorPreviewSpritesInfo, Requires<RenderSpritesInfo>, Requires<BodyOrientationInfo>
	{
		[Desc("Image used for this decoration. Defaults to the actor's type.")]
		public readonly string Image = null;

		[FieldLoader.Require]
		[SequenceReference(nameof(Image), allowNullImage: true)]
		[Desc("Animation to play when the trait is enabling and disabling.")]
		public readonly string SwitchingSequence = null;

		[FieldLoader.Require]
		[SequenceReference(nameof(Image), allowNullImage: true)]
		[Desc("Animation to play when the trait is enabled")]
		public readonly string EnabledSequence = "idle-overlay";

		[FieldLoader.Require]
		[SequenceReference(nameof(Image), allowNullImage: true)]
		[Desc("Animation to play when the trait is disabled.")]
		public readonly string DisabledSequence = "idle-overlay";

		[Desc("Position relative to body")]
		public readonly WVec Offset = WVec.Zero;

		[PaletteReference(nameof(IsPlayerPalette))]
		[Desc("Custom palette name")]
		public readonly string Palette = null;

		[Desc("Custom palette is a player palette BaseName")]
		public readonly bool IsPlayerPalette = false;

		public readonly bool IsDecoration = false;

		[Desc("Duration in ticks when overlay switching.")]
		public readonly int SwitchingLevel = 20;

		[Desc("Duration in ticks when overlay switching.")]
		public readonly int SwitchingLevelWhenStart = 20;

		public override object Create(ActorInitializer init) { return new WithSwitchableOverlay(init.Self, this); }

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

			anim.PlayRepeating(RenderSprites.NormalizeSequence(anim, init.GetDamageState(), EnabledSequence));

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

	public class WithSwitchableOverlay : PausableConditionalTrait<WithSwitchableOverlayInfo>, ITick, INotifyDamageStateChanged
	{
		readonly Animation overlay;
		int switchingLevel;
		int chargeUnit;
		bool hasSwitched;

		public WithSwitchableOverlay(Actor self, WithSwitchableOverlayInfo info)
			: base(info)
		{
			switchingLevel = info.SwitchingLevelWhenStart;

			var rs = self.Trait<RenderSprites>();
			var body = self.Trait<BodyOrientation>();
			var facing = self.TraitOrDefault<IFacing>();

			var image = info.Image ?? rs.GetImage(self);
			overlay = new Animation(self.World, image, facing == null ? () => WAngle.Zero : (body == null ? () => facing.Facing : () => body.QuantizeFacing(facing.Facing)), () => false)
			{
				IsDecoration = info.IsDecoration
			};

			overlay.PlayRepeating(RenderSprites.NormalizeSequence(overlay, self.GetDamageState(), Info.EnabledSequence));

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

		protected override void TraitPaused(Actor self)
		{
			chargeUnit = -1;
			hasSwitched = true;
			base.TraitPaused(self);
		}

		protected override void TraitResumed(Actor self)
		{
			chargeUnit = 1;
			hasSwitched = true;
			base.TraitResumed(self);
		}

		void ITick.Tick(Actor self)
		{
			if (IsTraitDisabled)
				return;

			switchingLevel += chargeUnit;
			if (switchingLevel > Info.SwitchingLevel && chargeUnit > 0)
			{
				overlay.PlayRepeating(RenderSprites.NormalizeSequence(overlay, self.GetDamageState(), Info.EnabledSequence));
				chargeUnit = 0;
			}
			else if (switchingLevel < 0 && chargeUnit < 0)
			{
				overlay.PlayRepeating(RenderSprites.NormalizeSequence(overlay, self.GetDamageState(), Info.DisabledSequence));
				chargeUnit = 0;
			}
			else if (chargeUnit != 0 && hasSwitched)
			{
				overlay.PlayFetchIndex(RenderSprites.NormalizeSequence(overlay, self.GetDamageState(), Info.SwitchingSequence),
					() => int2.Lerp(0, overlay.CurrentSequence.Length, switchingLevel, Info.SwitchingLevel + 1));
				hasSwitched = false;
			}
		}
	}
}
