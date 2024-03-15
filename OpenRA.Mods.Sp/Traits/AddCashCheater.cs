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

using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.SP.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Sp.Traits
{
	public enum CashCheatType { None, Refinery, Harvester, BaseBuilder }

	[Desc("Lets the actor make CashCheater generate more cash.")]
	public sealed class AddCashCheaterInfo : TraitInfo
	{
		[Desc("Type of cash cheat for CashCheater.")]
		public readonly CashCheatType CashCheatType = CashCheatType.None;

		public override object Create(ActorInitializer init) { return new AddCashCheater(this); }
	}

	public sealed class AddCashCheater : INotifyOwnerChanged, INotifyKilled, INotifySold, INotifyTransform, INotifyCreated, INotifyActorDisposing
	{
		readonly CashCheatType type;
		IEnumerable<CashCheater> cashCheaters;
		bool removedFromWorld;

		public AddCashCheater(AddCashCheaterInfo info)
		{
			type = info.CashCheatType;
		}

		void INotifyCreated.Created(Actor self)
		{
			cashCheaters = self.Owner.PlayerActor.TraitsImplementing<CashCheater>().ToArray();
			AddCashCheat();
		}

		void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
		{
			if (!removedFromWorld)
			{
				RemoveCashCheat();
				cashCheaters = newOwner.PlayerActor.TraitsImplementing<CashCheater>().ToArray();
				AddCashCheat();
			}
		}

		void INotifySold.Selling(Actor self) { }

		void INotifySold.Sold(Actor self)
		{
			if (!removedFromWorld)
			{
				removedFromWorld = true;
				RemoveCashCheat();
			}
		}

		void INotifyTransform.BeforeTransform(Actor self) { }

		void INotifyTransform.OnTransform(Actor self) { }

		void INotifyTransform.AfterTransform(Actor toActor)
		{
			if (!removedFromWorld)
			{
				removedFromWorld = true;
				RemoveCashCheat();
			}
		}

		void INotifyKilled.Killed(Actor self, AttackInfo e)
		{
			if (!removedFromWorld)
			{
				removedFromWorld = true;
				RemoveCashCheat();
			}
		}

		void AddCashCheat()
		{
			switch (type)
			{
				case CashCheatType.BaseBuilder:
					foreach (var c in cashCheaters)
						c.BaseBuilderNum++;
					break;
				case CashCheatType.Refinery:
					foreach (var c in cashCheaters)
						c.RefineryNum++;
					break;
				case CashCheatType.Harvester:
					foreach (var c in cashCheaters)
						c.HarvestersNum++;
					break;
			}
		}

		void INotifyActorDisposing.Disposing(Actor self)
		{
			if (!removedFromWorld)
			{
				removedFromWorld = true;
				RemoveCashCheat();
			}
		}

		void RemoveCashCheat()
		{
			switch (type)
			{
				case CashCheatType.BaseBuilder:
					foreach (var c in cashCheaters)
						c.BaseBuilderNum--;
					break;
				case CashCheatType.Refinery:
					foreach (var c in cashCheaters)
						c.RefineryNum--;
					break;
				case CashCheatType.Harvester:
					foreach (var c in cashCheaters)
						c.HarvestersNum--;
					break;
			}
		}
	}
}
