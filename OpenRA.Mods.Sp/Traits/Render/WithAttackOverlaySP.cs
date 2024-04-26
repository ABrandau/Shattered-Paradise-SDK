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

using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits.Render
{
	[Desc("Rendered together with an attack with specific armament.")]
	public class WithAttackOverlaySPInfo : TraitInfo, Requires<RenderSpritesInfo>
	{
		[Desc("Armament name")]
		public readonly string Armament = "primary";

		[SequenceReference]
		[FieldLoader.Require]
		[Desc("Sequence name to use")]
		public readonly string Sequence = null;

		[PaletteReference(nameof(IsPlayerPalette))]
		[Desc("Custom palette name")]
		public readonly string Palette = null;

		[Desc("Custom palette is a player palette BaseName")]
		public readonly bool IsPlayerPalette = false;

		[Desc("Delay in ticks before overlay starts, either relative to attack preparation or attack.")]
		public readonly int Delay = 0;

		[Desc("Should the overlay be delayed relative to preparation or actual attack?")]
		public readonly AttackDelayType DelayRelativeTo = AttackDelayType.Preparation;

		public override object Create(ActorInitializer init) { return new WithAttackOverlaySP(init, this); }
	}

	public class WithAttackOverlaySP : INotifyAttack, ITick
	{
		readonly Animation overlay;
		readonly RenderSprites renderSprites;
		readonly WithAttackOverlaySPInfo info;

		bool attacking;
		int tick;

		public WithAttackOverlaySP(ActorInitializer init, WithAttackOverlaySPInfo info)
		{
			this.info = info;

			renderSprites = init.Self.Trait<RenderSprites>();
			var body = init.Self.TraitOrDefault<BodyOrientation>();
			var facing = init.Self.TraitOrDefault<IFacing>();

			overlay = new Animation(init.World, renderSprites.GetImage(init.Self), facing == null || body == null ? () => WAngle.Zero : () => body.QuantizeFacing(facing.Facing));

			renderSprites.Add(new AnimationWithOffset(overlay, null, () => !attacking, p => RenderUtils.ZOffsetFromCenter(init.Self, p, 1)),
				info.Palette, info.IsPlayerPalette);
		}

		void PlayOverlay()
		{
			attacking = true;
			overlay.PlayThen(info.Sequence, () => attacking = false);
		}

		void INotifyAttack.Attacking(Actor self, in Target target, Armament a, Barrel barrel)
		{
			if (info.DelayRelativeTo == AttackDelayType.Attack && info.Armament == a.Info.Name)
			{
				if (info.Delay > 0)
					tick = info.Delay;
				else
					PlayOverlay();
			}
		}

		void INotifyAttack.PreparingAttack(Actor self, in Target target, Armament a, Barrel barrel)
		{
			if (info.DelayRelativeTo == AttackDelayType.Preparation && info.Armament == a.Info.Name)
			{
				if (info.Delay > 0)
					tick = info.Delay;
				else
					PlayOverlay();
			}
		}

		void ITick.Tick(Actor self)
		{
			if (info.Delay > 0 && --tick == 0)
				PlayOverlay();
		}
	}
}
