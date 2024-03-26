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

using System.Collections.Generic;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	enum RBGSwappedMode { RandG, RandB, GandB }

	[TraitLocation(SystemActors.World | SystemActors.EditorWorld)]
	[Desc("Create a palette by applying alpha transparency to another palette.")]
	sealed class PaletteFromPaletteWithRBGSwappedInfo : TraitInfo
	{
		[PaletteDefinition]
		[FieldLoader.Require]
		[Desc("Internal palette name")]
		public readonly string Name = null;

		[PaletteReference]
		[FieldLoader.Require]
		[Desc("The name of the palette to base off.")]
		public readonly string BasePalette = null;

		[Desc("RBG Swapped Mode used for this convertion.")]
		public readonly RBGSwappedMode RBGSwappedMode = RBGSwappedMode.RandG;

		[Desc("Allow palette modifiers to change the palette.")]
		public readonly bool AllowModifiers = true;

		public override object Create(ActorInitializer init) { return new PaletteFromPaletteWithRBGSwapped(this); }
	}

	sealed class PaletteFromPaletteWithRBGSwapped : ILoadsPalettes, IProvidesAssetBrowserPalettes
	{
		readonly PaletteFromPaletteWithRBGSwappedInfo info;

		public PaletteFromPaletteWithRBGSwapped(PaletteFromPaletteWithRBGSwappedInfo info) { this.info = info; }

		public void LoadPalettes(WorldRenderer wr)
		{
			var remap = new RBGSwappedPaletteRemap(info.RBGSwappedMode);
			wr.AddPalette(info.Name, new ImmutablePalette(wr.Palette(info.BasePalette).Palette, remap), info.AllowModifiers);
		}

		public IEnumerable<string> PaletteNames { get { yield return info.Name; } }
	}

	sealed class RBGSwappedPaletteRemap : IPaletteRemap
	{
		readonly RBGSwappedMode mode;

		public RBGSwappedPaletteRemap(RBGSwappedMode mode)
		{
			this.mode = mode;
		}

		public Color GetRemappedColor(Color original, int index)
		{
			switch (mode)
			{
				case RBGSwappedMode.RandG:
					return Color.FromArgb(original.A, original.G, original.R, original.B);

				case RBGSwappedMode.RandB:
					return Color.FromArgb(original.A, original.B, original.G, original.R);

				case RBGSwappedMode.GandB:
					return Color.FromArgb(original.A, original.R, original.B, original.G);

				default:
					return original;
			}
		}
	}
}
