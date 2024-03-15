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

using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits.Render
{
	[Desc("Grants condition after weapon fire and show the duration as the weapon is charging.")]
	sealed class ArmamentsChargeBarInfo : ConditionalTraitInfo
	{
		[Desc("Armament name")]
		public readonly string ArmamentName = "primary";

		[GrantedConditionReference]
		[Desc("The condition to grant after fire.")]
		public readonly string Condition = null;

		[Desc("Armament name")]
		public readonly int Duration = 100;

		public readonly Color Color = Color.Magenta;

		public override object Create(ActorInitializer init) { return new ArmamentsChargeBar(init.Self, this); }
	}

	sealed class ArmamentsChargeBar : ConditionalTrait<ArmamentsChargeBarInfo>, ISelectionBar, INotifyAttack, ITick
	{
		readonly Actor self;
		int token;

		[Sync]
		public int Countdown { get; private set; }

		public ArmamentsChargeBar(Actor self, ArmamentsChargeBarInfo info)
			: base(info)
		{
			this.self = self;
			token = Actor.InvalidConditionToken;
			Countdown = info.Duration;
		}

		void INotifyAttack.Attacking(Actor self, in Target target, Armament a, Barrel barrel)
		{
			if (IsTraitDisabled || a.Info.Name != Info.ArmamentName)
				return;

			if (token == Actor.InvalidConditionToken)
				token = self.GrantCondition(Info.Condition);
			Countdown = 0;
		}

		void INotifyAttack.PreparingAttack(Actor self, in Target target, Armament a, Barrel barrel) { }

		void ITick.Tick(Actor self)
		{
			if (Countdown >= Info.Duration)
			{
				if (token != Actor.InvalidConditionToken)
					token = self.RevokeCondition(token);
			}
			else
				Countdown++;
		}

		float ISelectionBar.GetValue()
		{
			if (!self.Owner.IsAlliedWith(self.World.RenderPlayer) || IsTraitDisabled)
				return 0;

			return (float)Countdown / Info.Duration;
		}

		Color ISelectionBar.GetColor() { return Info.Color; }

		bool ISelectionBar.DisplayWhenEmpty => false;
	}
}
