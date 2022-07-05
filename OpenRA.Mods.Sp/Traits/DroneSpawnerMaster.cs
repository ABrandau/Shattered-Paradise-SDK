#region Copyright & License Information
/*
 * Copyright 2015- OpenRA.Mods.AS Developers (see AUTHORS)
 * This file is a part of a third-party plugin for OpenRA, which is
 * free software. It is made available to you under the terms of the
 * GNU General Public License as published by the Free Software
 * Foundation. For more information, see COPYING.
 */
#endregion

using System;
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

		[Desc("Place slave will gather to. Only recommended to used on building master")] // TODO: Test it on ground unit on map edges
		public readonly CVec[] GatherCell = Array.Empty<CVec>();

		[Desc("When idle and not moving, master check slaves and gathers them in this many tick. Set it properly can save performance")]
		public readonly int IdleCheckTick = 103;

		public override void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			base.RulesetLoaded(rules, ai);

			if (Actors == null || Actors.Length == 0)
				throw new YamlException("Actors is null or empty for DroneSpawner for actor type {0}!".F(ai.Name));

			if (InitialActorCount > Actors.Length || InitialActorCount < -1)
				throw new YamlException("DroneSpawner can't have more InitialActorCount than the actors defined!");

			if (GatherCell.Length > Actors.Length)
				throw new YamlException("Length of GatherOffsetCell can't be larger than the actors defined! (Actor type = {0})".F(ai.Name));
		}

		public override object Create(ActorInitializer init) { return new DroneSpawnerMaster(init, this); }
	}

	public class DroneSpawnerMaster : BaseSpawnerMaster, INotifyOwnerChanged, ITick,
		IResolveOrder, INotifyAttack
	{
		class DroneSpawnerSlaveEntry : BaseSpawnerSlaveEntry
		{
			public new DroneSpawnerSlave SpawnerSlave;
			public CVec GatherOffsetCell = CVec.Zero;
		}

		public new DroneSpawnerMasterInfo Info { get; private set; }

		DroneSpawnerSlaveEntry[] slaveEntries;

		bool hasSpawnedInitialLoad = false;
		int spawnReplaceTicks = 0;

		readonly OpenRA.Activities.ActivityType attacktype = OpenRA.Activities.ActivityType.Attack;
		readonly OpenRA.Activities.ActivityType movetype = OpenRA.Activities.ActivityType.Move;
		readonly OpenRA.Activities.ActivityType abilitytype = OpenRA.Activities.ActivityType.Ability;
		readonly OpenRA.Activities.ActivityType othertype = OpenRA.Activities.ActivityType.Undefined;

		OpenRA.Activities.ActivityType preState;

		WPos preLoc;

		int remainingIdleCheckTick;

		public DroneSpawnerMaster(ActorInitializer init, DroneSpawnerMasterInfo info)
			: base(init, info)
		{
			Info = info;
			preLoc = WPos.Zero;
		}

		protected override void Created(Actor self)
		{
			base.Created(self);

			remainingIdleCheckTick = Info.IdleCheckTick;
			for (var i = 0; i < Info.GatherCell.Length; i++)
				slaveEntries[i].GatherOffsetCell = Info.GatherCell[i];

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

			for (var i = 0; i < slaveEntries.Length; i++)
				slaveEntries[i] = new DroneSpawnerSlaveEntry();

			return slaveEntries; // For the base class to use
		}

		public override void InitializeSlaveEntry(Actor slave, BaseSpawnerSlaveEntry entry)
		{
			var se = entry as DroneSpawnerSlaveEntry;
			base.InitializeSlaveEntry(slave, se);

			se.SpawnerSlave = slave.Trait<DroneSpawnerSlave>();
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
				else if (preLoc != self.CenterPosition)
				{
					MoveSlaves(self);
					remainingIdleCheckTick = Info.IdleCheckTick;
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
					se.SpawnerSlave.Move(se.Actor, self.Location + se.GatherOffsetCell);
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

			// 1. Drone may get away from master due to auto-targeting.
			if (effectiveActivity == null || effectiveActivity.ActivityType == abilitytype || effectiveActivity.ActivityType == othertype)
			{
				if (remainingIdleCheckTick < 0)
				{
					MoveSlaves(self);
					remainingIdleCheckTick = Info.IdleCheckTick;
				}

				// 1.1 There is situation like teleport will just change actor's position without activity
				else if (preLoc != self.CenterPosition)
				{
					MoveSlaves(self);
					remainingIdleCheckTick = Info.IdleCheckTick;
				}
				else
					remainingIdleCheckTick--;
			}

			// 2. Stop the drone attacking when move for special case of fire at an ally.
			// Only move slaves when position change
			else if (effectiveActivity.ActivityType == movetype)
			{
				if (preState == attacktype)
				{
					StopSlaves();
					remainingIdleCheckTick = Info.IdleCheckTick;
				}
				else if (preLoc != self.CenterPosition)
				{
					MoveSlaves(self);
					remainingIdleCheckTick = Info.IdleCheckTick;
				}
			}

			// Actually, new code here or old code in MobSpawnerMaster is not working
			// The only working code is in INotifyAttack. It is due to Activity of attack
			// do not achieve `GetTargets(actor)`
			// 3. Stop the slaves move when prepare to attack
			else if (effectiveActivity.ActivityType == attacktype)
			{
				if (preState == movetype)
				{
					StopSlaves();
					remainingIdleCheckTick = Info.IdleCheckTick;
				}
				else if (preState == othertype || preState == abilitytype)
				{
					StopSlaves();
					remainingIdleCheckTick = Info.IdleCheckTick;
				}
			}

			preState = effectiveActivity == null ? othertype : effectiveActivity.ActivityType;
			preLoc = self.CenterPosition;
		}

		/* Debug
		, ITickRender
		void ITickRender.TickRender(Graphics.WorldRenderer wr, Actor self)
		{
			var font = Game.Renderer.Fonts["Bold"];
			foreach (var kv in Info.GatherOffsetCell)
			{
				var i = new FloatingText(self.World.Map.CenterOfCell(kv + self.Location), Color.Gold, "1", 1);
				self.World.Add(i);
			}
		}
		*/
	}
}
