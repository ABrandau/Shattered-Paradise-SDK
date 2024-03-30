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
using OpenRA.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Activities
{
	public class ChronoTeleportSP : Activity
	{
		readonly Actor chronoProvider;
		readonly int? maximumDistance;
		readonly Dictionary<HashSet<string>, BitSet<DamageType>> terrainsAndDeathTypes = new();
		readonly List<CPos> teleportCells;
		readonly ActorMap actorMap;
		CPos destination;

		public ChronoTeleportSP(Actor chronoProvider, CPos destination, List<CPos> teleportCells, int? maximumDistance,
			bool interruptable = true, Dictionary<HashSet<string>, BitSet<DamageType>> terrainsAndDeathTypes = default)
		{
			ActivityType = ActivityType.Move;
			actorMap = chronoProvider.World.WorldActor.TraitOrDefault<ActorMap>();
			var max = chronoProvider.World.Map.Grid.MaximumTileSearchRange;
			if (maximumDistance > max)
				throw new InvalidOperationException($"Teleport distance cannot exceed the value of MaximumTileSearchRange ({max}).");

			this.chronoProvider = chronoProvider;
			this.destination = destination;
			this.maximumDistance = maximumDistance;
			this.terrainsAndDeathTypes = terrainsAndDeathTypes;
			this.teleportCells = teleportCells;

			if (!interruptable)
				IsInterruptible = false;
		}

		public override bool Tick(Actor self)
		{
			(var bestCell, var damage) = ChooseBestDestinationCell(self, destination);
			if (bestCell == null)
				return true;

			destination = bestCell.Value;

			self.Trait<IPositionable>().SetPosition(self, destination);
			self.Generation++;

			if (damage != null)
				self.Kill(chronoProvider.Owner.PlayerActor, damage.Value);

			return true;
		}

		(CPos? Dest, BitSet<DamageType>? Damage) ChooseBestDestinationCell(Actor self, CPos destination)
		{
			if (chronoProvider == null)
				return (null, null);

			var pos = self.Trait<IPositionable>();
			var map = chronoProvider.World.Map;
			var max = maximumDistance ?? map.Grid.MaximumTileSearchRange;

			// If we teleport a hostile unit, we are going to make it killed if possible within teleport cells
			if (self.Owner.RelationshipWith(chronoProvider.Owner).HasRelationship(PlayerRelationship.Enemy))
			{
				if (!pos.CanEnterCell(destination) && !actorMap.AnyActorsAt(destination) && chronoProvider.Owner.Shroud.IsExplored(destination) && TryGetDamage(map.GetTerrainInfo(destination).Type, out var damage))
					return (destination, damage);

				foreach (var tile in teleportCells)
				{
					if (chronoProvider.Owner.Shroud.IsExplored(tile)
						&& !pos.CanEnterCell(tile) && !actorMap.AnyActorsAt(tile)
						&& TryGetDamage(map.GetTerrainInfo(tile).Type, out var damage2))
						return (tile, damage2);
				}
			}

			// When we cannot find a place to kill it or this is an ally, we make it into somewhere can enter.
			if (pos.CanEnterCell(destination) && chronoProvider.Owner.Shroud.IsExplored(destination))
				return (destination, null);

			foreach (var tile in self.World.Map.FindTilesInCircle(destination, max))
			{
				if (chronoProvider.Owner.Shroud.IsExplored(tile)
					&& pos.CanEnterCell(tile))
					return (tile, null);
			}

			return (null, null);
		}

		bool TryGetDamage(string terrainType, out BitSet<DamageType>? damage)
		{
			foreach (var terrains in terrainsAndDeathTypes.Keys)
				if (terrains.Contains(terrainType))
				{
					damage = terrainsAndDeathTypes[terrains];
					return true;
				}

			damage = null;
			return false;
		}
	}
}
