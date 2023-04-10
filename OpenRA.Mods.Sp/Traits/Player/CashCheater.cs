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
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	[TraitLocation(SystemActors.Player)]
	[Desc("Lets the player (AI) generate cash in a set of rules.")]
	public class CashCheaterInfo : PausableConditionalTraitInfo, IRulesetLoaded
	{
		public readonly int CashAmountForEachHarvester = 0;
		public readonly int CashDelayForHarvester = 500;
		public readonly int MaxCashAmountForHarvester = 0;

		public readonly int CashAmountForEachRefinery = 50;
		public readonly int CashDelayForRefinery = 500;
		public readonly int MaxCashAmountForRefinery = 0;

		public readonly int CashAmountForEachBaseBuilder = 0;
		public readonly int CashDelayForBaseBuilder = 500;
		public readonly int MaxCashAmountForBaseBuilder = 0;

		[Desc("Use resource storage for cash granted.")]
		public readonly bool UseResourceStorage = false;

		public override object Create(ActorInitializer init) { return new CashCheater(this); }
	}

	public class CashCheater : PausableConditionalTrait<CashCheaterInfo>, ITick, ISync, INotifyCreated
	{
		readonly CashCheaterInfo info;
		PlayerResources resources;

		[Sync]
		public int HarvesterTicks { get; private set; }

		public int RefineryTicks { get; private set; }

		public int BaseBuilderTicks { get; private set; }

		public int HarvestersNum { get; set; }

		public int RefineryNum { get; set; }

		public int BaseBuilderNum { get; set; }

		public CashCheater(CashCheaterInfo info)
			: base(info)
		{
			this.info = info;
		}

		protected override void Created(Actor self)
		{
			resources = self.Owner.PlayerActor.Trait<PlayerResources>();

			base.Created(self);
		}

		void ITick.Tick(Actor self)
		{
			if (IsTraitPaused || IsTraitDisabled || self.Owner.WinState == WinState.Lost)
				return;

			if (--HarvesterTicks < 0)
			{
				HarvesterTicks = info.CashDelayForHarvester;
				var cash = info.CashAmountForEachHarvester * HarvestersNum;
				ModifyCash(Math.Min(Info.MaxCashAmountForHarvester, cash));
			}

			if (--RefineryTicks < 0)
			{
				RefineryTicks = info.CashDelayForRefinery;
				var cash = info.CashAmountForEachRefinery * RefineryNum;
				ModifyCash(Math.Min(Info.MaxCashAmountForRefinery, cash));
			}

			if (--BaseBuilderTicks < 0)
			{
				BaseBuilderTicks = info.CashDelayForBaseBuilder;
				var cash = info.CashAmountForEachBaseBuilder * BaseBuilderNum;
				ModifyCash(Math.Min(Info.MaxCashAmountForBaseBuilder, cash));
			}
		}

		void ModifyCash(int amount)
		{
			if (info.UseResourceStorage)
				resources.GiveResources(amount);
			else
				resources.ChangeCash(amount);
		}
	}
}
