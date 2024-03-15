#region Copyright & License Information
/*
 * Copyright The OpenRA-SP Developers (see AUTHORS)
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
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	public struct FirePortSP
	{
		public Armament[] Armaments;
		public IFacing PaxFacing;
		public IPositionable PaxPos;
		public RenderSprites PaxRender;
		public int PortIndex;
	}

	[Desc("Cargo can fire their weapons out of fire ports. Each passenger a port.")]
	public sealed class AttackGarrisonedSPInfo : AttackFollowInfo, IRulesetLoaded, Requires<CargoInfo>
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

	public sealed class AttackGarrisonedSP : AttackFollow, INotifyPassengerEntered, INotifyPassengerExited, IRender
	{
		public new readonly AttackGarrisonedSPInfo Info;
		readonly Lazy<BodyOrientation> coords;
		readonly Dictionary<Actor, FirePortSP> ports = new();
		readonly Dictionary<AnimationWithOffset, string> muzzles = new();
		INotifyAttack[] notifyAttacksForUncloak;

		public AttackGarrisonedSP(Actor self, AttackGarrisonedSPInfo info)
			: base(self, info)
		{
			Info = info;
			coords = Exts.Lazy(() => self.Trait<BodyOrientation>());
		}

		protected override void Created(Actor self)
		{
			// HACK: we only reveal cloak at AttackGarrisonedSP, because we cannot really
			// apply other actors' Armament to all the INotifyAttack belongs to transport, which
			// will leads to crash or bug.
			notifyAttacksForUncloak = self.TraitsImplementing<INotifyAttack>().Where(t => t is Cloak).ToArray();
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
					arms.AddRange(port.Armaments);
				}

				return arms;
			};
		}

		int currentPortIndex;
		void INotifyPassengerEntered.OnPassengerEntered(Actor self, Actor passenger)
		{
			currentPortIndex = (currentPortIndex + 1) % Info.PortOffsets.Length;
			var port = new FirePortSP
			{
				Armaments = passenger.TraitsImplementing<Armament>().Where(a => Info.Armaments.Contains(a.Info.Name)).ToArray(),
				PaxFacing = passenger.Trait<IFacing>(),
				PaxPos = passenger.Trait<IPositionable>(),
				PaxRender = passenger.Trait<RenderSprites>(),
				PortIndex = currentPortIndex,
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
			var hasNotifiedAttack = false;

			foreach (var seat in ports.Keys)
			{
				var port = ports[seat];

				foreach (var arm in port.Armaments)
				{
					if (arm.IsTraitDisabled)
						continue;

					port.PaxFacing.Facing = targetYaw;
					port.PaxPos.SetCenterPosition(seat, pos + PortOffset(self, port));

					if (!arm.CheckFire(seat, facing, target))
						continue;

					if (arm.Info.MuzzleSequence != null)
					{
						// Muzzle facing is fixed once the firing starts
						var muzzleAnim = new Animation(self.World, port.PaxRender.GetImage(seat), () => targetYaw);
						var sequence = arm.Info.MuzzleSequence;
						var muzzleFlash = new AnimationWithOffset(muzzleAnim,
							() => PortOffset(self, port),
							() => false,
							p => RenderUtils.ZOffsetFromCenter(self, p, 1024));

						muzzles[muzzleFlash] = arm.Info.MuzzlePalette;
						muzzleAnim.PlayThen(sequence, () => muzzles.Remove(muzzleFlash));
					}

					if (!hasNotifiedAttack)
					{
						hasNotifiedAttack = true;
						foreach (var npa in notifyAttacksForUncloak)
							npa.Attacking(self, target, arm, null);
					}
				}
			}
		}

		IEnumerable<IRenderable> IRender.Render(Actor self, WorldRenderer wr)
		{
			// Display muzzle flashes
			foreach (var m in muzzles.Keys)
				foreach (var r in m.Render(self, wr.Palette(muzzles[m])))
					yield return r;
		}

		IEnumerable<Rectangle> IRender.ScreenBounds(Actor self, WorldRenderer wr)
		{
			// Muzzle flashes don't contribute to actor bounds
			yield break;
		}

		protected override void Tick(Actor self)
		{
			base.Tick(self);

			// Take a copy so that Tick() can remove animations
			foreach (var m in muzzles.Keys)
				m.Animation.Tick();
		}
	}
}
