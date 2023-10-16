#region Copyright & License Information
/*
 * Copyright The OpenRA-SP Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	public struct MapSmudgeSP
	{
		public string Type;
		public int Depth;
	}

	[TraitLocation(SystemActors.World)]
	[Desc("Spawn cell based Smudge, will disappear after duration", "Order of the layers defines the Z sorting.")]
	public sealed class SmudgeLayerSPInfo : TraitInfo
	{
		[Desc("Smudge and their sprites set name")]
		public readonly Dictionary<string, string> SmudgeTypesImages = default;

		[Desc("Smudge types and their level, higher level will override lower level in one cell of the same type")]
		public readonly Dictionary<string, string[]> SmudgeLevelSequences = default;

		[Desc("Smudge level down after this duration, and disappears after level 0")]
		public readonly Dictionary<string, int> SmudgeDurationOfEachLevel = default;

		[Desc("Allow higher level overrides lower level among different types.")]
		public readonly bool AllowOverrideAmongTypes = true;

		[PaletteReference]
		public readonly string Palette = TileSet.TerrainPaletteInternalName;

		[FieldLoader.LoadUsing(nameof(LoadInitialSmudges))]
		public readonly Dictionary<CPos, MapSmudgeSP> InitialSmudges;

		public static object LoadInitialSmudges(MiniYaml yaml)
		{
			var nd = yaml.ToDictionary();
			var smudges = new Dictionary<CPos, MapSmudgeSP>();
			if (nd.TryGetValue("InitialSmudges", out var smudgeYaml))
			{
				foreach (var node in smudgeYaml.Nodes)
				{
					try
					{
						var cell = FieldLoader.GetValue<CPos>("key", node.Key);
						var parts = node.Value.Value.Split(',');
						var type = parts[0];
						var depth = FieldLoader.GetValue<int>("depth", parts[1]);
						smudges.Add(cell, new MapSmudgeSP { Type = type, Depth = depth });
					}
					catch { }
				}
			}

			return smudges;
		}

		public override object Create(ActorInitializer init) { return new SmudgeLayerSP(init.Self, this); }
	}

	public sealed class SmudgeLayerSP : IRenderOverlay, IWorldLoaded, ITickRender, INotifyActorDisposing
	{
		struct SmudgeSP
		{
			public string Type;
			public int LifeTime;
			public ISpriteSequence Sequence;
			public int Level;
		}

		public readonly SmudgeLayerSPInfo Info;
		readonly Dictionary<CPos, SmudgeSP> tiles = new();
		readonly Dictionary<CPos, SmudgeSP> dirty = new();

		// type : {level: SmudgeSP}
		readonly Dictionary<string, Dictionary<int, SmudgeSP>> smudgesDB = new();
		readonly World world;

		TerrainSpriteLayer render;
		PaletteReference paletteReference;
		bool disposed;

		public SmudgeLayerSP(Actor self, SmudgeLayerSPInfo info)
		{
			Info = info;
			world = self.World;

			var mapSequences = world.Map.Sequences;

			foreach (var type in Info.SmudgeTypesImages.Keys)
			{
				var smudgeDict = new Dictionary<int, SmudgeSP>();
				var levelSequence = Info.SmudgeLevelSequences[type];
				for (var i = 0; i < levelSequence.Length; i++)
				{
					smudgeDict[i] = new SmudgeSP
					{
						Level = i,
						LifeTime = Info.SmudgeDurationOfEachLevel[type],
						Type = type,
						Sequence = mapSequences.GetSequence(Info.SmudgeTypesImages[type], levelSequence[i]),
					};
				}

				smudgesDB.Add(type, smudgeDict);
			}
		}

		public void WorldLoaded(World w, WorldRenderer wr)
		{
			var spritesSet = smudgesDB.Values.SelectMany(levels => Exts.MakeArray(levels.Values.Count, i => Exts.MakeArray(levels[i].Sequence.Length, f => levels[i].Sequence.GetSprite(f)))).ToList();

			var sheet = spritesSet[0][0].Sheet;
			var blendMode = spritesSet[0][0].BlendMode;
			var emptySprite = new Sprite(sheet, Rectangle.Empty, TextureChannel.Alpha);

			foreach (var sprites in spritesSet)
			{
				if (sprites.Any(s => s.BlendMode != blendMode))
					throw new InvalidDataException("Smudges specify different blend modes. "
						+ "Try using different smudge types for smudges that use different blend modes.");
			}

			render = new TerrainSpriteLayer(w, wr, emptySprite, blendMode, w.Type != WorldType.Editor);
			paletteReference = wr.Palette(Info.Palette);

			// Add map smudges
			foreach (var kv in Info.InitialSmudges)
			{
				var s = kv.Value;
				if (!smudgesDB.ContainsKey(s.Type))
					continue;

				var level = Math.Min(s.Depth, smudgesDB[s.Type].Count);
				var seq = smudgesDB[s.Type][level].Sequence;
				var smudge = new SmudgeSP
				{
					Type = s.Type,
					Level = level,
					LifeTime = int.MaxValue,
					Sequence = seq,
				};

				tiles.Add(kv.Key, smudge);
				render.Update(kv.Key, seq, paletteReference, s.Depth);
			}
		}

		int GetLevel(int currentLevel, int initialLevel, int addlevel, int maxLevel)
		{
			return Math.Min(Math.Max(currentLevel + addlevel, initialLevel), maxLevel);
		}

		public void AddSmudge(CPos loc, string type, int initialLevel, int addlevel, int maxLevel)
		{
			if (!world.Map.Contains(loc) || !smudgesDB.ContainsKey(type))
				return;

			var alreadyInDirty = dirty.ContainsKey(loc);
			var deprecatedInDirty = false;
			if (alreadyInDirty)
			{
				var dirtySmudge = dirty[loc];
				deprecatedInDirty = dirtySmudge.Sequence == null || (dirtySmudge.Level <= 0 && dirtySmudge.LifeTime <= 0);
			}

			// Setting Sequence to null to indicate a deleted smudge by building.
			if ((!alreadyInDirty || deprecatedInDirty) && !tiles.ContainsKey(loc))
			{
					// No smudge; create a new one
					// -1 means no smudge
					var level = GetLevel(-1, initialLevel, 0, Math.Min(maxLevel, smudgesDB[type].Count));
					if (level < 0)
						return;

					dirty[loc] = smudgesDB[type][level];
			}
			else
			{
				// Existing smudge; check overrides, increase levels or refresh lifetime
				// Setting Sequence to null to indicate a deleted smudge by building.
				var tile = alreadyInDirty && !deprecatedInDirty ? dirty[loc] : tiles[loc];

				if (type != tile.Type)
				{
					if (!Info.AllowOverrideAmongTypes)
						return;

					var level = GetLevel(-1, initialLevel, 0, Math.Min(maxLevel, smudgesDB[type].Count));
					if (level <= tile.Level)
						return;

					dirty[loc] = smudgesDB[type][level];
				}
				else
				{
					var level = GetLevel(tile.Level, initialLevel, addlevel, Math.Min(maxLevel, smudgesDB[type].Count));
					if (level < tile.Level)
						return;

					dirty[loc] = smudgesDB[type][level];
				}
			}
		}

		public void RemoveSmudge(CPos loc)
		{
			if (!world.Map.Contains(loc))
				return;

			var tile = dirty.ContainsKey(loc) ? dirty[loc] : default;

			// Setting Sequence to null to indicate a deleted smudge by building.
			tile.Sequence = null;
			dirty[loc] = tile;
		}

		int updatedTick = -1;

		void ITickRender.TickRender(WorldRenderer wr, Actor self)
		{
			if (updatedTick == self.World.WorldTick)
				return;

			updatedTick = self.World.WorldTick;

			var remove = new List<CPos>();
			foreach (var kv in dirty)
			{
				// Update visual cell at where we can see
				if (!world.FogObscures(kv.Key))
				{
					if (kv.Value.Sequence == null)
					{
						tiles.Remove(kv.Key);
						render.Clear(kv.Key);
					}
					else if (kv.Value.LifeTime < 0)
					{
						if (kv.Value.Level <= 0)
						{
							tiles.Remove(kv.Key);
							render.Clear(kv.Key);
						}
						else
						{
							tiles[kv.Key] = smudgesDB[kv.Value.Type][kv.Value.Level - 1];
							var seq = tiles[kv.Key].Sequence;
							render.Update(kv.Key, seq, paletteReference, Game.CosmeticRandom.Next(seq.Length));
						}
					}
					else
					{
						var smudge = kv.Value;
						var seq = kv.Value.Sequence;
						tiles[kv.Key] = smudge;
						render.Update(kv.Key, seq, paletteReference, Game.CosmeticRandom.Next(seq.Length));
					}

					remove.Add(kv.Key);
				}

				// Update dirty cell at where we cannot see
				else
				{
					if (kv.Value.LifeTime < 0)
					{
						if (kv.Value.Level > 0)
							dirty[kv.Key] = smudgesDB[kv.Value.Type][kv.Value.Level - 1];
					}
					else
					{
						var tile = kv.Value;
						tile.LifeTime--;
						dirty[kv.Key] = tile;
					}
				}
			}

			foreach (var r in remove)
				dirty.Remove(r);

			// Update visual cell's lifetime
			foreach (var kv in tiles)
			{
				if (kv.Value.LifeTime < 0)
				{
					dirty[kv.Key] = kv.Value;
					var tile = kv.Value;
					tile.LifeTime = int.MaxValue; // only update once to dirty
					tiles[kv.Key] = tile;
				}
				else
				{
					var tile = kv.Value;
					tile.LifeTime--;
					tiles[kv.Key] = tile;
				}
			}
		}

		void IRenderOverlay.Render(WorldRenderer wr)
		{
			render.Draw(wr.Viewport);
		}

		void INotifyActorDisposing.Disposing(Actor self)
		{
			if (disposed)
				return;

			render.Dispose();
			disposed = true;
		}
	}
}
