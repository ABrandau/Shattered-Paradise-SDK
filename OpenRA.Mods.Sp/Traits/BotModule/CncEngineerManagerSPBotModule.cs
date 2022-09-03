#region Copyright & License Information
/*
 * Copyright 2007-2022 The OpenRA Developers (see AUTHORS)
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
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	[Desc("Manages AI traditional cnc engineer logic. Only consider closest buildings and AI stuck problem.")]
	public class CncEngineerSPBotModuleInfo : ConditionalTraitInfo
	{
		[Desc("Actor types that can capture other actors (via `Captures`).",
			"Leave this empty to disable capturing.")]
		public readonly HashSet<string> EngineerActorTypes = new HashSet<string>();

		[Desc("Actor types that can be targeted for capturing.",
			"Leave this empty to include all actors.")]
		public readonly HashSet<string> CapturableActorTypes = new HashSet<string>();

		[Desc("Player relationships that capturers should attempt to target.")]
		public readonly PlayerRelationship CapturableRelationships = PlayerRelationship.Enemy | PlayerRelationship.Neutral;

		[Desc("Actor types that can be targeted for engineer repairing.",
			"Leave this empty to include all actors.")]
		public readonly HashSet<string> RepairableActorTypes = new HashSet<string>();

		[Desc("Engineer repair actor when at this damage state.")]
		public readonly DamageState RepairableDamageState = DamageState.Heavy;

		[Desc("Actor types that can be targeted for bridge repairing.",
			"Leave this empty to include all actors.")]
		public readonly HashSet<string> RepairableHutActorTypes = new HashSet<string>();

		[Desc("Minimum delay (in ticks) between trying to capture with CapturingActorTypes.")]
		public readonly int AssignRoleDelay = 120;

		public override object Create(ActorInitializer init) { return new CncEngineerManagerSPBotModule(init.Self, this); }
	}

	public class CncEngineerManagerSPBotModule : ConditionalTrait<CncEngineerSPBotModuleInfo>, IBotTick
	{
		readonly World world;
		readonly Player player;
		readonly Predicate<Actor> unitCannotBeOrderedOrIsIdle;
		readonly Predicate<Actor> unitCannotBeOrdered;

		// Units that the bot already knows about and has given a capture order. Any unit not on this list needs to be given a new order.
		readonly List<UnitWposWrapper> activeEngineers = new List<UnitWposWrapper>();
		readonly List<Actor> stuckEngineers = new List<Actor>();
		int minAssignRoleDelayTicks;
		int actionSelection;

		public CncEngineerManagerSPBotModule(Actor self, CncEngineerSPBotModuleInfo info)
			: base(info)
		{
			world = self.World;
			player = self.Owner;

			if (world.Type == WorldType.Editor)
				return;

			unitCannotBeOrdered = a => a.Owner != player || a.IsDead || !a.IsInWorld;
			unitCannotBeOrderedOrIsIdle = a => unitCannotBeOrdered(a) || a.IsIdle;
		}

		protected override void TraitEnabled(Actor self)
		{
			// Avoid all AIs reevaluating assignments on the same tick, randomize their initial evaluation delay.
			minAssignRoleDelayTicks = world.LocalRandom.Next(0, Info.AssignRoleDelay);
		}

		void IBotTick.BotTick(IBot bot)
		{
			if (--minAssignRoleDelayTicks <= 0)
			{
				minAssignRoleDelayTicks = Info.AssignRoleDelay;

				activeEngineers.RemoveAll(u => unitCannotBeOrderedOrIsIdle(u.Actor));
				stuckEngineers.RemoveAll(a => unitCannotBeOrdered(a));
				for (var i = 0; i < activeEngineers.Count; i++)
				{
					var engineer = activeEngineers[i];
					if (engineer.Actor.CurrentActivity.ChildActivity != null && engineer.Actor.CurrentActivity.ChildActivity.ActivityType == Activities.ActivityType.Move && engineer.Actor.CenterPosition == engineer.WPos)
					{
						stuckEngineers.Add(engineer.Actor);
						bot.QueueOrder(new Order("Stop", engineer.Actor, false));
						activeEngineers.Remove(engineer);
					}

					engineer.WPos = engineer.Actor.CenterPosition;
				}

				actionSelection = (actionSelection + 1) % 3;
				if (actionSelection == 0)
					QueueRepairBuildingOrders(bot);
				else if (actionSelection == 1)
					QueueRepairBridgeOrders(bot);
				else
					QueueCaptureOrders(bot);
			}
		}

		void QueueCaptureOrders(IBot bot)
		{
			if (Info.EngineerActorTypes.Count == 0 || player.WinState != WinState.Undefined)
				return;

			var capturers = world.ActorsHavingTrait<IPositionable>()
				.Where(a => Info.EngineerActorTypes.Contains(a.Info.Name) && a.Owner == player && !unitCannotBeOrdered(a) && !stuckEngineers.Contains(a) && a.Info.HasTraitInfo<CapturesInfo>())
				.Select(a => new TraitPair<CaptureManager>(a, a.TraitOrDefault<CaptureManager>()))
				.Where(tp => tp.Trait != null)
				.ToArray();

			if (capturers.Length == 0)
				return;

			foreach (var capturer in capturers)
			{
				var inactivatedActor = true;

				foreach (var u in activeEngineers)
				{
					if (u.Actor == capturer.Actor)
					{
						inactivatedActor = false;
						break;
					}
				}

				if (!inactivatedActor)
					continue;

				var mobile = capturer.Actor.TraitOrDefault<Mobile>();
				if (mobile == null)
					continue;

				var targetActor = world.Actors.Where(target =>
				{
					if (Info.CapturableActorTypes != null && !Info.CapturableActorTypes.Contains(target.Info.Name))
						return false;

					var captureManager = target.TraitOrDefault<CaptureManager>();
					if (captureManager == null)
						return false;

					if (!mobile.PathFinder.PathExistsForLocomotor(mobile.Locomotor, capturer.Actor.Location, target.Location))
						return false;

					return capturers.Any(tp => captureManager.CanBeTargetedBy(target, tp.Actor, tp.Trait));
				}).ClosestTo(capturer.Actor);

				bot.QueueOrder(new Order("CaptureActor", capturer.Actor, Target.FromActor(targetActor), true));
				AIUtils.BotDebug("AI ({0}): Ordered {1} to capture {2}", player.ClientIndex, capturer.Actor, targetActor);
				activeEngineers.Add(new UnitWposWrapper(capturer.Actor));
				break;
			}
		}

		void QueueRepairBuildingOrders(IBot bot)
		{
			if (Info.EngineerActorTypes.Count == 0 || player.WinState != WinState.Undefined)
				return;

			var repairers = world.ActorsHavingTrait<IPositionable>()
				.Where(a => Info.EngineerActorTypes.Contains(a.Info.Name) && a.Owner == player && !unitCannotBeOrdered(a) && !stuckEngineers.Contains(a) && a.Info.HasTraitInfo<EngineerRepairInfo>())
				.ToArray();

			if (repairers.Length == 0)
				return;

			foreach (var r in repairers)
			{
				var inactivatedActor = true;

				foreach (var u in activeEngineers)
				{
					if (u.Actor == r)
					{
						inactivatedActor = false;
						break;
					}
				}

				if (!inactivatedActor)
					continue;

				var mobile = r.TraitOrDefault<Mobile>();
				if (mobile == null)
					continue;

				var targetActor = world.Actors.Where(target =>
				{
					if (Info.CapturableActorTypes != null && !Info.RepairableActorTypes.Contains(target.Info.Name))
						return false;

					if (target.Owner.RelationshipWith(r.Owner) != PlayerRelationship.Ally)
						return false;

					var health = target.TraitOrDefault<IHealth>();

					if (health == null || health.DamageState < Info.RepairableDamageState)
						return false;

					var buildingrepair = target.TraitOrDefault<EngineerRepairable>();
					if (buildingrepair == null)
						return false;

					if (!mobile.PathFinder.PathExistsForLocomotor(mobile.Locomotor, r.Location, target.Location))
						return false;

					return true;
				}).ClosestTo(r);

				bot.QueueOrder(new Order("EngineerRepair", r, Target.FromActor(targetActor), true));
				AIUtils.BotDebug("AI ({0}): Ordered {1} to Repair {2}", player.ClientIndex, r, targetActor);
				activeEngineers.Add(new UnitWposWrapper(r));
				break;
			}
		}

		void QueueRepairBridgeOrders(IBot bot)
		{
			if (Info.EngineerActorTypes.Count == 0 || player.WinState != WinState.Undefined)
				return;

			var brigdeRepairers = world.ActorsHavingTrait<IPositionable>()
				.Where(a => Info.EngineerActorTypes.Contains(a.Info.Name) && a.Owner == player && !unitCannotBeOrdered(a) && !stuckEngineers.Contains(a) && a.Info.HasTraitInfo<RepairsBridgesInfo>())
				.ToArray();

			if (brigdeRepairers.Length == 0)
				return;

			foreach (var r in brigdeRepairers)
			{
				var inactivatedActor = true;

				foreach (var u in activeEngineers)
				{
					if (u.Actor == r)
					{
						inactivatedActor = false;
						break;
					}
				}

				if (!inactivatedActor)
					continue;

				var mobile = r.TraitOrDefault<Mobile>();
				if (mobile == null)
					continue;

				var targetActor = world.Actors.Where(target =>
				{
					if (Info.CapturableActorTypes != null && !Info.RepairableHutActorTypes.Contains(target.Info.Name))
						return false;

					var repairableBridgeHut = target.TraitOrDefault<BridgeHut>();

					var legacyRepairableBridgeHut = target.TraitOrDefault<LegacyBridgeHut>();

					if (repairableBridgeHut == null && legacyRepairableBridgeHut == null)
						return false;

					if (repairableBridgeHut != null && repairableBridgeHut.BridgeDamageState < DamageState.Dead)
						return false;

					if (legacyRepairableBridgeHut != null && legacyRepairableBridgeHut.BridgeDamageState < DamageState.Dead)
						return false;

					if (!mobile.PathFinder.PathExistsForLocomotor(mobile.Locomotor, r.Location, target.Location))
						return false;

					return true;
				}).ClosestTo(r);

				bot.QueueOrder(new Order("RepairBridge", r, Target.FromActor(targetActor), true));
				AIUtils.BotDebug("AI ({0}): Ordered {1} to repair bridge hut {2}", player.ClientIndex, r, targetActor);
				activeEngineers.Add(new UnitWposWrapper(r));
				break;
			}
		}
	}
}
