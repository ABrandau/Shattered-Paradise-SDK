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
using OpenRA.Mods.Common.Traits;

using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	public struct FirePortSP
	{
		public Armament[] armaments;
		public IFacing paxFacing;
		public IPositionable paxPos;
		public int PortIndex;
	}

	[Desc("Cargo can fire their weapons out of fire ports.")]
	public class AttackGarrisonedSPInfo : AttackFollowInfo, IRulesetLoaded, Requires<CargoInfo>
	{
		[FieldLoader.Require]
		[Desc("Fire port offsets in local coordinates.")]
		public readonly WVec[] PortOffsets = null;

		public override object Create(ActorInitializer init) { return new AttackGarrisonedSP(init.Self, this); }
		public override void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			if (PortOffsets.Length == 0)
				throw new YamlException("PortOffsets must have at least one entry.");

			base.RulesetLoaded(rules, ai);
		}
	}

	public class AttackGarrisonedSP : AttackFollow, INotifyPassengerEntered, INotifyPassengerExited
	{
		public new readonly AttackGarrisonedSPInfo Info;
		readonly Lazy<BodyOrientation> coords;
		INotifyAttack[] notifyAttacks;

		Dictionary<Actor, FirePortSP> ports = new();

		public AttackGarrisonedSP(Actor self, AttackGarrisonedSPInfo info)
			: base(self, info)
		{
			Info = info;
			coords = Exts.Lazy(() => self.Trait<BodyOrientation>());
		}

		protected override void Created(Actor self)
		{
			notifyAttacks = self.TraitsImplementing<INotifyAttack>().ToArray();
			base.Created(self);
		}

		protected override Func<IEnumerable<Armament>> InitializeGetArmaments(Actor self)
		{
			return () =>
			{
				var passengers = ports.Keys;

				List<Armament> arms = new();
				foreach (var pass in passengers)
				{
					var port = ports[pass];

					foreach (var arm in port.armaments)
						arms.Add(arm);
				}

				return arms;
			};
		}

		int CurrentPortIndex;
		void INotifyPassengerEntered.OnPassengerEntered(Actor self, Actor passenger)
		{
			CurrentPortIndex = (CurrentPortIndex + 1) % Info.PortOffsets.Length;
			var port = new FirePortSP
			{
				armaments = passenger.TraitsImplementing<Armament>().Where(a => Info.Armaments.Contains(a.Info.Name)).ToArray(),
				paxFacing = passenger.Trait<IFacing>(),
				paxPos = passenger.Trait<IPositionable>(),
				PortIndex = CurrentPortIndex,
			};

			ports[passenger] = port;
		}

		void INotifyPassengerExited.OnPassengerExited(Actor self, Actor passenger)
		{
			ports.Remove(passenger);
		}

		WVec PortOffset(Actor self, FirePortSP p)
		{
			var bodyOrientation = coords.Value.QuantizeOrientation(self.Orientation);
			return coords.Value.LocalToWorld(Info.PortOffsets[p.PortIndex].Rotate(bodyOrientation));
		}

		public override void DoAttack(Actor self, in Target target)
		{
			if (!CanAttack(self, target))
				return;

			var pos = self.CenterPosition;
			var targetedPosition = GetTargetPosition(pos, target);
			var targetYaw = (targetedPosition - pos).Yaw;

			var passengers = ports.Keys;
			foreach (var pass in passengers)
			{
				var port = ports[pass];

				foreach (var arm in port.armaments)
				{
					if (arm.IsTraitDisabled)
						continue;

					port.paxFacing.Facing = targetYaw;
					port.paxPos.SetCenterPosition(pass, pos + PortOffset(self, port));

					var barrel = arm.CheckFire(pass, facing, target);
					if (barrel == null)
						continue;

					foreach (var npa in notifyAttacks)
						npa.Attacking(self, target, arm, barrel);
				}
			}
		}

		protected override void Tick(Actor self)
		{
			base.Tick(self);
		}
	}
}
