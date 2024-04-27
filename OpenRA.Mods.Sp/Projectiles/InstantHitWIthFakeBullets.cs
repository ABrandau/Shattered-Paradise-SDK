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
using OpenRA.Graphics;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;
using Util = OpenRA.Mods.Common.Util;

namespace OpenRA.Mods.SP.Projectiles
{
	[Desc("Instant and usually direct-on-target projectile, with traces effect like 3rd generation of CNC.")]
	public class InstantHitWIthFakeBulletsInfo : IProjectileInfo
	{
		[Desc("The maximum/constant/incremental inaccuracy used in conjunction with the InaccuracyType property.")]
		public readonly WDist Inaccuracy = WDist.Zero;

		[Desc("Controls the way inaccuracy is calculated. Possible values are 'Maximum' - scale from 0 to max with range, 'PerCellIncrement' - scale from 0 with range and 'Absolute' - use set value regardless of range.")]
		public readonly InaccuracyType InaccuracyType = InaccuracyType.Maximum;

		[Desc("Projectile can be blocked.")]
		public readonly bool Blockable = false;

		[Desc("The width of the projectile.")]
		public readonly WDist Width = new(1);

		[Desc("Scan radius for actors with projectile-blocking trait. If set to a negative value (default), it will automatically scale",
			"to the blocker with the largest health shape. Only set custom values if you know what you're doing.")]
		public readonly WDist BlockerScanRadius = new(-1);

		[Desc("Fake bullet number.")]
		public readonly int FakeBulletNumber = 0;

		[Desc("Fake bullet spawn interval. Only useful when FakeBulletNumber > 0")]
		public readonly int FakeBulletSpawnInterval = 3;

		[Desc("Fake bullet speed")]
		public readonly int FakeBulletSpeed = 1024;

		[Desc("Fake bullet inaccuracy, use set value regardless of range")]
		public readonly WDist FakeBulletInaccuracy = WDist.Zero;

		[Desc("Image to display.")]
		public readonly string Image = null;

		[SequenceReference(nameof(Image), allowNullImage: true)]
		[Desc("Loop sequence of Image from this list while this projectile is moving.")]
		public readonly string Sequence = "idle";

		[Desc("The palette used to draw this projectile.")]
		[PaletteReference(nameof(IsPlayerPalette))]
		public readonly string Palette = "effect";

		public readonly bool IsPlayerPalette = false;

		[Desc("Trail animation.")]
		public readonly string TrailImage = null;

		[Desc("Loop sequence of TrailImage from this list while this projectile is moving.")]
		[SequenceReference(nameof(TrailImage), allowNullImage: true)]
		public readonly string TrailSequence = "idle";

		[Desc("Interval in ticks between each spawned Trail animation.")]
		public readonly int TrailInterval = 2;

		[Desc("Palette used to render the trail sequence.")]
		[PaletteReference(nameof(TrailUsePlayerPalette))]
		public readonly string TrailPalette = "effect";

		[Desc("Use the Player Palette to render the trail sequence.")]
		public readonly bool TrailUsePlayerPalette = false;

		[Desc("When set, display a line behind the fake bullet. Length is measured in ticks after appearing.")]
		public readonly int ContrailLength = 0;

		[Desc("Time (in ticks) after which the line should appear. Controls the distance to the actor.")]
		public readonly int ContrailDelay = 1;

		[Desc("Equivalent to sequence ZOffset. Controls Z sorting.")]
		public readonly int ContrailZOffset = 2047;

		[Desc("Thickness of the emitted line at the start of the contrail.")]
		public readonly WDist ContrailStartWidth = new(64);

		[Desc("Thickness of the emitted line at the end of the contrail. Will default to " + nameof(ContrailStartWidth) + " if left undefined")]
		public readonly WDist? ContrailEndWidth = null;

		[Desc("RGB color at the contrail start.")]
		public readonly Color ContrailStartColor = Color.White;

		[Desc("Use player remap color instead of a custom color at the contrail the start.")]
		public readonly bool ContrailStartColorUsePlayerColor = false;

		[Desc("The alpha value [from 0 to 255] of color at the contrail the start.")]
		public readonly int ContrailStartColorAlpha = 255;

		[Desc("RGB color at the contrail end. Will default to " + nameof(ContrailStartColor) + " if left undefined")]
		public readonly Color? ContrailEndColor;

		[Desc("Use player remap color instead of a custom color at the contrail end.")]
		public readonly bool ContrailEndColorUsePlayerColor = false;

		[Desc("The alpha value [from 0 to 255] of color at the contrail end.")]
		public readonly int ContrailEndColorAlpha = 0;

		public IProjectile Create(ProjectileArgs args) { return new InstantHitWIthFakeBullets(this, args); }
	}

	public class InstantHitWIthFakeBullets : IProjectile
	{
		readonly ProjectileArgs args;
		readonly InstantHitWIthFakeBulletsInfo info;
		readonly FakeBulletWrapper[] fakeBullets;
		readonly World world;
		readonly string animPalette;
		readonly string trailPalette;

		Target target;
		bool notImpacted = true;
		int ticks;
		WPos fakeBulletEndBasePos;

		struct FakeBulletWrapper
		{
			public int Time;
			public int OverallTime;
			public ContrailRenderable Contrail;
			public Animation Anim;
			public WPos EndPos;
			public WPos Pos;
			public WPos SourcePos;
			public WAngle Facing;
		}

		public InstantHitWIthFakeBullets(InstantHitWIthFakeBulletsInfo info, ProjectileArgs args)
		{
			this.args = args;
			this.info = info;
			world = args.SourceActor.World;

			animPalette = info.Palette;
			if (info.IsPlayerPalette)
				animPalette += args.SourceActor.Owner.InternalName;

			trailPalette = info.TrailPalette;
			if (info.TrailUsePlayerPalette)
				trailPalette += args.SourceActor.Owner.InternalName;

			if (info.FakeBulletNumber > 0)
			{
				fakeBullets = new FakeBulletWrapper[info.FakeBulletNumber];
				var startcolor = info.ContrailStartColorUsePlayerColor ? Color.FromArgb(info.ContrailStartColorAlpha, args.SourceActor.Owner.Color) : Color.FromArgb(info.ContrailStartColorAlpha, info.ContrailStartColor);
				var endcolor = info.ContrailEndColorUsePlayerColor ? Color.FromArgb(info.ContrailEndColorAlpha, args.SourceActor.Owner.Color) : Color.FromArgb(info.ContrailEndColorAlpha, info.ContrailEndColor ?? startcolor);
				for (var i = 0; i < fakeBullets.Length; i++)
				{
					var contrail = info.ContrailLength <= 0 ? null : new ContrailRenderable(world, startcolor, endcolor, info.ContrailStartWidth, info.ContrailEndWidth ?? info.ContrailStartWidth, info.ContrailLength, info.ContrailDelay, info.ContrailZOffset);

					fakeBullets[i] = new FakeBulletWrapper
					{
						Time = -1,
						OverallTime = 0,
						Contrail = contrail,
						EndPos = WPos.Zero
					};
				}
			}

			if (args.Weapon.TargetActorCenter)
				target = args.GuidedTarget;
			else if (info.Inaccuracy.Length > 0)
			{
				var maxInaccuracyOffset = Util.GetProjectileInaccuracy(info.Inaccuracy.Length, info.InaccuracyType, args);
				var inaccuracyOffset = WVec.FromPDF(args.SourceActor.World.SharedRandom, 2) * maxInaccuracyOffset / 1024;
				target = Target.FromPos(args.PassiveTarget + inaccuracyOffset);
			}
			else
				target = Target.FromPos(args.PassiveTarget);
		}

		public void Tick(World world)
		{
			ticks++;

			if (notImpacted)
			{
				notImpacted = false;

				// If GuidedTarget has become invalid due to getting killed the same tick,
				// we need to set target to args.PassiveTarget to prevent target.CenterPosition below from crashing.
				if (target.Type == TargetType.Invalid)
					target = Target.FromPos(args.PassiveTarget);

				// Check for blocking actors
				if (info.Blockable && BlocksProjectiles.AnyBlockingActorsBetween(world, args.SourceActor.Owner, args.Source, target.CenterPosition, info.Width, out var blockedPos))
					target = Target.FromPos(blockedPos);

				fakeBulletEndBasePos = target.CenterPosition;

				var warheadArgs = new WarheadArgs(args)
				{
					ImpactOrientation = new WRot(WAngle.Zero, Util.GetVerticalAngle(args.Source, target.CenterPosition), args.Facing),
					ImpactPosition = target.CenterPosition,
				};

				args.Weapon.Impact(target, warheadArgs);
			}

			var allFakeBulletDone = true;

			// Fake bullet generate/render position process
			if (fakeBullets != null)
			{
				for (var i = 0; i < fakeBullets.Length; i++)
				{
					// 1. Generate the bullet when time has come
					if (ticks >= i * info.FakeBulletSpawnInterval && fakeBullets[i].OverallTime == 0 && !args.SourceActor.IsDead)
					{
						allFakeBulletDone = false;

						// Fake bullet contains only renderable objects,
						// it won't affect game logic & network sync so we DON'T use SharedRandom here.
						fakeBullets[i].SourcePos = args.CurrentSource();
						var fakeInaccOffset = WVec.FromPDF(Game.CosmeticRandom, 2) * info.FakeBulletInaccuracy.Length / 1024;
						var vec = fakeBulletEndBasePos + fakeInaccOffset - fakeBullets[i].SourcePos;
						var time = Math.Max(vec.Length / info.FakeBulletSpeed, 1);
						fakeBullets[i].Time = time;
						fakeBullets[i].OverallTime = time;
						fakeBullets[i].Pos = fakeBullets[i].SourcePos;
						fakeBullets[i].EndPos = fakeBulletEndBasePos + fakeInaccOffset;
						fakeBullets[i].Facing = vec.Pitch;

						if (!string.IsNullOrEmpty(info.Image))
						{
							var facing = fakeBullets[i].Facing;
							fakeBullets[i].Anim = new Animation(world, info.Image, () => facing);
							fakeBullets[i].Anim.PlayRepeating(info.Sequence);
						}
					}

					// 2. Process the exsting bullet when it does not expire.
					if (fakeBullets[i].Time > 0 && fakeBullets[i].OverallTime != 0)
					{
						allFakeBulletDone = false;
						var currentPos = WPos.Lerp(fakeBullets[i].SourcePos, fakeBullets[i].EndPos, fakeBullets[i].OverallTime - fakeBullets[i].Time, fakeBullets[i].OverallTime);
						fakeBullets[i].Pos = currentPos;
						fakeBullets[i].Contrail?.Update(currentPos);
						fakeBullets[i].Anim?.Tick();

						if (!string.IsNullOrEmpty(info.TrailImage) && (info.TrailInterval == 0 || ((fakeBullets[i].OverallTime - fakeBullets[i].Time) % info.TrailInterval == 0)))
						{
							var pos = fakeBullets[i].Pos;
							var facing = fakeBullets[i].Facing;
							world.AddFrameEndTask(w => w.Add(new SpriteEffect(pos, facing, world,
								info.TrailImage, info.TrailSequence, trailPalette)));
						}

						if (--fakeBullets[i].Time <= 0 && info.ContrailLength > 0)
						{
							var contrail = fakeBullets[i].Contrail;
							var pos = fakeBullets[i].EndPos;
							world.AddFrameEndTask(w => w.Add(new ContrailFader(pos, contrail)));
						}
					}
				}

				// 3. Process the exsting bullet when it does not expire.
				if (ticks <= (fakeBullets.Length - 1) * info.FakeBulletSpawnInterval)
					allFakeBulletDone = false;
			}

			if (allFakeBulletDone)
				world.AddFrameEndTask(w => w.Remove(this));
		}

		public IEnumerable<IRenderable> Render(WorldRenderer wr)
		{
			if (fakeBullets == null)
				yield break;

			foreach (var c in fakeBullets)
			{
				if (c.Time <= 0)
					yield break;

				if (info.ContrailLength > 0)
					yield return c.Contrail;

				if (c.Anim != null && !world.FogObscures(c.Pos))
				{
					var palette = wr.Palette(animPalette);
					foreach (var r in c.Anim.Render(c.Pos, palette))
						yield return r;
				}
			}
		}
	}
}
