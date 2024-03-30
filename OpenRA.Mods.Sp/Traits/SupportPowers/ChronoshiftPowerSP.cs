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
using System.Linq;
using OpenRA.GameRules;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Orders;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	sealed class ChronoshiftPowerSPInfo : SupportPowerInfo
	{
		[FieldLoader.Require]
		[Desc("Size of the footprint of the affected area.")]
		public readonly Dictionary<int, CVec> Dimensions = new();

		[FieldLoader.Require]
		[Desc("Actual footprint. Cells marked as x will be affected.")]
		public readonly Dictionary<int, string> Footprints = new();

		[PaletteReference]
		public readonly string TargetOverlayPalette = TileSet.TerrainPaletteInternalName;

		public readonly string FootprintImage = "overlay";

		[SequenceReference(nameof(FootprintImage), prefix: true)]
		public readonly string ValidFootprintSequence = "target-valid";

		[SequenceReference(nameof(FootprintImage))]
		public readonly string InvalidFootprintSequence = "target-invalid";

		[SequenceReference(nameof(FootprintImage))]
		public readonly string SourceFootprintSequence = "target-select";

		public readonly string EffectImage = null;

		[SequenceReference(nameof(EffectImage))]
		public readonly string SelectionStartSequence = null;

		[SequenceReference(nameof(EffectImage))]
		public readonly string SelectionLoopSequence = null;

		[PaletteReference]
		public readonly string EffectPalette = null;

		[WeaponReference]
		[FieldLoader.Require]
		[Desc("Default weapon to use for explosion.")]
		public readonly string ImpactWeapon = null;

		[WeaponReference]
		[FieldLoader.Require]
		[Desc("Default weapon to use for explosion. Use Weapon if not set.")]
		public readonly string TeleportWeapon = null;

		[CursorReference]
		[Desc("Cursor to display when selecting targets for the chronoshift.")]
		public readonly string SelectionCursor = "chrono-select";

		[CursorReference]
		[Desc("Cursor to display when targeting an area for the chronoshift.")]
		public readonly string TargetCursor = "chrono-target";

		[CursorReference]
		[Desc("Cursor to display when the targeted area is blocked.")]
		public readonly string TargetBlockedCursor = "move-blocked";

		public WeaponInfo ImpactWeaponInfo { get; private set; }
		public WeaponInfo TeleportWeaponInfo { get; private set; }

		public override object Create(ActorInitializer init) { return new ChronoshiftPowerSP(init.Self, this); }
		public override void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			if (!string.IsNullOrEmpty(ImpactWeapon))
			{
				var weaponToLower = ImpactWeapon.ToLowerInvariant();
				if (!rules.Weapons.TryGetValue(weaponToLower, out var weapon))
					throw new YamlException($"Weapons Ruleset does not contain an entry '{weaponToLower}'");
				ImpactWeaponInfo = weapon;
			}

			if (!string.IsNullOrEmpty(TeleportWeapon))
			{
				var weaponToLower = TeleportWeapon.ToLowerInvariant();
				if (!rules.Weapons.TryGetValue(weaponToLower, out var weapon))
					throw new YamlException($"Weapons Ruleset does not contain an entry '{weaponToLower}'");
				TeleportWeaponInfo = weapon;
			}

			base.RulesetLoaded(rules, ai);
		}
	}

	sealed class ChronoshiftPowerSP : SupportPower
	{
		readonly Dictionary<int, char[]> footprints = new();
		readonly Dictionary<int, CVec> dimensions;

		public ChronoshiftPowerSP(Actor self, ChronoshiftPowerSPInfo info)
			: base(self, info)
		{
			foreach (var pair in info.Footprints)
				footprints.Add(pair.Key, pair.Value.Where(c => !char.IsWhiteSpace(c)).ToArray());

			dimensions = info.Dimensions;
		}

		public override void SelectTarget(Actor self, string order, SupportPowerManager manager)
		{
			self.World.OrderGenerator = new SelectChronoshiftTarget(Self.World, order, manager, this);
		}

		public override void Activate(Actor self, Order order, SupportPowerManager manager)
		{
			base.Activate(self, order, manager);

			var info = (ChronoshiftPowerSPInfo)Info;

			// Generate a weapon on the place of impact, Generate a weapon on the place of teleport
			var weapon = info.TeleportWeaponInfo;
			var pos = order.Target.CenterPosition;
			var weapon2 = info.ImpactWeaponInfo;
			var pos2 = self.World.Map.CenterOfCell(order.ExtraLocation);
			var firer = self.Owner.PlayerActor;

			self.World.AddFrameEndTask(w =>
			{
				PlayLaunchSounds();
				if (weapon.Report != null && weapon.Report.Length > 0)
				{
					if (weapon.AudibleThroughFog || (!self.World.ShroudObscures(pos) && !self.World.FogObscures(pos)))
						Game.Sound.Play(SoundType.World, weapon.Report, self.World, pos, null, weapon.SoundVolume);
				}

				weapon.Impact(Target.FromPos(pos), firer);

				if (weapon2.Report != null && weapon2.Report.Length > 0)
				{
					if (weapon2.AudibleThroughFog || (!self.World.ShroudObscures(pos2) && !self.World.FogObscures(pos2)))
						Game.Sound.Play(SoundType.World, weapon2.Report, self.World, pos2, null, weapon2.SoundVolume);
				}

				weapon2.Impact(Target.FromPos(pos2), firer);
			});

			var targetDelta = self.World.Map.CellContaining(order.Target.CenterPosition) - order.ExtraLocation;

			var level = GetLevel();
			var teleportCells = CellsMatching(self.World.Map.CellContaining(order.Target.CenterPosition), footprints.First(f => f.Key == level).Value, dimensions.First(d => d.Key == level).Value).ToList();

			foreach (var target in UnitsInRange(order.ExtraLocation))
			{
				var cs = target.TraitsImplementing<ChronoshiftableSP>()
					.FirstEnabledConditionalTraitOrDefault();

				if (cs == null)
					continue;

				var targetCell = target.Location + targetDelta;

				cs.Teleport(target, targetCell, teleportCells, self);
			}
		}

		public IEnumerable<Actor> UnitsInRange(CPos xy)
		{
			var level = GetLevel();
			var tiles = CellsMatching(xy, footprints.First(f => f.Key == level).Value, dimensions.First(d => d.Key == level).Value);
			var units = new HashSet<Actor>();
			foreach (var t in tiles)
				units.UnionWith(Self.World.ActorMap.GetActorsAt(t));

			return units.Where(a => a.TraitsImplementing<ChronoshiftableSP>().Any(cs => !cs.IsTraitDisabled));
		}

		public bool SimilarTerrain(CPos xy, CPos sourceLocation)
		{
			if (!Self.Owner.Shroud.IsExplored(xy))
				return false;

			var level = GetLevel();
			var footprint = footprints.First(f => f.Key == level).Value;
			var dimension = dimensions.First(f => f.Key == level).Value;
			var sourceTiles = CellsMatching(xy, footprint, dimension);
			var destTiles = CellsMatching(sourceLocation, footprint, dimension);
			if (!sourceTiles.Any() || !destTiles.Any())
				return false;

			using (var se = sourceTiles.GetEnumerator())
			using (var de = destTiles.GetEnumerator())
				while (se.MoveNext() && de.MoveNext())
				{
					var a = se.Current;
					var b = de.Current;

					if (!Self.Owner.Shroud.IsExplored(a) || !Self.Owner.Shroud.IsExplored(b))
						return false;

					if (Self.World.Map.GetTerrainIndex(a) != Self.World.Map.GetTerrainIndex(b))
						return false;
				}

			return true;
		}

		sealed class SelectChronoshiftTarget : OrderGenerator
		{
			readonly ChronoshiftPowerSP power;
			readonly Dictionary<int, char[]> footprints = new();
			readonly Dictionary<int, CVec> dimensions;
			readonly Sprite tile;
			readonly float alpha;
			readonly SupportPowerManager manager;
			readonly string order;

			public SelectChronoshiftTarget(World world, string order, SupportPowerManager manager, ChronoshiftPowerSP power)
			{
				// Clear selection if using Left-Click Orders
				if (Game.Settings.Game.UseClassicMouseStyle)
					manager.Self.World.Selection.Clear();

				this.manager = manager;
				this.order = order;
				this.power = power;

				var info = (ChronoshiftPowerSPInfo)power.Info;
				var s = world.Map.Sequences.GetSequence(info.FootprintImage, info.SourceFootprintSequence);
				foreach (var pair in info.Footprints)
					footprints.Add(pair.Key, pair.Value.Where(c => !char.IsWhiteSpace(c)).ToArray());

				dimensions = info.Dimensions;
				tile = s.GetSprite(0);
				alpha = s.GetAlpha(0);
			}

			protected override IEnumerable<Order> OrderInner(World world, CPos cell, int2 worldPixel, MouseInput mi)
			{
				world.CancelInputMode();
				if (mi.Button == MouseButton.Left)
					world.OrderGenerator = new SelectDestination(world, order, manager, power, cell);

				yield break;
			}

			protected override void Tick(World world)
			{
				// Cancel the OG if we can't use the power
				if (!manager.Powers.TryGetValue(order, out var p) || !p.Active || !p.Ready)
					world.CancelInputMode();
			}

			protected override IEnumerable<IRenderable> RenderAboveShroud(WorldRenderer wr, World world) { yield break; }

			protected override IEnumerable<IRenderable> RenderAnnotations(WorldRenderer wr, World world)
			{
				var xy = wr.Viewport.ViewToWorld(Viewport.LastMousePos);
				var targetUnits = power.UnitsInRange(xy).Where(a => !world.FogObscures(a));

				foreach (var unit in targetUnits)
				{
					if (unit.CanBeViewedByPlayer(manager.Self.Owner))
					{
						var decorations = unit.TraitsImplementing<ISelectionDecorations>().FirstEnabledTraitOrDefault();
						if (decorations != null)
							foreach (var d in decorations.RenderSelectionAnnotations(unit, wr, Color.Red))
								yield return d;
					}
				}
			}

			protected override IEnumerable<IRenderable> Render(WorldRenderer wr, World world)
			{
				var xy = wr.Viewport.ViewToWorld(Viewport.LastMousePos);
				var level = power.GetLevel();
				var tiles = power.CellsMatching(xy, footprints.First(f => f.Key == level).Value, dimensions.First(d => d.Key == level).Value);
				var palette = wr.Palette(((ChronoshiftPowerSPInfo)power.Info).TargetOverlayPalette);
				foreach (var t in tiles)
					yield return new SpriteRenderable(tile, wr.World.Map.CenterOfCell(t), WVec.Zero, -511, palette, 1f, alpha, float3.Ones, TintModifiers.IgnoreWorldTint, true);
			}

			protected override string GetCursor(World world, CPos cell, int2 worldPixel, MouseInput mi)
			{
				return ((ChronoshiftPowerSPInfo)power.Info).SelectionCursor;
			}
		}

		sealed class SelectDestination : OrderGenerator
		{
			readonly ChronoshiftPowerSP power;
			readonly CPos sourceLocation;
			readonly Dictionary<int, char[]> footprints = new();
			readonly Dictionary<int, CVec> dimensions;
			readonly Sprite validTile, invalidTile, sourceTile;
			readonly float validAlpha, invalidAlpha, sourceAlpha;
			readonly SupportPowerManager manager;
			readonly Animation overlay;
			readonly string order;

			public SelectDestination(World world, string order, SupportPowerManager manager, ChronoshiftPowerSP power, CPos sourceLocation)
			{
				this.manager = manager;
				this.order = order;
				this.power = power;
				this.sourceLocation = sourceLocation;

				var info = (ChronoshiftPowerSPInfo)power.Info;
				if (info.EffectImage != null)
				{
					overlay = new Animation(world, info.EffectImage);

					var powerInfo = (ChronoshiftPowerSPInfo)power.Info;
					if (powerInfo.SelectionStartSequence != null)
						overlay.PlayThen(powerInfo.SelectionStartSequence,
							() => overlay.PlayRepeating(powerInfo.SelectionLoopSequence));
					else
						overlay.PlayRepeating(powerInfo.SelectionLoopSequence);
				}

				foreach (var pair in info.Footprints)
					footprints.Add(pair.Key, pair.Value.Where(c => !char.IsWhiteSpace(c)).ToArray());

				dimensions = info.Dimensions;
				var sequences = world.Map.Sequences;

				var tilesetValid = info.ValidFootprintSequence + "-" + world.Map.Tileset.ToLowerInvariant();
				if (sequences.HasSequence(info.FootprintImage, tilesetValid))
				{
					var validSequence = sequences.GetSequence(info.FootprintImage, tilesetValid);
					validTile = validSequence.GetSprite(0);
					validAlpha = validSequence.GetAlpha(0);
				}
				else
				{
					var validSequence = sequences.GetSequence(info.FootprintImage, info.ValidFootprintSequence);
					validTile = validSequence.GetSprite(0);
					validAlpha = validSequence.GetAlpha(0);
				}

				var invalidSequence = sequences.GetSequence(info.FootprintImage, info.InvalidFootprintSequence);
				invalidTile = invalidSequence.GetSprite(0);
				invalidAlpha = invalidSequence.GetAlpha(0);

				var sourceSequence = sequences.GetSequence(info.FootprintImage, info.SourceFootprintSequence);
				sourceTile = sourceSequence.GetSprite(0);
				sourceAlpha = sourceSequence.GetAlpha(0);
			}

			protected override IEnumerable<Order> OrderInner(World world, CPos cell, int2 worldPixel, MouseInput mi)
			{
				if (mi.Button == MouseButton.Right)
				{
					world.CancelInputMode();
					yield break;
				}

				var ret = OrderInner(cell).FirstOrDefault();
				if (ret == null)
					yield break;

				world.CancelInputMode();
				yield return ret;
			}

			IEnumerable<Order> OrderInner(CPos xy)
			{
				// Cannot chronoshift into unexplored location
				if (IsValidTarget(xy))
					yield return new Order(order, manager.Self, Target.FromCell(manager.Self.World, xy), false)
					{
						ExtraLocation = sourceLocation,
						SuppressVisualFeedback = true
					};
			}

			protected override void Tick(World world)
			{
				// Cancel the OG if we can't use the power
				if (!manager.Powers.TryGetValue(order, out var p) || !p.Active || !p.Ready)
					world.CancelInputMode();

				overlay?.Tick();
			}

			protected override IEnumerable<IRenderable> RenderAboveShroud(WorldRenderer wr, World world)
			{
				var xy = wr.Viewport.ViewToWorld(Viewport.LastMousePos);
				var palette = wr.Palette(power.Info.IconPalette);

				// Destination tiles
				var delta = xy - sourceLocation;
				var level = power.GetLevel();
				foreach (var t in power.CellsMatching(sourceLocation, footprints.First(f => f.Key == level).Value, dimensions.First(d => d.Key == level).Value))
				{
					var isValid = manager.Self.Owner.Shroud.IsExplored(t + delta);
					var tile = isValid ? validTile : invalidTile;
					var alpha = isValid ? validAlpha : invalidAlpha;
					yield return new SpriteRenderable(tile, wr.World.Map.CenterOfCell(t + delta), WVec.Zero, -511, palette, 1f, alpha, float3.Ones, TintModifiers.IgnoreWorldTint, true);
				}

				// Unit previews
				foreach (var unit in power.UnitsInRange(sourceLocation))
				{
					if (unit.CanBeViewedByPlayer(manager.Self.Owner))
					{
						var targetCell = unit.Location + (xy - sourceLocation);
						var canEnter = manager.Self.Owner.Shroud.IsExplored(targetCell);
						var tile = canEnter ? validTile : invalidTile;
						var alpha = canEnter ? validAlpha : invalidAlpha;
						yield return new SpriteRenderable(tile, wr.World.Map.CenterOfCell(targetCell), WVec.Zero, -511, palette, 1f, alpha, float3.Ones, TintModifiers.IgnoreWorldTint, true);
					}

					var offset = world.Map.CenterOfCell(xy) - world.Map.CenterOfCell(sourceLocation);
					if (unit.CanBeViewedByPlayer(manager.Self.Owner))
						foreach (var r in unit.Render(wr))
						{
							if (!r.IsDecoration)
								yield return r.OffsetBy(offset);
						}
				}
			}

			protected override IEnumerable<IRenderable> RenderAnnotations(WorldRenderer wr, World world)
			{
				foreach (var unit in power.UnitsInRange(sourceLocation))
				{
					if (unit.CanBeViewedByPlayer(manager.Self.Owner))
					{
						var decorations = unit.TraitsImplementing<ISelectionDecorations>().FirstEnabledTraitOrDefault();
						if (decorations != null)
							foreach (var d in decorations.RenderSelectionAnnotations(unit, wr, Color.Red))
								yield return d;
					}
				}
			}

			protected override IEnumerable<IRenderable> Render(WorldRenderer wr, World world)
			{
				if (overlay != null)
				{
					var powerInfo = (ChronoshiftPowerSPInfo)power.Info;
					foreach (var r in overlay.Render(world.Map.CenterOfCell(sourceLocation), wr.Palette(powerInfo.EffectPalette)))
						yield return r;
				}

				// Source tiles
				var palette = wr.Palette(power.Info.IconPalette);
				var level = power.GetLevel();
				foreach (var t in power.CellsMatching(sourceLocation, footprints.First(f => f.Key == level).Value, dimensions.First(d => d.Key == level).Value))
					yield return new SpriteRenderable(sourceTile, wr.World.Map.CenterOfCell(t), WVec.Zero, -511, palette, 1f, sourceAlpha, float3.Ones, TintModifiers.IgnoreWorldTint, true);
			}

			bool IsValidTarget(CPos xy)
			{
				var canTeleport = false;
				var anyUnitsInRange = false;
				foreach (var unit in power.UnitsInRange(sourceLocation))
				{
					anyUnitsInRange = true;
					var targetCell = unit.Location + (xy - sourceLocation);
					if (manager.Self.Owner.Shroud.IsExplored(targetCell))
					{
						canTeleport = true;
						break;
					}
				}

				// Don't teleport if there are no units in range (either all moved out of range, or none yet moved into range)
				if (!anyUnitsInRange)
					return false;

				if (!canTeleport)
				{
					// Check the terrain types. This will allow chronoshifts to occur on empty terrain to terrain of
					// a similar type. This also keeps the cursor from changing in non-visible property, alerting the
					// chronoshifter of enemy unit presence
					canTeleport = power.SimilarTerrain(sourceLocation, xy);
				}

				return canTeleport;
			}

			protected override string GetCursor(World world, CPos cell, int2 worldPixel, MouseInput mi)
			{
				var powerInfo = (ChronoshiftPowerSPInfo)power.Info;
				return IsValidTarget(cell) ? powerInfo.TargetCursor : powerInfo.TargetBlockedCursor;
			}
		}
	}
}
