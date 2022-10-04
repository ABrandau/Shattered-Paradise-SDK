using System;
using System.Linq;
using OpenRA.GameRules;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Warheads;
using OpenRA.Mods.TA.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.TA.Warheads
{
	public class TriggerLayerWeaponWarhead : Warhead, IRulesetLoaded<WeaponInfo>
	{
		[Desc("Range between falloff steps in cells.")]
		public readonly WDist Spread = new WDist(1024);

		[Desc("Level percentage at each range step.")]
		public readonly int[] Falloff = { 100, 75, 50 };

		[Desc("The name of the layer we want to increase the level of.")]
		public readonly string LayerName = "";

		[Desc("Ranges at which each Falloff step is defined (in cells). Overrides Spread.")]
		public WDist[] Range = null;

		[Desc("Level this weapon puts on the ground. Accumulates over previously trigger area.")]
		public int Level = 200;

		[Desc("It saturates at this level, by this weapon.")]
		public int MaxLevel = 600;

		[Desc("Allow triggering set level to affected cell when hit position at this trigger level on trigger area")]
		public bool AllowTiggerLevel = true;

		[Desc("Allow triggering set level to affected cell when hit position at this trigger level on trigger area")]
		public bool AllowSetLevelWhenTrigger = true;

		[Desc("Allow triggering set level to affected cell when hit position at this trigger level on trigger area")]
		public bool AllowTiggerWeaponWhenTrigger = true;

		[Desc("Trigger ExplodeSetLevel and TiggerWeapon when hit position at this [TiggerAtLevelMin, TiggerAtLevelMax] on trigger area")]
		public int TiggerAtLevelMax = int.MaxValue;

		[Desc("Trigger ExplodeSetLevel and TiggerWeapon when hit position at this [TiggerAtLevelMin, TiggerAtLevelMax] on trigger area")]
		public int TiggerAtLevelMin = int.MinValue;

		[Desc("Trigger set level to affected cell when hit position at this trigger level on trigger area")]
		public int TiggerSetLevel = 0;

		[WeaponReference]
		[FieldLoader.Require]
		[Desc("Has to be defined in weapons.yaml as well.")]
		public readonly string TiggerWeapon = null;

		WeaponInfo weapon;

		public void RulesetLoaded(Ruleset rules, WeaponInfo info)
		{
			if (Range == null)
				Range = Exts.MakeArray(Falloff.Length, i => i * Spread);
			else
			{
				if (Range.Length != 1 && Range.Length != Falloff.Length)
					throw new YamlException("Number of range values must be 1 or equal to the number of Falloff values.");

				for (var i = 0; i < Range.Length - 1; i++)
					if (Range[i] > Range[i + 1])
						throw new YamlException("Range values must be specified in an increasing order.");
			}

			if (!rules.Weapons.TryGetValue(TiggerWeapon.ToLowerInvariant(), out weapon))
				throw new YamlException("Weapons Ruleset does not contain an entry '{0}'".F(TiggerWeapon.ToLowerInvariant()));
		}

		public override void DoImpact(in Target target, WarheadArgs args)
		{
			var firedBy = args.SourceActor;
			var world = firedBy.World;

			if (world.LocalPlayer != null)
			{
				var devMode = world.LocalPlayer.PlayerActor.TraitOrDefault<DebugVisualizations>();
				if (devMode != null && devMode.CombatGeometry)
				{
					var rng = Exts.MakeArray(Range.Length, i => WDist.FromCells(Range[i].Length));
					world.WorldActor.Trait<WarheadDebugOverlay>().AddImpact(target.CenterPosition, rng, Primitives.Color.Gold);
				}
			}

			var targetTile = world.Map.CellContaining(target.CenterPosition);
			var raLayer = world.WorldActor.TraitsImplementing<WeaponTriggerCells>()
				.First(l => l.Info.Name == LayerName);

			var triggeredSetLevel = false;
			if (AllowTiggerLevel &&
				raLayer.GetLevel(targetTile) >= TiggerAtLevelMin &&
				raLayer.GetLevel(targetTile) <= TiggerAtLevelMax)
			{
				if (AllowTiggerWeaponWhenTrigger)
					weapon.Impact(Target.FromPos(target.CenterPosition), firedBy);

				var affectedCells = world.Map.FindTilesInCircle(targetTile, (int)Math.Ceiling((decimal)Range[Range.Length - 1].Length / 1024));
				if (AllowSetLevelWhenTrigger)
				{
					triggeredSetLevel = true;
					foreach (var cell in affectedCells)
						raLayer.SetLevel(cell, TiggerSetLevel);
				}
			}

			if (!triggeredSetLevel && Level != 0)
			{
				var affectedCells = world.Map.FindTilesInCircle(targetTile, (int)Math.Ceiling((decimal)Range[Range.Length - 1].Length / 1024));
				foreach (var cell in affectedCells)
				{
					var mul = GetIntensityFalloff((target.CenterPosition - world.Map.CenterOfCell(cell)).Length);
					raLayer.IncreaseLevel(cell, Level * mul / 100, MaxLevel);
				}
			}
		}

		int GetIntensityFalloff(int distance)
		{
			var inner = Range[0].Length;
			for (var i = 1; i < Range.Length; i++)
			{
				var outer = Range[i].Length;
				if (outer > distance)
					return int2.Lerp(Falloff[i - 1], Falloff[i], distance - inner, outer - inner);

				inner = outer;
			}

			return 0;
		}
	}
}
