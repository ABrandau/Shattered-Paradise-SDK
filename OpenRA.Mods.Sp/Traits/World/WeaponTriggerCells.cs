﻿using System.Collections.Generic;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Effects;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.TA.Traits
{
	[TraitLocation(SystemActors.World | SystemActors.EditorWorld)]
	public class WeaponTriggerCellsInfo : TraitInfo
	{
		[Desc("Name of the layer type")]
		public readonly string Name = "";

		[Desc("Speed of restore per tick to 0")]
		public readonly int RestorePerTick = 4;

		[Desc("Show debug overlay of this WeaponTriggerCells")]
		public readonly bool ShowDebugOverlay = false;

		public override object Create(ActorInitializer init) { return new WeaponTriggerCells(init.Self, this); }
	}

	class TriggerCell
	{
		public int Level;
	}

	public class WeaponTriggerCells : INotifyActorDisposing, ITick, ITickRender
	{
		readonly World world;
		public readonly WeaponTriggerCellsInfo Info;

		readonly Dictionary<CPos, TriggerCell> tiles = new Dictionary<CPos, TriggerCell>();

		public WeaponTriggerCells(Actor self, WeaponTriggerCellsInfo info)
		{
			world = self.World;
			Info = info;
		}

		void ITick.Tick(Actor self)
		{
			var remove = new List<CPos>();

			// Apply half life to each cell.
			foreach (var kv in tiles)
			{
				// has to be decreased by at least 1 so that it disappears eventually.
				var level = kv.Value.Level;
				if (level > 0)
				{
					if ((level -= Info.RestorePerTick) <= 0)
						remove.Add(kv.Key);
					else
						kv.Value.Level = level;
				}
				else
				{
					if ((level += Info.RestorePerTick) >= 0)
						remove.Add(kv.Key);
					else
						kv.Value.Level = level;
				}
			}

			foreach (var r in remove)
				tiles.Remove(r);
		}

		public int GetLevel(CPos cell)
		{
			if (!tiles.ContainsKey(cell))
				return 0;

			return tiles[cell].Level;
		}

		public void SetLevel(CPos cell, int level)
		{
			if (!tiles.ContainsKey(cell))
				return;

			tiles[cell].Level = level;
		}

		public void IncreaseLevel(CPos cell, int add_level, int max_level)
		{
			if (add_level == 0)
				return;

			var currentLevel = 0;

			// Initialize, on fresh impact.
			if (tiles.ContainsKey(cell))
				currentLevel = tiles[cell].Level;

			// the given weapon can't saturate the cell anymore.
			if ((add_level > 0 && currentLevel >= max_level) ||
				(add_level < 0 && currentLevel <= max_level))
				return;

			var new_level = currentLevel + add_level;

			if ((add_level > 0 && new_level > max_level) ||
				(add_level < 0 && new_level < max_level))
			{
				currentLevel = max_level;
				return;
			}

			currentLevel = new_level;

			if (!tiles.ContainsKey(cell))
				tiles[cell] = new TriggerCell() { Level = currentLevel };
			else
				tiles[cell].Level = currentLevel;
		}

		bool disposed = false;
		void INotifyActorDisposing.Disposing(Actor self)
		{
			if (disposed)
				return;

			disposed = true;
		}

		// Debug only, require enabling the `ITickRender` interface in this class
		public void TickRender(WorldRenderer wr, Actor self)
		{
			if (Info.ShowDebugOverlay)
			{
				var font = Game.Renderer.Fonts["Bold"];
				foreach (var kv in tiles)
				{
					var i = new FloatingText(world.Map.CenterOfCell(kv.Key), Color.Gold, kv.Value.Level.ToString(), 1);
					world.Add(i);
				}
			}
		}
	}
}
