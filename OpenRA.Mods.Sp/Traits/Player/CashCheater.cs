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
	public sealed class CashCheaterInfo : PausableConditionalTraitInfo, IRulesetLoaded
	{
		public readonly int CashAmountForEachHarvester = 0;
		public readonly int MaxCashAmountForHarvester = 0;

		public readonly int CashAmountForEachRefinery = 50;
		public readonly int MaxCashAmountForRefinery = 0;

		public readonly int CashAmountForEachBaseBuilder = 0;
		public readonly int MaxCashAmountForBaseBuilder = 0;

		[Desc("When game start, player (AI) get this amount of cash.")]
		public readonly int StartCashCheat = 3000;

		[Desc("Use resource storage for cash granted.")]
		public readonly int CashCheatInterval = 500;

		[Desc("Stop cash cheat when player (AI) has more than this amount of cash.")]
		public readonly int CashCheatCap = 15000;

		[Desc("Use resource storage for cash granted.")]
		public readonly bool UseResourceStorage = false;

		[Desc("Show cash granted.")]
		public readonly bool ShowCashCheat = false;

		public override object Create(ActorInitializer init) { return new CashCheater(this); }
	}

	public sealed class CashCheater : PausableConditionalTrait<CashCheaterInfo>, ITick, ISync, INotifyCreated
	{
		readonly CashCheaterInfo info;
		PlayerResources resources;
		int startcash;

		[Sync]
		public int CashCheatTicks { get; private set; }

		public int HarvestersNum { get; set; }

		public int RefineryNum { get; set; }

		public int BaseBuilderNum { get; set; }

		public CashCheater(CashCheaterInfo info)
			: base(info)
		{
			this.info = info;
			startcash = info.StartCashCheat;
		}

		protected override void Created(Actor self)
		{
			resources = self.Owner.PlayerActor.Trait<PlayerResources>();
			base.Created(self);
		}

		void ITick.Tick(Actor self)
		{
			if (IsTraitPaused || IsTraitDisabled || self.Owner.WinState == WinState.Lost || resources.Cash > info.CashCheatCap)
				return;

			if (--CashCheatTicks < 0)
			{
				CashCheatTicks = info.CashCheatInterval;
				var harvcash = Math.Min(Info.MaxCashAmountForHarvester, info.CashAmountForEachHarvester * HarvestersNum);
				var refcash = Math.Min(Info.MaxCashAmountForRefinery, info.CashAmountForEachRefinery * RefineryNum);
				var basebuildercash = Math.Min(Info.MaxCashAmountForBaseBuilder, info.CashAmountForEachBaseBuilder * BaseBuilderNum);
				ModifyCash(harvcash + refcash + basebuildercash + startcash);
				startcash = 0;
				if (info.ShowCashCheat)
					TextNotificationsManager.AddSystemLine($"{self.Owner}:CashAmountForBaseBuilder:{basebuildercash}.CashAmountForRefinery:{refcash}.CashAmountForHarvester:{harvcash}");
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
