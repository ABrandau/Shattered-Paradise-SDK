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
using OpenRA.Activities;
using OpenRA.Mods.Cnc.Traits;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Sp.Traits
{
	[Desc("Manages AI minelayer unit related with Minelayer traits.")]
	public class MinelayerBotModuleInfo : ConditionalTraitInfo
	{
		[Desc("Enemy target types to never target when calculate the mine layer route.")]
		public readonly BitSet<TargetableType> IgnoredEnemyTargetTypes = default;

		[Desc("Victim target types that considering lay mine to enemy location instead of victim location.")]
		public readonly BitSet<TargetableType> UseEnemyLocationTargetTypes = default;

		[Desc("Actor types that used for mine laying, must have Minelayer.")]
		public readonly HashSet<string> Minelayers = default;

		[Desc("Scan this amount of suitable actors and lay mine to a location.")]
		public readonly int MaxMinelayersPerAssign = 1;

		[Desc("Locomotor types that used for mine laying.")]
		public readonly string SuggestedMinelayersLocomotor = null;

		[Desc("Scan suitable actors and target in this interval.")]
		public readonly int ScanTick = 331;

		[Desc("Minelayer radius.")]
		public readonly int MineFieldRadius = 1;

		[Desc("Minefield is laying away from those target type belong to allied.")]
		public readonly BitSet<TargetableType> AwayFromAlliedTargetTypes = default;

		[Desc("Minefield is laying away from those target type belong to enemy.")]
		public readonly BitSet<TargetableType> AwayFromEnemyTargetTypes = default;

		[Desc("Minefield is laying this cell distance away from those target type belong to AwayFromAlliedTargettype and AwayFromEnemyTargettype.")]
		public readonly int AwayFromDistance = 9;

		[Desc("Bot module merge 2 favorite minefield position into 1 when they are within this distance.")]
		public readonly int FavoritePositionDistance = 6;

		public override object Create(ActorInitializer init) { return new MinelayerBotModule(init.Self, this); }
	}

	public class MinelayerBotModule : ConditionalTrait<MinelayerBotModuleInfo>, IBotTick, IBotRespondToAttack
	{
		const int maxPositionCacheLength = 5;
		const int RepeatedAltertTicks = 40;

		readonly World world;
		readonly Player player;
		readonly Predicate<Actor> unitCannotBeOrdered;
		readonly Predicate<Actor> unitCannotBeOrderedOrIsBusy;
		readonly Predicate<Actor> unitCannotBeOrderedOrIsIdle;

		readonly List<UnitWposWrapper> activeMinelayers = new List<UnitWposWrapper>();
		readonly List<Actor> stuckMinelayers = new List<Actor>();
		int minAssignRoleDelayTicks;
		CPos?[] conflictPositionQueue;
		CPos?[] favoritePositions;

		int conflictPositionLength;
		int favoritePositionsLength;
		int currentFavoritePositionIndex;
		int alertedTicks;

		Locomotor locomotor;

		public MinelayerBotModule(Actor self, MinelayerBotModuleInfo info)
		: base(info)
		{
			world = self.World;
			player = self.Owner;
			unitCannotBeOrdered = a => a == null || a.IsDead || !a.IsInWorld || a.Owner != player;
			unitCannotBeOrderedOrIsBusy = a => unitCannotBeOrdered(a) || (!a.IsIdle && !(a.CurrentActivity is FlyIdle));
			unitCannotBeOrderedOrIsIdle = a => unitCannotBeOrdered(a) || a.IsIdle || a.CurrentActivity is FlyIdle;
			conflictPositionQueue = new CPos?[maxPositionCacheLength] { null, null, null, null, null };
			favoritePositions = new CPos?[maxPositionCacheLength] { null, null, null, null, null };
		}

		protected override void TraitEnabled(Actor self)
		{
			// Avoid all AIs reevaluating assignments on the same tick, randomize their initial evaluation delay.
			minAssignRoleDelayTicks = world.LocalRandom.Next(0, Info.ScanTick);
			alertedTicks = 0;
			conflictPositionLength = 0;
			favoritePositionsLength = 0;
			currentFavoritePositionIndex = 0;
			locomotor = self.World.WorldActor.TraitsImplementing<Locomotor>().First(l => l.Info.Name == Info.SuggestedMinelayersLocomotor);
		}

		void IBotTick.BotTick(IBot bot)
		{
			if (alertedTicks > 0)
				alertedTicks--;

			if (--minAssignRoleDelayTicks <= 0)
			{
				minAssignRoleDelayTicks = Info.ScanTick;

				activeMinelayers.RemoveAll(u => unitCannotBeOrderedOrIsIdle(u.Actor));
				stuckMinelayers.RemoveAll(a => unitCannotBeOrdered(a));
				for (var i = 0; i < activeMinelayers.Count; i++)
				{
					var p = activeMinelayers[i];
					if (p.Actor.CurrentActivity.ChildActivity != null && p.Actor.CurrentActivity.ChildActivity.ActivityType == ActivityType.Move && p.Actor.CenterPosition == p.WPos)
					{
						stuckMinelayers.Add(p.Actor);
						bot.QueueOrder(new Order("Stop", p.Actor, false));
						activeMinelayers.RemoveAt(i);
						i--;
					}

					p.WPos = p.Actor.CenterPosition;
				}

				var minelayingPosition = CPos.Zero;
				var useFavoritePosition = false;

				while (conflictPositionLength > 0)
				{
					minelayingPosition = conflictPositionQueue[0].Value;
					if (HasInvalidActorInCircle(world.Map.CenterOfCell(minelayingPosition), WDist.FromCells(Info.AwayFromDistance)))
						DequeueFirstConflictPosition();
					else
						break;
				}

				if (conflictPositionLength == 0)
				{
					if (favoritePositionsLength == 0)
						return;

					useFavoritePosition = true;
					while (favoritePositionsLength > 0)
					{
						minelayingPosition = favoritePositions[currentFavoritePositionIndex].Value;
						if (HasInvalidActorInCircle(world.Map.CenterOfCell(minelayingPosition), WDist.FromCells(Info.AwayFromDistance)))
						{
							DeleteCurrentFavoritePosition();
							if (favoritePositionsLength == 0)
								return;
						}
						else
							break;
					}
				}

				var ats = world.ActorsWithTrait<Minelayer>().Where(at => !unitCannotBeOrderedOrIsBusy(at.Actor)).ToArray();

				if (ats.Length == 0)
					return;

				var orderedActors = new List<Actor>();

				foreach (var at in ats)
				{
					var mobile = at.Actor.TraitOrDefault<Mobile>();
					if (mobile == null || !mobile.PathFinder.PathExistsForLocomotor(locomotor, at.Actor.Location, minelayingPosition))
						continue;

					orderedActors.Add(at.Actor);

					if (orderedActors.Count >= Info.MaxMinelayersPerAssign)
						break;
				}

				if (orderedActors.Count > 0)
				{
					if (useFavoritePosition)
					{
						AIUtils.BotDebug("AI ({0}): Use favorite position {1} at index {2}", player.ClientIndex, minelayingPosition, currentFavoritePositionIndex);
						NextFavoritePositionIndex();
					}
					else
					{
						DequeueFirstConflictPosition();
						AddPositionToFavoritePositions(minelayingPosition);
						AIUtils.BotDebug("AI ({0}): Use in time conflict position {1}", player.ClientIndex, minelayingPosition);
					}

					var vec = new CVec(Info.MineFieldRadius, Info.MineFieldRadius);
					bot.QueueOrder(new Order("PlaceMinefield", null, Target.FromCell(world, minelayingPosition + vec), false, groupedActors: orderedActors.ToArray()) { ExtraLocation = minelayingPosition - vec });
					bot.QueueOrder(new Order("Move", null, Target.FromCell(world, orderedActors.First().Location), true, groupedActors: orderedActors.ToArray()));
				}
				else
				{
					if (useFavoritePosition)
						DeleteCurrentFavoritePosition();
					else
						DequeueFirstConflictPosition();
				}
			}
		}

		void DequeueFirstConflictPosition()
		{
			for (var i = 1; i < conflictPositionLength; i++)
				conflictPositionQueue[i - 1] = conflictPositionQueue[i];
			conflictPositionQueue[conflictPositionLength - 1] = null;
			conflictPositionLength--;
		}

		void DeleteCurrentFavoritePosition()
		{
			if (favoritePositionsLength == 0)
				return;

			for (var i = currentFavoritePositionIndex; i < favoritePositionsLength - 1; i++)
				favoritePositions[i] = favoritePositions[i + 1];
			favoritePositions[favoritePositionsLength - 1] = null;
			favoritePositionsLength--;

			if (favoritePositionsLength == 0)
				return;

			currentFavoritePositionIndex = currentFavoritePositionIndex % favoritePositionsLength;
		}

		void AddPositionToFavoritePositions(CPos cpos)
		{
			var favoriteDistSquare = Info.FavoritePositionDistance * Info.FavoritePositionDistance;
			var closestIndex = 0;
			var closestDistSquare = int.MaxValue;
			for (var i = 0; i < favoritePositionsLength; i++)
			{
				var lengthsquare = (favoritePositions[i].Value - cpos).LengthSquared;
				if (lengthsquare < closestDistSquare)
				{
					closestIndex = i;
					closestDistSquare = lengthsquare;
				}
			}

			// Add new if there is space
			if (closestDistSquare > favoriteDistSquare && favoritePositionsLength < favoritePositions.Length)
			{
				favoritePositions[favoritePositionsLength] = cpos;
				favoritePositionsLength++;
			}
			else
			{
				var pos = favoritePositions[closestIndex].Value;
				favoritePositions[closestIndex] = (pos - cpos) / 2 + cpos;
			}
		}

		void NextFavoritePositionIndex()
		{
			currentFavoritePositionIndex = (currentFavoritePositionIndex + 1) % favoritePositionsLength;
		}

		bool IsPreferredEnemyUnit(Actor a)
		{
			if (a == null || a.IsDead || player.RelationshipWith(a.Owner) != PlayerRelationship.Enemy || a.Info.HasTraitInfo<HuskInfo>())
				return false;

			var targetTypes = a.GetEnabledTargetTypes();
			return !targetTypes.IsEmpty && !targetTypes.Overlaps(Info.IgnoredEnemyTargetTypes);
		}

		bool HasInvalidActorInCircle(WPos pos, WDist dist)
		{
			return world.FindActorsInCircle(pos, dist).Any(a =>
			{
				if (a.Owner.RelationshipWith(player) == PlayerRelationship.Ally)
				{
					var targetTypes = a.GetEnabledTargetTypes();
					return !targetTypes.IsEmpty && targetTypes.Overlaps(Info.AwayFromAlliedTargetTypes);
				}

				if (a.Owner.RelationshipWith(player) == PlayerRelationship.Enemy)
				{
					var targetTypes = a.GetEnabledTargetTypes();
					return !targetTypes.IsEmpty && targetTypes.Overlaps(Info.AwayFromEnemyTargetTypes);
				}

				return false;
			});
		}

		void IBotRespondToAttack.RespondToAttack(IBot bot, Actor self, AttackInfo e)
		{
			if (alertedTicks > 0 || !IsPreferredEnemyUnit(e.Attacker))
				return;

			alertedTicks = RepeatedAltertTicks;

			var hasInvalidActor = HasInvalidActorInCircle(self.CenterPosition, WDist.FromCells(Info.AwayFromDistance));

			if (hasInvalidActor)
				return;

			var targetTypes = self.GetEnabledTargetTypes();
			CPos pos;
			if (!targetTypes.IsEmpty && targetTypes.Overlaps(Info.UseEnemyLocationTargetTypes))
				pos = e.Attacker.Location;
			else
				pos = self.Location;

			if (conflictPositionLength < maxPositionCacheLength)
			{
				conflictPositionQueue[conflictPositionLength] = pos;
				conflictPositionLength++;
			}
			else
				conflictPositionQueue[maxPositionCacheLength - 1] = pos;
		}
	}
}
