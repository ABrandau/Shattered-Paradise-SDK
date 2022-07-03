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
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

/*
 * Needs base engine modification. (Becaus MobSpawner.cs mods it)
 */

namespace OpenRA.Mods.SP.Traits
{
	[Desc("Can be slaved to a drone spawner.")]
	public class DroneSpawnerSlaveInfo : BaseSpawnerSlaveInfo
	{
		[Desc("Aircraft slaves outside of this range from master while moving will be call back")]
		public readonly WDist MovingCallBackDistance = WDist.FromCells(3);

		[Desc("Slaves will follow master instead of attack while target outside of this range")]
		public readonly WDist AttackCallBackDistance = WDist.FromCells(10);

		public override object Create(ActorInitializer init) { return new DroneSpawnerSlave(this); }
	}

	public class DroneSpawnerSlave : BaseSpawnerSlave, INotifySelected
	{
		public IMove[] Moves { get; private set; }
		public IPositionable Positionable { get; private set; }
		public bool isAircraft;
		public readonly DroneSpawnerSlaveInfo info;
		Actor currentActor;
		Actor masterActor;
		public readonly Predicate<Actor> InvalidActor;

		DroneSpawnerMaster spawnerMaster;

		public bool IsMoving()
		{
			if (isAircraft)
			{
				if (!InvalidActor(currentActor) && !InvalidActor(masterActor) &&
					(currentActor.CenterPosition - masterActor.CenterPosition).HorizontalLengthSquared > info.MovingCallBackDistance.LengthSquared)
					return false;

				return true;
			}

			var groundmove = Moves.Any(m => m.IsTraitEnabled() && (m.CurrentMovementTypes.HasFlag(MovementType.Horizontal) || m.CurrentMovementTypes.HasFlag(MovementType.Vertical)));
			return groundmove;
		}

		public DroneSpawnerSlave(DroneSpawnerSlaveInfo info)
			: base(info)
		{
			InvalidActor = a => a == null || a.IsDead || !a.IsInWorld;
			this.info = info;
		}

		protected override void Created(Actor self)
		{
			base.Created(self);

			currentActor = self;

			Moves = self.TraitsImplementing<IMove>().ToArray();

			var positionables = self.TraitsImplementing<IPositionable>();
			if (positionables.Count() != 1)
				throw new InvalidOperationException("Actor {0} has multiple (or no) traits implementing IPositionable.".F(self));

			Positionable = positionables.First();

			isAircraft = self.Info.HasTraitInfo<AircraftInfo>();
		}

		public override void LinkMaster(Actor self, Actor master, BaseSpawnerMaster spawnerMaster)
		{
			base.LinkMaster(self, master, spawnerMaster);
			this.spawnerMaster = spawnerMaster as DroneSpawnerMaster;
			masterActor = master;
		}

		public void Move(Actor self, CPos location)
		{
			// And tell attack bases to stop attacking.
			if (Moves.Length == 0)
				return;

			foreach (var mv in Moves)
				if (mv.IsTraitEnabled())
				{
					self.QueueActivity(mv.MoveTo(location, 2));
					break;
				}
		}

		void INotifySelected.Selected(Actor self)
		{
			if (spawnerMaster.Info.SlavesHaveFreeWill)
				return;

			// I'm assuming these guys are selectable, both slave and the nexus.
			// self.World.Selection.Remove(self.World, self); No need to remove when you don't wee the selection decoration.
			// -SelectionDecorations: is all you need.
			// Also use RejectsOrder if necessary.
			self.World.Selection.Add(Master);
		}

	}
}
