using OpenRA.GameRules;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Warheads;
using OpenRA.Traits;

namespace OpenRA.Mods.Sp.Warheads
{
	[Desc("Apply fixed damage in a specified range ignore height. used for superweapon instant kill for air unit")]
	class IgnoreHeightDamageWarhead : TargetDamageWarhead
	{
		protected override void DoImpact(WPos pos, Actor firedBy, WarheadArgs args)
		{
			if (Spread == WDist.Zero)
				return;

			var debugVis = firedBy.World.WorldActor.TraitOrDefault<DebugVisualizations>();
			if (debugVis != null && debugVis.CombatGeometry)
				firedBy.World.WorldActor.Trait<WarheadDebugOverlay>().AddImpact(pos, new[] { WDist.Zero, Spread }, DebugOverlayColor);

			foreach (var victim in firedBy.World.FindActorsInCircle(pos, Spread))
			{
				if (!IsValidAgainst(victim, firedBy))
					continue;

				HitShape closestActiveShape = null;
				var closestDistance = int.MaxValue;

				// PERF: Avoid using TraitsImplementing<HitShape> that needs to find the actor in the trait dictionary.
				foreach (var targetPos in victim.EnabledTargetablePositions)
				{
					if (targetPos is HitShape hitshape)
					{
						var distance = hitshape.DistanceFromEdge(victim, pos).Length;
						if (distance < closestDistance)
						{
							closestDistance = distance;
							closestActiveShape = hitshape;
						}
					}
				}

				// Cannot be damaged without an active HitShape.
				if (closestActiveShape == null)
					continue;

				// Summary: when find victim actors, OpenRA ignores height,
				// but when calculate hitshape, most of damage warhead will
				// consider height.
				InflictDamage(victim, firedBy, closestActiveShape, args);
			}
		}
	}
}
