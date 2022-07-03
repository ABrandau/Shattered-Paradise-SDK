#region Copyright & License Information
/*
 * Copyright 2015- OpenRA.Mods.AS Developers (see AUTHORS)
 * This file is a part of a third-party plugin for OpenRA, which is
 * free software. It is made available to you under the terms of the
 * GNU General Public License as published by the Free Software
 * Foundation. For more information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Mods.AS.Traits;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

/*
 * Needs base engine modification.
 * In AttackOmni.cs, SetTarget() made public.
 * In Mobile.cs, OccupySpace (true by default) added. Aircraft trait for a dummy unit wasn't the greatest idea as they fly over anything.
 * Move.cs, uses my PR which isn't in bleed yet. (PR to make Move use parent child activity)
 *
 * The difference between Spawner (carrier logic) and this is that
 * carriers have units going in and out of the master actor for reload activities,
 * while MobSpawner doesn't, thus MobSpawner has much simpler code.
 */

/*
 * The code is very similar to Spawner.cs.
 * Sometimes it is neater to have a duplicate than to have wrong inheirtances.
 */

namespace OpenRA.Mods.SP.Traits
{
	// What to do when master is killed or mind controlled
	public enum MobMemberDisposal
	{
		DoNothing,
		KillSlaves,
		GiveSlavesToAttacker
	}

	[Desc("This actor can spawn actors.")]
	public class DroneSpawnerMasterInfo : BaseSpawnerMasterInfo
	{
		[Desc("Spawn at a member, not the nexus?")]
		public readonly bool ExitByBudding = true;

		[Desc("Can the slaves be controlled independently?")]
		public readonly bool SlavesHaveFreeWill = false;

		[Desc("This is a dummy spawner like cin C&C Generals and use virtual position and health.")]
		public readonly bool AggregateHealth = true;

		public readonly int AggregateHealthUpdateDelay = 17; // Just a visual parameter, Doesn't affect the game.

		public override void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			base.RulesetLoaded(rules, ai);

			if (Actors == null || Actors.Length == 0)
				throw new YamlException("Actors is null or empty for MobSpawner for actor type {0}!".F(ai.Name));

			if (InitialActorCount > Actors.Length || InitialActorCount < -1)
				throw new YamlException("MobSpawner can't have more InitialActorCount than the actors defined!");

			if (InitialActorCount == 0 && AggregateHealth == true)
				throw new YamlException("You can't have InitialActorCount == 0 and AggregateHealth");
		}

		public override object Create(ActorInitializer init) { return new DroneSpawnerMaster(init, this); }
	}

	public class DroneSpawnerMaster : BaseSpawnerMaster, INotifyOwnerChanged, ITick,
		IResolveOrder, INotifyAttack
	{
		class DroneSpawnerSlaveEntry : BaseSpawnerSlaveEntry
		{
			public new DroneSpawnerSlave SpawnerSlave;
			public Health Health;
		}

		public new DroneSpawnerMasterInfo Info { get; private set; }

		DroneSpawnerSlaveEntry[] slaveEntries;

		bool hasSpawnedInitialLoad = false;
		int spawnReplaceTicks = 0;

		IPositionable position;
		Aircraft aircraft;
		Health health;

		readonly OpenRA.Activities.ActivityType attacktype = OpenRA.Activities.ActivityType.Attack;
		readonly OpenRA.Activities.ActivityType movetype = OpenRA.Activities.ActivityType.Move;
		readonly OpenRA.Activities.ActivityType abilitytype = OpenRA.Activities.ActivityType.Ability;
		readonly OpenRA.Activities.ActivityType othertype = OpenRA.Activities.ActivityType.Undefined;

		OpenRA.Activities.ActivityType preState;

		public DroneSpawnerMaster(ActorInitializer init, DroneSpawnerMasterInfo info)
			: base(init, info)
		{
			Info = info;
		}

		protected override void Created(Actor self)
		{
			position = self.TraitOrDefault<IPositionable>();
			health = self.Trait<Health>();
			aircraft = self.TraitOrDefault<Aircraft>();

			base.Created(self);

			// Spawn initial load.
			int burst = Info.InitialActorCount == -1 ? Info.Actors.Length : Info.InitialActorCount;
			for (int i = 0; i < burst; i++)
				Replenish(self, SlaveEntries);

			// The base class creates the slaves but doesn't move them into world.
			// Let's do it here.
			SpawnReplenishedSlaves(self);

			hasSpawnedInitialLoad = true;
		}

		public override BaseSpawnerSlaveEntry[] CreateSlaveEntries(BaseSpawnerMasterInfo info)
		{
			slaveEntries = new DroneSpawnerSlaveEntry[info.Actors.Length]; // For this class to use

			for (int i = 0; i < slaveEntries.Length; i++)
				slaveEntries[i] = new DroneSpawnerSlaveEntry();

			return slaveEntries; // For the base class to use
		}

		public override void InitializeSlaveEntry(Actor slave, BaseSpawnerSlaveEntry entry)
		{
			var se = entry as DroneSpawnerSlaveEntry;
			base.InitializeSlaveEntry(slave, se);

			se.SpawnerSlave = slave.Trait<DroneSpawnerSlave>();
			se.Health = slave.Trait<Health>();
		}

		public void ResolveOrder(Actor self, Order order)
		{
			if (Info.SlavesHaveFreeWill)
				return;

			switch (order.OrderString)
			{
				case "Stop":
					StopSlaves();
					break;
				default:
					break;
			}
		}

		void INotifyAttack.PreparingAttack(Actor self, in Target target, Armament a, Barrel barrel) { }

		void INotifyAttack.Attacking(Actor self, in Target target, Armament a, Barrel barrel)
		{
			if (Info.SlavesHaveFreeWill || target.Type == TargetType.Invalid)
				return;

			AssignTargetsToSlaves(self, target);
		}

		void ITick.Tick(Actor self)
		{
			if (spawnReplaceTicks > 0)
			{
				spawnReplaceTicks--;

				// Time to respawn someting.
				if (spawnReplaceTicks <= 0)
				{
					Replenish(self, slaveEntries);

					SpawnReplenishedSlaves(self);

					// If there's something left to spawn, restart the timer.
					if (SelectEntryToSpawn(slaveEntries) != null)
						spawnReplaceTicks = Util.ApplyPercentageModifiers(Info.RespawnTicks, reloadModifiers.Select(rm => rm.GetReloadModifier()));
				}
			}

			if (!Info.SlavesHaveFreeWill)
				AssignSlaveActivity(self);
		}

		void SpawnReplenishedSlaves(Actor self)
		{
			WPos centerPosition = WPos.Zero;
			if (!hasSpawnedInitialLoad || !Info.ExitByBudding)
			{
				// Spawning from a solid actor...
				centerPosition = self.CenterPosition;
			}
			else
			{
				// Spawning from a virtual nexus: exit by an existing member.
				var se = slaveEntries.FirstOrDefault(s => s.IsValid && s.Actor.IsInWorld);
				if (se != null)
					centerPosition = se.Actor.CenterPosition;
			}

			// WPos.Zero implies this mob spawner master is dead or something.
			if (centerPosition == WPos.Zero)
				return;

			foreach (var se in slaveEntries)
				if (se.IsValid && !se.Actor.IsInWorld)
					SpawnIntoWorld(self, se.Actor, centerPosition + se.Offset.Rotate(self.Orientation));
		}

		public override void OnSlaveKilled(Actor self, Actor slave)
		{
			// No need to update mobs entry because Actor.IsDead marking is done automatically by the engine.
			// However, we need to check if all are dead when AggregateHealth.
			if (Info.AggregateHealth && slaveEntries.All(m => !m.IsValid))
				self.Dispose();

			if (spawnReplaceTicks <= 0)
				spawnReplaceTicks = Info.RespawnTicks;
		}

		void AssignTargetsToSlaves(Actor self, Target target)
		{
			foreach (var se in slaveEntries)
			{
				if (!se.IsValid)
					continue;
				if (se.SpawnerSlave.info.AttackCallBackDistance.LengthSquared > (self.CenterPosition - target.CenterPosition).HorizontalLengthSquared)
					se.SpawnerSlave.Attack(se.Actor, target);
				else
				{
					if (!se.SpawnerSlave.IsMoving())
					{
						se.SpawnerSlave.Stop(se.Actor);
						se.SpawnerSlave.Move(se.Actor, self.Location);
					}
				}
			}
		}

		void MoveSlaves(Actor self)
		{
			foreach (var se in slaveEntries)
			{
				if (!se.IsValid || !se.Actor.IsInWorld)
					continue;

				if (!se.SpawnerSlave.IsMoving())
				{
					se.SpawnerSlave.Stop(se.Actor);
					se.SpawnerSlave.Move(se.Actor, self.Location);
				}
			}
		}

		void AssignSlaveActivity(Actor self)
		{
			var effectiveActivity = self.CurrentActivity;
			if (!self.IsIdle)
			{
				while (effectiveActivity.ChildActivity != null)
					effectiveActivity = effectiveActivity.ChildActivity;
			}

			if (effectiveActivity == null || effectiveActivity.ActivityType == abilitytype || effectiveActivity.ActivityType == othertype)
			{
				if (preState == movetype)
					MoveSlaves(self);
				else if (preState == attacktype)
					MoveSlaves(self);
			}
			else if (effectiveActivity.ActivityType == movetype)
			{
				if (preState == movetype)
					MoveSlaves(self);
				else if (preState == attacktype)
					StopSlaves();
			}

			// Actually, new code here or old code in MobSpawnerMaster is not working
			// The only working code is in INotifyAttack. It is due to Activity of attack
			// do not achieve `GetTargets(actor)`
			else if (effectiveActivity.ActivityType == attacktype)
			{
				if (preState == movetype)
					StopSlaves();
				else if (preState == othertype || preState == abilitytype)
					StopSlaves();
			}

			preState = effectiveActivity == null ? othertype : effectiveActivity.ActivityType;
		}
	}
}
