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
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	[Desc("Manages AI load unit.")]
	public class LoadCargoBotModuleInfo : ConditionalTraitInfo
	{
		public readonly HashSet<string> Transports = default;
		public readonly HashSet<string> Passengers = default;
		public readonly bool OnlyEnterOwnerPlayer = true;
		public readonly string EnterOrderName = "EnterTransport";
		public readonly int ScanTick = 300;
		public readonly DamageState ValidDamageState = DamageState.Heavy;

		public override object Create(ActorInitializer init) { return new LoadCargoBotModule(init.Self, this); }
	}

	public class LoadCargoBotModule : ConditionalTrait<LoadCargoBotModuleInfo>, IBotTick
	{
		readonly World world;
		readonly Player player;
		readonly Predicate<Actor> unitCannotBeOrdered;
		readonly Predicate<Actor> unitCannotBeOrderedOrIsBusy;
		readonly Predicate<Actor> unitCannotBeOrderedOrIsIdle;
		readonly Predicate<Actor> invalidTransport;

		readonly List<UnitWposWrapper> activePassengers = new List<UnitWposWrapper>();
		readonly List<Actor> stuckPassengers = new List<Actor>();
		int minAssignRoleDelayTicks;

		public LoadCargoBotModule(Actor self, LoadCargoBotModuleInfo info)
			: base(info)
		{
			world = self.World;
			player = self.Owner;
			if (info.OnlyEnterOwnerPlayer)
				invalidTransport = a => a == null || a.IsDead || !a.IsInWorld || a.Owner != player;
			else
				invalidTransport = a => a == null || a.IsDead || !a.IsInWorld || a.Owner.RelationshipWith(player) == PlayerRelationship.Ally;
			unitCannotBeOrdered = a => a == null || a.IsDead || !a.IsInWorld || a.Owner != player;
			unitCannotBeOrderedOrIsBusy = a => unitCannotBeOrdered(a) || (!a.IsIdle && !(a.CurrentActivity is FlyIdle));
			unitCannotBeOrderedOrIsIdle = a => unitCannotBeOrdered(a) || a.IsIdle || a.CurrentActivity is FlyIdle;
		}

		protected override void TraitEnabled(Actor self)
		{
			// Avoid all AIs reevaluating assignments on the same tick, randomize their initial evaluation delay.
			minAssignRoleDelayTicks = world.LocalRandom.Next(0, Info.ScanTick);
		}

		void IBotTick.BotTick(IBot bot)
		{
			if (--minAssignRoleDelayTicks <= 0)
			{
				minAssignRoleDelayTicks = Info.ScanTick;

				activePassengers.RemoveAll(u => unitCannotBeOrderedOrIsIdle(u.Actor));
				stuckPassengers.RemoveAll(a => unitCannotBeOrdered(a));
				for (var i = 0; i < activePassengers.Count; i++)
				{
					var p = activePassengers[i];
					if (p.Actor.CurrentActivity.ChildActivity != null && p.Actor.CurrentActivity.ChildActivity.ActivityType == Activities.ActivityType.Move && p.Actor.CenterPosition == p.WPos)
					{
						stuckPassengers.Add(p.Actor);
						bot.QueueOrder(new Order("Stop", p.Actor, false));
						activePassengers.RemoveAt(i);
						i--;
					}

					p.WPos = p.Actor.CenterPosition;
				}

				var tcs = world.ActorsWithTrait<Cargo>().Where(
				at =>
				{
					var health = at.Actor.TraitOrDefault<IHealth>()?.DamageState;
					return Info.Transports.Contains(at.Actor.Info.Name) && !invalidTransport(at.Actor)
					&& at.Trait.HasSpace(1) && (health == null || health < Info.ValidDamageState);
				}).ToArray();

				if (tcs.Length == 0)
					return;

				var tc = tcs.Random(world.LocalRandom);
				var cargo = tc.Trait;
				var transport = tc.Actor;
				var spaceTaken = 0;

				var passengers = world.ActorsWithTrait<Passenger>().Where(at => !unitCannotBeOrderedOrIsBusy(at.Actor) && Info.Passengers.Contains(at.Actor.Info.Name) && !stuckPassengers.Contains(at.Actor) && cargo.HasSpace(at.Trait.Info.Weight))
					.OrderBy(at => (at.Actor.CenterPosition - transport.CenterPosition).HorizontalLengthSquared);

				var orderedActors = new List<Actor>();

				foreach (var p in passengers)
				{
					var mobile = p.Actor.TraitOrDefault<Mobile>();
					if (mobile == null || !mobile.PathFinder.PathExistsForLocomotor(mobile.Locomotor, p.Actor.Location, transport.Location))
						continue;

					if (cargo.HasSpace(spaceTaken + p.Trait.Info.Weight))
					{
						spaceTaken += p.Trait.Info.Weight;
						orderedActors.Add(p.Actor);
						activePassengers.Add(new UnitWposWrapper(p.Actor));
					}

					if (!cargo.HasSpace(spaceTaken + 1))
						break;
				}

				if (orderedActors.Count > 0)
					bot.QueueOrder(new Order(Info.EnterOrderName, null, Target.FromActor(transport), false, groupedActors: orderedActors.ToArray()));
			}
		}
	}
}
