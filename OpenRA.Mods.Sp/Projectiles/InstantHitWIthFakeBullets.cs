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
	[Desc("Instant and usually direct-on-target projectile, with traces effect.")]
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

		[Desc("When set, display a line behind the actor. Length is measured in ticks after appearing.")]
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
		readonly FakeBulletWrapper[] contrails;
		readonly World world;

		Target target;
		bool notImpacted = true;
		int ticks;
		WPos fakeBulletEndBasePos;

		struct FakeBulletWrapper
		{
			public int Time;
			public int OverallTime;
			public ContrailRenderable Contrail;
			public WPos EndPos;
			public WPos SourcePos;
		}

		public InstantHitWIthFakeBullets(InstantHitWIthFakeBulletsInfo info, ProjectileArgs args)
		{
			this.args = args;
			this.info = info;
			world = args.SourceActor.World;

			if (info.FakeBulletNumber > 0 && info.ContrailLength > 0)
			{
				contrails = new FakeBulletWrapper[info.FakeBulletNumber];
				var startcolor = info.ContrailStartColorUsePlayerColor ? Color.FromArgb(info.ContrailStartColorAlpha, args.SourceActor.Owner.Color) : Color.FromArgb(info.ContrailStartColorAlpha, info.ContrailStartColor);
				var endcolor = info.ContrailEndColorUsePlayerColor ? Color.FromArgb(info.ContrailEndColorAlpha, args.SourceActor.Owner.Color) : Color.FromArgb(info.ContrailEndColorAlpha, info.ContrailEndColor ?? startcolor);
				for (var i = 0; i < contrails.Length; i++)
				{
					var contrail = new ContrailRenderable(world, startcolor, endcolor, info.ContrailStartWidth, info.ContrailEndWidth ?? info.ContrailStartWidth, info.ContrailLength, info.ContrailDelay, info.ContrailZOffset);
					contrails[i] = new FakeBulletWrapper
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

			var allContrailDone = true;

			// Contrail generate/render position process
			if (contrails != null)
			{
				for (var i = 0; i < contrails.Length; i++)
				{
					if (ticks >= i * info.FakeBulletSpawnInterval && contrails[i].OverallTime == 0 && args.SourceActor.IsInWorld && !args.SourceActor.IsDead)
					{
						contrails[i].SourcePos = args.CurrentSource();

						// Contrail render won't affect network sync so we DON'T use SharedRandom here.
						var fakeInaccOffset = WVec.FromPDF(Game.CosmeticRandom, 2) * info.FakeBulletInaccuracy.Length / 1024;
						var time = Math.Max((fakeBulletEndBasePos + fakeInaccOffset - contrails[i].SourcePos).Length / info.FakeBulletSpeed, 1);
						contrails[i].Time = time;
						contrails[i].OverallTime = time;
						contrails[i].EndPos = fakeBulletEndBasePos + fakeInaccOffset;
						allContrailDone = false;
					}

					if (contrails[i].Time > 0 && contrails[i].OverallTime != 0)
					{
						contrails[i].Contrail.Update(WPos.Lerp(contrails[i].SourcePos, contrails[i].EndPos, contrails[i].OverallTime - contrails[i].Time, contrails[i].OverallTime));
						allContrailDone = false;
						if (--contrails[i].Time <= 0)
						{
							var contrail = contrails[i].Contrail;
							var pos = contrails[i].EndPos;
							world.AddFrameEndTask(w => w.Add(new ContrailFader(pos, contrail)));
						}
					}
				}

				if (ticks <= (contrails.Length - 1) * info.FakeBulletSpawnInterval)
					allContrailDone = false;
			}

			if (allContrailDone)
				world.AddFrameEndTask(w => w.Remove(this));
		}

		public IEnumerable<IRenderable> Render(WorldRenderer wr)
		{
			if (contrails != null)
			{
				foreach (var c in contrails)
					if (c.Time > 0)
						yield return c.Contrail;
			}
		}
	}
}
