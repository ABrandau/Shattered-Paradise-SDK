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
using OpenRA.FileSystem;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Sp.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	[TraitLocation(SystemActors.World | SystemActors.EditorWorld)]
	[Desc("Palette reprocessed with swapping RPG and lightning effect.")]
	public class LoadPaletteWithLightModifiedAndRBGSwappedInfo : TraitInfo, ITilesetSpecificPaletteInfo, IProvidesCursorPaletteInfo, ILobbyCustomRulesIgnore
	{
		[FieldLoader.Require]
		[PaletteDefinition]
		[Desc("The name for the resulting palette")]
		public readonly string Name = null;

		[Desc("If defined, load the palette only for this tileset.")]
		public readonly string Tileset = null;

		[FieldLoader.Require]
		[Desc("filename to load")]
		public readonly string Filename = null;

		[Desc("Map listed indices to transparent. Ignores previous color.")]
		public readonly int[] TransparentIndex = { 0 };

		[Desc("Map listed indices to shadow. Ignores previous color.")]
		public readonly int[] ShadowIndex = Array.Empty<int>();

		[Desc("Allow palette modifiers to change the palette.")]
		public readonly bool AllowModifiers = true;

		[Desc("Map listed indices to RBG swapping. Leave empty to convert all.")]
		public readonly int[] RBGSwapIndex = Array.Empty<int>();

		[Desc("RBG Swapped Mode used for this convertion.")]
		public readonly RBGSwapMode RBGSwapMode = RBGSwapMode.None;

		[Desc("Map listed indices to cast light on. Leave empty to convert all.")]
		public readonly int[] LightEffectIndex = Array.Empty<int>();

		public readonly float Intensity = 1;
		public readonly float RedTint = 1;
		public readonly float GreenTint = 1;
		public readonly float BlueTint = 1;

		[Desc("Whether this palette is available for cursors.")]
		public readonly bool CursorPalette = false;

		string IProvidesCursorPaletteInfo.Palette => CursorPalette ? Name : null;

		ImmutablePalette IProvidesCursorPaletteInfo.ReadPalette(IReadOnlyFileSystem fileSystem)
		{
			return new ImmutablePalette(fileSystem.Open(Filename), TransparentIndex, ShadowIndex);
		}

		string ITilesetSpecificPaletteInfo.Tileset => Tileset;

		public override object Create(ActorInitializer init) { return new LoadPaletteWithLightModifiedAndRBGSwapped(init.World, this); }
	}

	public class LoadPaletteWithLightModifiedAndRBGSwapped : ILoadsPalettes, IProvidesAssetBrowserPalettes
	{
		readonly LoadPaletteWithLightModifiedAndRBGSwappedInfo info;
		readonly World world;

		public LoadPaletteWithLightModifiedAndRBGSwapped(World world, LoadPaletteWithLightModifiedAndRBGSwappedInfo info)
		{
			this.info = info;
			this.world = world;
		}

		public IEnumerable<string> PaletteNames
		{
			get
			{
				// Only expose the palette if it is available for the shellmap's tileset (which is a requirement for its use).
				if (info.Tileset == null || info.Tileset == world.Map.Rules.TerrainInfo.Id)
					yield return info.Name;
			}
		}

		void ILoadsPalettes.LoadPalettes(WorldRenderer wr)
		{
			if (info.Tileset != null && !string.Equals(info.Tileset, world.Map.Tileset, StringComparison.InvariantCultureIgnoreCase))
				return;

			var basePalette = ((IProvidesCursorPaletteInfo)info).ReadPalette(world.Map);
			var lightcolor = new float3(info.RedTint, info.GreenTint, info.BlueTint);

			var remap = new GenerateLightRemapAfterRPGSwapped(basePalette, info.RBGSwapMode, info.Intensity, lightcolor, info.RBGSwapIndex, info.LightEffectIndex);
			wr.AddPalette(info.Name, new ImmutablePalette(basePalette, remap), info.AllowModifiers);
		}
	}
}
