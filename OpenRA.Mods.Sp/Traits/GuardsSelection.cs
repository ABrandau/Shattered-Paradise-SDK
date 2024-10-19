#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Combined Arms Developers (see Authors.txt).
 * This file is part of OpenRA Combined Arms & Shattered Paradise, which is free software.
 * It is made available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License,
 * or (at your option) any later version. For more information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	[Desc("Attach to support unit so that when ordered as part of a group with combat units it will guard those units when Attack and AttackMove.")]
	class GuardsSelectionInfo : ConditionalTraitInfo
	{
		[CursorReference]
		[Desc("Cursor to display when hovering over a valid target.")]
		public readonly string Cursor = "attackguard";

		public readonly PlayerRelationship TargetRelationships = PlayerRelationship.Enemy;
		public readonly PlayerRelationship ForceTargetRelationships = PlayerRelationship.Enemy | PlayerRelationship.Neutral | PlayerRelationship.Ally;

		[Desc("Will only guard units with these target types.")]
		public readonly BitSet<TargetableType> ValidTargetsToGuard = new("Ground");

		[Desc("Will not guard units with these target types.")]
		public readonly BitSet<TargetableType> InvalidTargetsToGuard = default;

		[Desc("Maximum number of guard orders to chain together.")]
		public readonly int MaxGuardingTargets = 8;

		[Desc("Orders to override to guard ally unit in selection. Use AttackGuards if you need override Attack/ForceAttack order.")]
		public readonly HashSet<string> OverrideOrders = new() { "AttackMove", "AssaultMove", "AttackGuards" };

		[Desc("Guard ally closest to target when distance between smaller than this value, otherwise choose ally closest to this actor.")]
		public readonly int ChooseClosestAllyRangeCells = 7;

		[Desc("When there are units with " + nameof(GuardsSelection) + " in player's selection, the one with higher level will guards the one with lower level.")]
		public readonly int GuardsSelectionLevel = 1;

		public override object Create(ActorInitializer init) { return new GuardsSelection(this); }
	}

	class GuardsSelection : ConditionalTrait<GuardsSelectionInfo>, IResolveOrder, INotifyCreated, IIssueOrder
	{
		AttackBase[] attackBases;

		public GuardsSelection(GuardsSelectionInfo info)
			: base(info) { }

		protected override void Created(Actor self)
		{
			attackBases = self.TraitsImplementing<AttackBase>().ToArray();
			base.Created(self);
		}

		IEnumerable<IOrderTargeter> IIssueOrder.Orders
		{
			get
			{
				if (IsTraitDisabled || !Info.OverrideOrders.Contains("AttackGuards"))
					yield break;

				yield return new AttackGuardOrderTargeter(this, 6);
			}
		}

		Order IIssueOrder.IssueOrder(Actor self, IOrderTargeter order, in Target target, bool queued)
		{
			if (order is AttackGuardOrderTargeter)
				return new Order(order.OrderID, self, target, queued);

			return null;
		}

		void IResolveOrder.ResolveOrder(Actor self, Order order)
		{
			if (IsTraitDisabled || order.Target.Type == TargetType.Invalid || order.Queued || self.Owner.IsBot || !Info.OverrideOrders.Contains(order.OrderString) || self.World.IsLoadingGameSave || self.World.IsReplay)
				return;

			var world = self.World;

			if (order.Target.Type == TargetType.Actor && (order.Target.Actor.Disposed || order.Target.Actor.Owner == self.Owner || !order.Target.Actor.IsInWorld || order.Target.Actor.IsDead))
				return;

			var guardableActors = world.Selection.Actors
				.Where(a => a.Owner == self.Owner
					&& !a.Disposed
					&& !a.IsDead
					&& a.IsInWorld
					&& a != self
					&& IsValidGuardableTarget(a))
				.OrderBy(a => (a.Location - self.Location).LengthSquared)
				.ToArray();

			if (guardableActors.Length == 0)
				return;

			// We find candidates that within "ChooseClosestAllyRangeCells" to guard at highest priority.
			var minDest = long.MaxValue;
			var candidate = 0;
			for (var i = 0; i < guardableActors.Length; i++)
			{
				if ((guardableActors[i].Location - self.Location).LengthSquared <= Info.ChooseClosestAllyRangeCells * Info.ChooseClosestAllyRangeCells)
				{
					var dist = (guardableActors[i].CenterPosition - order.Target.CenterPosition).HorizontalLengthSquared;
					if (dist < minDest)
					{
						minDest = dist;
						(guardableActors[candidate], guardableActors[i]) = (guardableActors[i], guardableActors[candidate]);
						candidate++;
					}
				}
			}

			var mainGuardActor = guardableActors[--candidate > 0 ? candidate : candidate = 0];
			if (mainGuardActor == null)
				return;

			world.IssueOrder(new Order("Guard", self, Target.FromActor(mainGuardActor), false, null, null));

			for (var i = 0; i < candidate && i < Info.MaxGuardingTargets; i++)
				world.IssueOrder(new Order("Guard", self, Target.FromActor(guardableActors[candidate - i - 1]), true, null, null));

			for (var i = candidate + 1; i < guardableActors.Length && i < Info.MaxGuardingTargets; i++)
				world.IssueOrder(new Order("Guard", self, Target.FromActor(guardableActors[i]), true, null, null));
		}

		bool IsValidGuardableTarget(Actor targetActor)
		{
			var targets = targetActor.GetEnabledTargetTypes();
			if (!Info.ValidTargetsToGuard.Overlaps(targets) || Info.InvalidTargetsToGuard.Overlaps(targets))
				return false;

			if (!targetActor.Info.HasTraitInfo<GuardableInfo>())
				return false;

			var guardsSelection = targetActor.TraitsImplementing<GuardsSelection>();
			if (guardsSelection.Any(t => !t.IsTraitDisabled && Info.GuardsSelectionLevel <= t.Info.GuardsSelectionLevel))
				return false;

			return true;
		}

		public bool CanAttackGuard(Actor self, Target t, bool forceAttack)
		{
			// If force-fire is not used, and the target requires force-firing or the target is
			// terrain or invalid, we will just ignore it.
			if (t.Type == TargetType.Invalid || (!forceAttack && (t.Type == TargetType.Terrain || t.RequiresForceFire)))
				return false;

			// Get target's owner; in case of terrain or invalid target there will be no problems
			// with owner == null since forceFire will have to be true in this part of the method
			// (short-circuiting in the logical expression below)
			Player owner = null;
			if (t.Type == TargetType.FrozenActor)
				owner = t.FrozenActor.Owner;
			else if (t.Type == TargetType.Actor)
				owner = t.Actor.Owner;

			return (owner == null || (forceAttack ? Info.ForceTargetRelationships : Info.TargetRelationships).HasRelationship(self.Owner.RelationshipWith(owner)))
				&& !attackBases.Any(ab => !ab.IsTraitDisabled && !ab.IsTraitPaused && ab.Armaments.Any(a => !a.IsTraitDisabled && !a.IsTraitPaused && a.Weapon.IsValidAgainst(t, self.World, self)));
		}
	}

	sealed class AttackGuardOrderTargeter : IOrderTargeter
	{
		readonly GuardsSelection gs;

		public AttackGuardOrderTargeter(GuardsSelection gs, int priority)
		{
			this.gs = gs;
			OrderID = "AttackGuards";
			OrderPriority = priority;
		}

		public string OrderID { get; }
		public int OrderPriority { get; }
		public bool TargetOverridesSelection(Actor self, in Target target, List<Actor> actorsAt, CPos xy, TargetModifiers modifiers) { return true; }

		bool CanTargetActor(Actor self, in Target target, ref TargetModifiers modifiers, ref string cursor)
		{
			IsQueued = modifiers.HasModifier(TargetModifiers.ForceQueue);

			if (modifiers.HasModifier(TargetModifiers.ForceMove))
				return false;

			// Disguised actors are revealed by the attack cursor
			// HACK: works around limitations in the targeting code that force the
			// targeting and attacking logic (which should be logically separate)
			// to use the same code
			if (target.Type == TargetType.Actor && target.Actor.EffectiveOwner != null &&
					target.Actor.EffectiveOwner.Disguised && self.Owner.RelationshipWith(target.Actor.Owner) == PlayerRelationship.Enemy)
				modifiers |= TargetModifiers.ForceAttack;

			if (!gs.CanAttackGuard(self, target, modifiers.HasModifier(TargetModifiers.ForceAttack)))
				return false;

			cursor = gs.Info.Cursor;

			return true;
		}

		bool CanTargetLocation(Actor self, CPos location, TargetModifiers modifiers, ref string cursor)
		{
			if (!self.World.Map.Contains(location))
				return false;

			IsQueued = modifiers.HasModifier(TargetModifiers.ForceQueue);

			// Targeting the terrain is only possible with force-attack modifier
			if (modifiers.HasModifier(TargetModifiers.ForceMove))
				return false;

			var target = Target.FromCell(self.World, location);

			if (!gs.CanAttackGuard(self, target, modifiers.HasModifier(TargetModifiers.ForceAttack)))
				return false;

			cursor = gs.Info.Cursor;

			return true;
		}

		public bool CanTarget(Actor self, in Target target, ref TargetModifiers modifiers, ref string cursor)
		{
			switch (target.Type)
			{
				case TargetType.Actor:
				case TargetType.FrozenActor:
					return CanTargetActor(self, target, ref modifiers, ref cursor);
				case TargetType.Terrain:
					return CanTargetLocation(self, self.World.Map.CellContaining(target.CenterPosition), modifiers, ref cursor);
				default:
					return false;
			}
		}

		public bool IsQueued { get; private set; }
	}
}
