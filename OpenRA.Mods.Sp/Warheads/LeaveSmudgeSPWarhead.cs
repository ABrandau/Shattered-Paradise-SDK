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
using OpenRA.GameRules;
using OpenRA.Mods.Common.Warheads;
using OpenRA.Mods.SP.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Warheads
{
	[Desc("Creates a smudge in `SmudgeLayer`.")]
	public class LeaveSmudgeSPWarhead : Warhead
	{
		[Desc("Size of the area. A smudge will be created in each tile.", "Provide 2 values for a ring effect (outer/inner).")]
		public readonly int[] Size = { 0, 0 };

		[Desc("Type of smudge to apply to terrain.")]
		public readonly HashSet<string> SmudgeType = new();

		[Desc("The smudge increase by this level.")]
		public readonly int AddSmudgeLevel = 1;

		[Desc("The smudge is created by this level.")]
		public readonly int[] InitialSmudgeLevel = { 0, 1 };

		[Desc("The Max smudge level can reach.If it is lower than InitialSmudgeLevel, then it will equal to InitialSmudgeLevel.")]
		public readonly int MaxSmudgeLevel = 6;

		public override void DoImpact(in Target target, WarheadArgs args)
		{
			if (target.Type == TargetType.Invalid)
				return;

			var firedBy = args.SourceActor;
			var world = firedBy.World;

			var pos = target.CenterPosition;
			var dat = world.Map.DistanceAboveTerrain(pos);

			if (dat > AirThreshold)
				return;

			var targetTile = world.Map.CellContaining(pos);
			var smudgeLayer = world.WorldActor.TraitsImplementing<SmudgeLayerSP>().FirstOrDefault();

			if (smudgeLayer == null)
				return;

			var minRange = (Size.Length > 1 && Size[1] > 0) ? Size[1] : 0;
			var allCells = world.Map.FindTilesInAnnulus(targetTile, minRange, Size[0]);

			// Draw the smudges:
			foreach (var sc in allCells)
			{
				var smudgeType = world.Map.GetTerrainInfo(sc).AcceptsSmudgeType.FirstOrDefault(SmudgeType.Contains);
				if (smudgeType == null)
					continue;

				var cellActors = world.ActorMap.GetActorsAt(sc);
				if (cellActors.Any(a => !IsValidAgainst(a, firedBy)))
					continue;

				var initialLevel = InitialSmudgeLevel.Length > 1 ? Game.CosmeticRandom.Next(InitialSmudgeLevel[0], InitialSmudgeLevel[1] + 1) : InitialSmudgeLevel[0];
				smudgeLayer.AddSmudge(sc, smudgeType, initialLevel, AddSmudgeLevel, Math.Max(MaxSmudgeLevel, initialLevel));
			}
		}
	}
}
