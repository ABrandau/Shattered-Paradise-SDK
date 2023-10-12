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
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	[TraitLocation(SystemActors.Player)]
	[Desc("Put this on the Player actor. Manages bot harvesters to ensure they always continue harvesting as long as there are resources on the map.")]
	public class HarvesterBotModuleSPInfo : ConditionalTraitInfo
	{
		[Desc("Actor types that are considered harvesters. If harvester count drops below RefineryTypes count, a new harvester is built.",
			"Leave empty to disable harvester replacement. Currently only needed by harvester replacement system.")]
		public readonly HashSet<string> HarvesterTypes = new();

		[Desc("Actor types that are counted as refineries. Currently only needed by harvester replacement system.")]
		public readonly HashSet<string> RefineryTypes = new();

		[Desc("Actor types that are generating resourse or suggest this is a resource center.")]
		public readonly HashSet<string> ResourseCenterType = new();

		[Desc("Search resourse cell around resource center in this range when searching for a new resource patch.")]
		public readonly int ResourseCenterSearchRangeInCells = 3;

		[Desc("If a resource center has less than this accessible resource cells, we will try next resource center.")]
		public readonly int FavoredHarvestableCell = 4;

		[Desc("Interval (in ticks) between giving out orders to idle harvesters.")]
		public readonly int AssignRoleInterval = 50;

		[Desc("Avoid enemy actors nearby when searching for a new resource patch. Should be somewhere near the max weapon range.")]
		public readonly WDist HarvesterEnemyAvoidanceRadius = WDist.FromCells(15);

		public override object Create(ActorInitializer init) { return new HarvesterBotModuleSP(init.Self, this); }
	}

	public class HarvesterBotModuleSP : ConditionalTrait<HarvesterBotModuleSPInfo>, IBotTick
	{
		readonly World world;
		readonly Player player;
		readonly Func<Actor, bool> unitCannotBeOrdered;

		IResourceLayer resourceLayer;
		ResourceClaimLayer claimLayer;
		int scanForIdleHarvestersTicks;
		bool initialized;
		CPos[] resourseCenters;

		public HarvesterBotModuleSP(Actor self, HarvesterBotModuleSPInfo info)
			: base(info)
		{
			world = self.World;
			player = self.Owner;
			unitCannotBeOrdered = a => a.Owner != self.Owner || a.IsDead || !a.IsInWorld;
		}

		protected override void TraitEnabled(Actor self)
		{
			if (!initialized)
			{
				resourceLayer = world.WorldActor.TraitOrDefault<IResourceLayer>();
				claimLayer = world.WorldActor.TraitOrDefault<ResourceClaimLayer>();
				resourseCenters = self.World.Actors.Where(a => Info.ResourseCenterType.Contains(a.Info.Name)).Select(a => a.Location).OrderByDescending(c => (c - player.HomeLocation).LengthSquared).ToArray();

				initialized = true;
			}

			// Avoid all AIs scanning for idle harvesters on the same tick, randomize their initial scan delay.
			scanForIdleHarvestersTicks = world.LocalRandom.Next(0, Info.AssignRoleInterval);
		}

		void IBotTick.BotTick(IBot bot)
		{
			if (resourceLayer == null || resourceLayer.IsEmpty)
				return;

			if (--scanForIdleHarvestersTicks > 0)
				return;

			scanForIdleHarvestersTicks = Info.AssignRoleInterval;

			var harvesters = world.ActorsWithTrait<Harvester>().Where(at => !unitCannotBeOrdered(at.Actor));

			// Find idle harvesters and give them orders:
			foreach (var h in harvesters)
			{
				if (!h.Actor.IsIdle && !(h.Actor.CurrentActivity is FlyIdle))
				{
					if (h.Actor.CurrentActivity is not FindAndDeliverResources act || !act.LastSearchFailed)
						continue;
				}

				// Tell the idle harvester to quit slacking:
				var newSafeResourcePatch = FindNextResource(h);
				if (newSafeResourcePatch != Target.Invalid)
				{
					AIUtils.BotDebug($"AI: Harvester {h.Actor} is idle. Ordering to {newSafeResourcePatch} in search for new resources.");
					bot.QueueOrder(new Order("Harvest", h.Actor, newSafeResourcePatch, false));
				}
				else
				{
					// If no resource, tell harvester to stop scanning by itself
					AIUtils.BotDebug($"AI: no valid resource for Harvester {h.Actor}.");
					bot.QueueOrder(new Order("Stop", h.Actor, false));
				}

				break;
			}
		}

		Target FindNextResource(TraitPair<Harvester> harv)
		{
			var mobile = harv.Actor.TraitOrDefault<Mobile>();
			var maxResourceCell = 0;
			var maxResourcetragetcell = Target.Invalid;

			foreach (var loc in resourseCenters.OrderBy(c => (c - harv.Actor.Location).LengthSquared))
			{
				if (world.FindActorsInCircle(world.Map.CenterOfCell(loc), Info.HarvesterEnemyAvoidanceRadius).Any(a => !a.IsDead && a.IsInWorld && a.Owner.RelationshipWith(player) == PlayerRelationship.Enemy))
					continue;

				if (mobile != null && !mobile.PathFinder.PathExistsForLocomotor(mobile.Locomotor, harv.Actor.Location, loc))
					continue;

				var harvestable = world.Map.FindTilesInAnnulus(loc, 0, Info.ResourseCenterSearchRangeInCells).Where(c => harv.Trait.CanHarvestCell(c) && claimLayer.CanClaimCell(harv.Actor, c)).ToArray();

				// If the resource field is rich enough then we will just stop checking and harvest.
				if (harvestable.Length >= Info.FavoredHarvestableCell)
					return Target.FromCell(world, harvestable.Random(world.LocalRandom));

				// If not, we are going to find a best location by comparing the resource cells around.
				if (harvestable.Length > maxResourceCell)
				{
					maxResourceCell = harvestable.Length;
					maxResourcetragetcell = Target.FromCell(world, harvestable.Random(world.LocalRandom));
				}
			}

			return maxResourcetragetcell;
		}
	}
}
