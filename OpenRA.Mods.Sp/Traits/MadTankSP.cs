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

using System.Collections.Generic;
using System.Linq;
using OpenRA.Activities;
using OpenRA.GameRules;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Orders;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	sealed class MadTankSPInfo : PausableConditionalTraitInfo, IRulesetLoaded, Requires<WithFacingSpriteBodyInfo>
	{
		[Desc("Measured in ticks.")]
		public readonly int PrepareDelay = 12;

		[SequenceReference]
		public readonly string ThumpSequence = null;

		public readonly int ThumpInitialInterval = 10;

		public readonly int IntervalReducedPerThump = 1;

		public readonly int LowestThumpInterval = 1;

		public readonly int ThumpHitDelay = 0;

		public readonly string ThumpArmamentName = null;

		public readonly string ThumpBeginSound = "";

		[WeaponReference]
		public readonly string ThumpWeapon = null;

		[Desc("Measured in ticks.")]
		public readonly int DetonationAtThumps = 10;

		public readonly string DetonationArmamentName = null;

		[WeaponReference]
		public readonly string DetonationWeapon = null;

		[VoiceReference]
		public readonly string Voice = "Action";

		[GrantedConditionReference]
		[Desc("The condition to grant to self while deployed.")]
		public readonly string DeployedCondition = null;

		public WeaponInfo ThumpDamageWeaponInfo { get; private set; }

		public WeaponInfo DetonationWeaponInfo { get; private set; }

		[Desc("Types of damage that this trait causes to self while self-destructing. Leave empty for no damage types.")]
		public readonly BitSet<DamageType> DamageTypes = default;

		[CursorReference]
		[Desc("Cursor to display when able to set up the detonation sequence.")]
		public readonly string DeployCursor = "deploy";

		public override object Create(ActorInitializer init) { return new MadTankSP(this); }

		public override void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			if (ThumpWeapon != null)
			{
				var thumpDamageWeaponToLower = ThumpWeapon.ToLowerInvariant();
				if (!rules.Weapons.TryGetValue(thumpDamageWeaponToLower, out var thumpDamageWeapon))
					throw new YamlException($"Weapons Ruleset does not contain an entry '{thumpDamageWeaponToLower}'");

				ThumpDamageWeaponInfo = thumpDamageWeapon;
			}

			if (DetonationWeapon != null)
			{
				var detonationWeaponToLower = DetonationWeapon.ToLowerInvariant();
				if (!rules.Weapons.TryGetValue(detonationWeaponToLower, out var detonationWeapon))
					throw new YamlException($"Weapons Ruleset does not contain an entry '{detonationWeapon}'");

				DetonationWeaponInfo = detonationWeapon;
			}
		}
	}

	sealed class MadTankSP : PausableConditionalTrait<MadTankSPInfo>, IIssueOrder, IResolveOrder, IOrderVoice, IIssueDeployOrder, INotifyAttack
	{
		readonly MadTankSPInfo info;
		public bool FirstDetonationComplete { get; set; }

		public MadTankSP(MadTankSPInfo info)
			: base(info)
		{
			this.info = info;
			FirstDetonationComplete = false;
		}

		public IEnumerable<IOrderTargeter> Orders
		{
			get
			{
				yield return new DeployOrderTargeter("Detonate", 5, () => Info.DeployCursor);
			}
		}

		Order IIssueOrder.IssueOrder(Actor self, IOrderTargeter order, in Target target, bool queued)
		{
			if (order.OrderID != "Detonate")
				return null;

			return new Order(order.OrderID, self, target, queued);
		}

		Order IIssueDeployOrder.IssueDeployOrder(Actor self, bool queued)
		{
			return new Order("Detonate", self, queued);
		}

		bool IIssueDeployOrder.CanIssueDeployOrder(Actor self, bool queued) { return self.CurrentActivity is not DetonationSequence; }

		string IOrderVoice.VoicePhraseForOrder(Actor self, Order order)
		{
			if (order.OrderString != "Detonate")
				return null;

			return info.Voice;
		}

		void IResolveOrder.ResolveOrder(Actor self, Order order)
		{
			if (order.OrderString == "Detonate")
				self.QueueActivity(order.Queued, new DetonationSequence(self, this));
		}

		void INotifyAttack.Attacking(Actor self, in Target target, Armament a, Barrel barrel)
		{
			self.QueueActivity(false, new DetonationSequence(self, this));
		}

		void INotifyAttack.PreparingAttack(Actor self, in Target target, Armament a, Barrel barrel)
		{
			return;
		}

		public class DetonationSequence : Activity
		{
			readonly MadTankSP mad;
			readonly IMove move;
			readonly WithFacingSpriteBody wfsb;
			readonly bool assignTargetOnFirstRun;

			int trumpcountdown;
			int impactDeplay = -1;
			int trumpedTime;
			bool initiated;
			Target target;

			public DetonationSequence(Actor self, MadTankSP mad)
				: this(self, mad, Target.Invalid)
			{
				assignTargetOnFirstRun = true;
			}

			public DetonationSequence(Actor self, MadTankSP mad, in Target target)
			{
				this.mad = mad;
				this.target = target;
				trumpcountdown = mad.info.PrepareDelay;

				move = self.Trait<IMove>();
				wfsb = self.Trait<WithFacingSpriteBody>();
			}

			protected override void OnFirstRun(Actor self)
			{
				if (assignTargetOnFirstRun)
					target = Target.FromCell(self.World, self.Location);
			}

			public override bool Tick(Actor self)
			{
				if (IsCanceling)
					return true;

				if (mad.IsTraitPaused)
					return false;

				if (target.Type != TargetType.Invalid && !move.CanEnterTargetNow(self, target))
				{
					QueueChild(new MoveAdjacentTo(self, target, targetLineColor: Color.Red));
					return false;
				}

				if (!initiated)
				{
					// If the target has died while we were moving, we should abort detonation.
					if (target.Type == TargetType.Invalid)
						return true;

					self.GrantCondition(mad.info.DeployedCondition);

					IsInterruptible = false;
					initiated = true;
				}

				if (impactDeplay >= 0)
				{
					if (impactDeplay == 0)
					{
						if (mad.info.ThumpWeapon != null)
						{
							if (mad.info.ThumpDamageWeaponInfo.Report != null && mad.info.ThumpDamageWeaponInfo.Report.Length > 0)
								Game.Sound.Play(SoundType.World, mad.info.ThumpDamageWeaponInfo.Report.Random(self.World.SharedRandom), self.CenterPosition, mad.info.ThumpDamageWeaponInfo.SoundVolume);

							// Use .FromPos since this weapon needs to affect more than just the MadTank actor
							mad.info.ThumpDamageWeaponInfo.Impact(Target.FromPos(self.CenterPosition), self);
						}

						trumpedTime++;
					}

					impactDeplay--;
				}
				else
				{
					if (trumpcountdown-- <= 0)
					{
						if (mad.info.ThumpSequence != null)
							wfsb.PlayCustomAnimation(self, mad.info.ThumpSequence);

						if (!string.IsNullOrEmpty(mad.info.ThumpBeginSound))
							Game.Sound.Play(SoundType.World, mad.info.ThumpBeginSound, self.CenterPosition);
						impactDeplay = mad.info.ThumpHitDelay;
						trumpcountdown = mad.info.ThumpInitialInterval - trumpedTime * mad.info.IntervalReducedPerThump;
						trumpcountdown = trumpcountdown < mad.info.LowestThumpInterval ? mad.info.LowestThumpInterval : trumpcountdown;
					}
				}

				return trumpedTime == mad.info.DetonationAtThumps - 1;
			}

			protected override void OnLastRun(Actor self)
			{
				if (!initiated)
					return;

				self.World.AddFrameEndTask(w =>
				{
					if (mad.info.DetonationWeaponInfo.Report != null && mad.info.DetonationWeaponInfo.Report.Length > 0)
						Game.Sound.Play(SoundType.World, mad.info.DetonationWeaponInfo.Report.Random(self.World.SharedRandom), self.CenterPosition, mad.info.DetonationWeaponInfo.SoundVolume);

					if (mad.info.DetonationWeapon != null)
					{
						var args = new WarheadArgs
						{
							Weapon = mad.info.DetonationWeaponInfo,
							SourceActor = self,
							WeaponTarget = target,
							DamageModifiers = self.TraitsImplementing<IFirepowerModifier>()
								.Select(a => a.GetFirepowerModifier(mad.info.ThumpArmamentName)).ToArray()
						};

						// Use .FromPos since this actor is killed. Cannot use Target.FromActor
						mad.info.DetonationWeaponInfo.Impact(Target.FromPos(self.CenterPosition), args);
					}

					self.Kill(self, mad.info.DamageTypes);
				});
			}

			public override IEnumerable<TargetLineNode> TargetLineNodes(Actor self)
			{
				yield return new TargetLineNode(target, Color.Crimson);
			}
		}
	}
}
