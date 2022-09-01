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
	[Desc("Manages AI capturing logic. Only consider closest buildings and AI stuck problem.")]
	public class CaptureManagerSPBotModuleInfo : ConditionalTraitInfo
	{
		[Desc("Actor types that can capture other actors (via `Captures`).",
			"Leave this empty to disable capturing.")]
		public readonly HashSet<string> CapturingActorTypes = new HashSet<string>();

		[Desc("Actor types that can be targeted for capturing.",
			"Leave this empty to include all actors.")]
		public readonly HashSet<string> CapturableActorTypes = new HashSet<string>();

		[Desc("Minimum delay (in ticks) between trying to capture with CapturingActorTypes.")]
		public readonly int MinimumCaptureDelay = 200;

		[Desc("Player relationships that capturers should attempt to target.")]
		public readonly PlayerRelationship CapturableRelationships = PlayerRelationship.Enemy | PlayerRelationship.Neutral;

		public override object Create(ActorInitializer init) { return new CaptureManagerSPBotModule(init.Self, this); }
	}

	public class CaptureManagerSPBotModule : ConditionalTrait<CaptureManagerSPBotModuleInfo>, IBotTick
	{
		readonly World world;
		readonly Player player;
		readonly Predicate<Actor> unitCannotBeOrderedOrIsIdle;
		readonly Predicate<Actor> unitCannotBeOrdered;

		// Units that the bot already knows about and has given a capture order. Any unit not on this list needs to be given a new order.
		readonly List<UnitWposWrapper> activeCapturers = new List<UnitWposWrapper>();
		readonly List<Actor> stuckCapturers = new List<Actor>();
		int minCaptureDelayTicks;

		public CaptureManagerSPBotModule(Actor self, CaptureManagerSPBotModuleInfo info)
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
			minCaptureDelayTicks = world.LocalRandom.Next(0, Info.MinimumCaptureDelay);
		}

		void IBotTick.BotTick(IBot bot)
		{
			if (--minCaptureDelayTicks <= 0)
			{
				minCaptureDelayTicks = Info.MinimumCaptureDelay;

				activeCapturers.RemoveAll(u => unitCannotBeOrderedOrIsIdle(u.Actor));
				stuckCapturers.RemoveAll(a => unitCannotBeOrdered(a));
				for (var i = 0; i < activeCapturers.Count; i++)
				{
					var capturer = activeCapturers[i];
					if (capturer.Actor.CurrentActivity.ChildActivity != null && capturer.Actor.CurrentActivity.ChildActivity.ActivityType == Activities.ActivityType.Move && capturer.Actor.CenterPosition == capturer.WPos)
					{
						stuckCapturers.Add(capturer.Actor);
						bot.QueueOrder(new Order("Stop", capturer.Actor, false));
						activeCapturers.Remove(capturer);
					}

					capturer.WPos = capturer.Actor.CenterPosition;
				}

				QueueCaptureOrders(bot);
			}
		}

		void QueueCaptureOrders(IBot bot)
		{
			if (Info.CapturingActorTypes.Count == 0 || player.WinState != WinState.Undefined)
				return;

			var capturers = world.ActorsHavingTrait<IPositionable>()
				.Where(a => Info.CapturingActorTypes.Contains(a.Info.Name) && a.Owner == player && !unitCannotBeOrdered(a) && !stuckCapturers.Contains(a) && a.Info.HasTraitInfo<CapturesInfo>())
				.Select(a => new TraitPair<CaptureManager>(a, a.TraitOrDefault<CaptureManager>()))
				.Where(tp => tp.Trait != null)
				.ToArray();

			if (capturers.Length == 0)
				return;

			foreach (var capturer in capturers)
			{
				var inactivatedActor = true;

				foreach (var u in activeCapturers)
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
				activeCapturers.Add(new UnitWposWrapper(capturer.Actor));
				break;
			}
		}
	}
}
