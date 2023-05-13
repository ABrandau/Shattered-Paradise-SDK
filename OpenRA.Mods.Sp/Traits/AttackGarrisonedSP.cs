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
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	public struct FirePortSP
	{
		public Armament[] armaments;
		public IFacing paxFacing;
		public IPositionable paxPos;
		public RenderSprites paxRender;
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

	public class AttackGarrisonedSP : AttackFollow, INotifyPassengerEntered, INotifyPassengerExited, IRender
	{
		public new readonly AttackGarrisonedSPInfo Info;
		readonly Lazy<BodyOrientation> coords;
		readonly Dictionary<AnimationWithOffset, string> muzzles = new();
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
				paxRender = passenger.Trait<RenderSprites>(),
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

					if (arm.Info.MuzzleSequence != null)
					{
						// Muzzle facing is fixed once the firing starts
						var muzzleAnim = new Animation(self.World, port.paxRender.GetImage(pass), () => targetYaw);
						var sequence = arm.Info.MuzzleSequence;
						var muzzleFlash = new AnimationWithOffset(muzzleAnim,
							() => PortOffset(self, port),
							() => false,
							p => RenderUtils.ZOffsetFromCenter(self, p, 1024));

						muzzles[muzzleFlash] = arm.Info.MuzzlePalette;
						muzzleAnim.PlayThen(sequence, () => muzzles.Remove(muzzleFlash));
					}

					foreach (var npa in notifyAttacks)
						npa.Attacking(self, target, arm, barrel);
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
