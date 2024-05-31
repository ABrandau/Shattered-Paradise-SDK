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
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Sp.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	[TraitLocation(SystemActors.World | SystemActors.EditorWorld)]
	[Desc("Create a palette by swapping RPG and adjust lightning to another palette.")]
	sealed class PaletteFromPaletteWithLightModifiedAndRBGSwappedInfo : TraitInfo, ILobbyCustomRulesIgnore
	{
		[PaletteDefinition]
		[FieldLoader.Require]
		[Desc("Internal palette name")]
		public readonly string Name = null;

		[PaletteReference]
		[FieldLoader.Require]
		[Desc("The name of the palette to base off.")]
		public readonly string BasePalette = null;

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

		[Desc("Allow palette modifiers to change the palette.")]
		public readonly bool AllowModifiers = true;

		public override object Create(ActorInitializer init) { return new PaletteFromPaletteWithLightModifiedAndRBGSwapped(this); }
	}

	sealed class PaletteFromPaletteWithLightModifiedAndRBGSwapped : ILoadsPalettes, IProvidesAssetBrowserPalettes
	{
		readonly PaletteFromPaletteWithLightModifiedAndRBGSwappedInfo info;

		public PaletteFromPaletteWithLightModifiedAndRBGSwapped(PaletteFromPaletteWithLightModifiedAndRBGSwappedInfo info) { this.info = info; }

		void ILoadsPalettes.LoadPalettes(WorldRenderer wr)
		{
			var lightcolor = new float3(info.RedTint, info.GreenTint, info.BlueTint);
			var basePalette = wr.Palette(info.BasePalette).Palette;

			var remap = new GenerateLightRemapAfterRPGSwapped(basePalette, info.RBGSwapMode, info.Intensity, lightcolor, info.RBGSwapIndex, info.LightEffectIndex);
			wr.AddPalette(info.Name, new ImmutablePalette(basePalette, remap), info.AllowModifiers);
		}

		public IEnumerable<string> PaletteNames { get { yield return info.Name; } }
	}
}
