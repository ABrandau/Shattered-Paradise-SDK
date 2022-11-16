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
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Sp.Traits
{
	[Flags]
	public enum AttackRequires
	{
		None = 0,
		CargoLoadedIfPossible = 1
	}

	public class SendUnitToAttackBotModuleInfo : ConditionalTraitInfo
	{
		[Desc("Actors used for attack, and their base desire provided for attack desire.",
			"When desire reach 100, AI will send them to attack.")]
		public readonly Dictionary<string, int> ActorsAndAttackDesire = default;

		[Desc("Target types that can be targeted.")]
		public readonly BitSet<TargetableType> ValidTargets = new BitSet<TargetableType>("Building");

		[Desc("Target types that can't be targeted.")]
		public readonly BitSet<TargetableType> InvalidTargets;

		[Desc("Should attack the furthest or closest target.")]
		public readonly bool AttackFurthest = true;

		[Desc("Attack order name.")]
		public readonly string AttackOrderName = "Attack";

		[Desc("Find target and attack actor in this interval.")]
		public readonly int ScanTick = 400;

		[Desc("The total attack desire increase this amount oer scan")]
		public readonly int AttackDesireIncreasedPerScan = 10;

		[Desc("Filters units don't meet the requires.")]
		public readonly AttackRequires AttackRequires = AttackRequires.CargoLoadedIfPossible;

		public override object Create(ActorInitializer init) { return new SendUnitToAttackBotModule(init.Self, this); }
	}

	public class SendUnitToAttackBotModule : ConditionalTrait<SendUnitToAttackBotModuleInfo>, IBotTick
	{
		readonly World world;
		readonly Player player;
		readonly Predicate<Actor> unitCannotBeOrdered;
		readonly Predicate<Actor> unitCannotBeOrderedOrIsBusy;
		readonly Predicate<Actor> unitCannotBeOrderedOrIsIdle;
		readonly Predicate<Actor> isInvalidActor;

		readonly List<UnitWposWrapper> activeActors = new List<UnitWposWrapper>();
		readonly List<Actor> stuckActors = new List<Actor>();
		int minAssignRoleDelayTicks;
		Player targetPlayer;
		int desireIncreased;

		public SendUnitToAttackBotModule(Actor self, SendUnitToAttackBotModuleInfo info)
		: base(info)
		{
			world = self.World;
			player = self.Owner;
			isInvalidActor = a => a == null || a.IsDead || !a.IsInWorld || a.Owner != targetPlayer;
			unitCannotBeOrdered = a => a == null || a.IsDead || !a.IsInWorld || a.Owner != player;
			unitCannotBeOrderedOrIsBusy = a => unitCannotBeOrdered(a) || (!a.IsIdle && !(a.CurrentActivity is FlyIdle));
			unitCannotBeOrderedOrIsIdle = a => unitCannotBeOrdered(a) || a.IsIdle || a.CurrentActivity is FlyIdle;
			desireIncreased = 0;
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

				activeActors.RemoveAll(u => unitCannotBeOrderedOrIsIdle(u.Actor));
				stuckActors.RemoveAll(a => unitCannotBeOrdered(a));
				for (var i = 0; i < activeActors.Count; i++)
				{
					var p = activeActors[i];
					if (p.Actor.CurrentActivity.ChildActivity != null && p.Actor.CurrentActivity.ChildActivity.ActivityType == Activities.ActivityType.Move && p.Actor.CenterPosition == p.WPos)
					{
						stuckActors.Add(p.Actor);
						bot.QueueOrder(new Order("Stop", p.Actor, false));
						activeActors.RemoveAt(i);
						i--;
					}

					p.WPos = p.Actor.CenterPosition;
				}

				var shouldAttackdesire = 0;
				var actors = world.ActorsWithTrait<AttackBase>().Select(at => at.Actor).Where(a =>
				{
					if (Info.ActorsAndAttackDesire.ContainsKey(a.Info.Name) && !unitCannotBeOrderedOrIsBusy(a) && !stuckActors.Contains(a))
					{
						if (Info.AttackRequires.HasFlag(AttackRequires.CargoLoadedIfPossible) && a.Info.HasTraitInfo<CargoInfo>())
						{
							if (a.Trait<Cargo>().IsEmpty())
								return false;
							else
							{
								shouldAttackdesire += Info.ActorsAndAttackDesire[a.Info.Name];
								return true;
							}
						}
						else
						{
							shouldAttackdesire += Info.ActorsAndAttackDesire[a.Info.Name];
							return true;
						}
					}

					return false;
				}).ToList();

				if (actors.Count == 0)
					desireIncreased = 0;
				else
					desireIncreased += Info.AttackDesireIncreasedPerScan;

				if (desireIncreased + shouldAttackdesire < 100)
					return;

				// Randomly choose enemy player to attack
				targetPlayer = world.Players.Where(p => p.RelationshipWith(player) == PlayerRelationship.Enemy).Random(world.LocalRandom);

				var targets = world.Actors.Where(a =>
				{
					if (isInvalidActor(a))
						return false;

					var t = a.GetAllTargetTypes();

					if (!Info.ValidTargets.Overlaps(t) || Info.InvalidTargets.Overlaps(t))
						return false;

					var hasModifier = false;
					var visModifiers = a.TraitsImplementing<IVisibilityModifier>();
					foreach (var v in visModifiers)
					{
						if (v.IsVisible(a, player))
							return true;

						hasModifier = true;
					}

					return !hasModifier;
				});

				if (Info.AttackFurthest)
					targets = targets.OrderByDescending(a => (a.CenterPosition - actors[0].CenterPosition).HorizontalLengthSquared);
				else
					targets = targets.OrderBy(a => (a.CenterPosition - actors[0].CenterPosition).HorizontalLengthSquared);

				foreach (var t in targets)
				{
					var orderedActors = new List<Actor>();

					foreach (var a in actors)
					{
						if (!a.Info.HasTraitInfo<AircraftInfo>())
						{
							var mobile = a.TraitOrDefault<Mobile>();
							if (mobile == null || !mobile.PathFinder.PathExistsForLocomotor(mobile.Locomotor, a.Location, t.Location))
								continue;
						}

						orderedActors.Add(a);
						activeActors.Add(new UnitWposWrapper(a));
					}

					actors.RemoveAll(a => orderedActors.Contains(a));

					if (orderedActors.Count > 0)
						bot.QueueOrder(new Order(Info.AttackOrderName, null, Target.FromActor(t), false, groupedActors: orderedActors.ToArray()));

					if (actors.Count == 0)
						break;
				}
			}
		}
	}
}
