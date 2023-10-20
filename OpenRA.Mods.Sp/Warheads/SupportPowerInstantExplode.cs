#region Copyright & License Information
/*
 * Copyright 2015- OpenRA.Mods.AS Developers (see AUTHORS)
 * This file is a part of a third-party plugin for OpenRA, which is
 * free software. It is made available to you under the terms of the
 * GNU General Public License as published by the Free Software
 * Foundation. For more information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using OpenRA.GameRules;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Projectiles
{
	[Desc("Hack: Used for SupportPower actor which use Armament as support power." +
		"Bind damage warhead to it to prevent unit and AI player return fire at this actor.")]
	public class SupportPowerInstantExplodeInfo : IProjectileInfo
	{
		public IProjectile Create(ProjectileArgs args) { return new SupportPowerInstantExplode(args); }
	}

	class SupportPowerInstantExplode : IProjectile
	{
		readonly ProjectileArgs args;

		public SupportPowerInstantExplode(ProjectileArgs args)
		{
			this.args = args;
		}

		public void Tick(World world)
		{
			world.AddFrameEndTask(w => w.Remove(this));

			var firer = args.SourceActor;
			if (firer != null && firer.Owner != null)
				firer = firer.Owner.PlayerActor;

			var warheadArgs = new WarheadArgs(args)
			{
				SourceActor = firer,
				ImpactOrientation = new WRot(WAngle.Zero, WAngle.Zero, args.CurrentMuzzleFacing()),
				ImpactPosition = args.Source,
			};

			args.Weapon.Impact(Target.FromPos(args.Source), warheadArgs);
		}

		public IEnumerable<IRenderable> Render(WorldRenderer wr)
		{
			yield break;
		}
	}
}
