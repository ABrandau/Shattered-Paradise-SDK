#region Copyright & License Information
/*
 * Copyright (c) The OpenHV Developers (see https://github.com/OpenHV/OpenHV)
 * This file is part of OpenHV, which is free software. It is made
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
	[Desc("Make AI move their expansion unit, when there is no more room for placing a new Mcv Factory, or we have plenty of MCV for normal production.")]
	public class UnpackBaseBotModuleInfo : ConditionalTraitInfo
	{
		[Desc("Actor types that provides build radius.")]
		public readonly HashSet<string> DeployedExpandVehicleTypes = new();

		[Desc("Actor types that are able to produce MCVs.")]
		public readonly HashSet<string> McvFactoryTypes = new();

		[Desc("Delay (in ticks) between unpacking unit at first, if there is no actor in McvFactoryTypes.")]
		public readonly int InitialUndeployTick = 8000;

		[Desc("Delay (in ticks) between unpacking unit.")]
		public readonly int UndeployTick = 10000;

		public override object Create(ActorInitializer init) { return new UnpackBaseBotModule(init.Self, this); }
	}

	public class UnpackBaseBotModule : ConditionalTrait<UnpackBaseBotModuleInfo>, IBotTick, IGameSaveTraitData
	{
		int undeployInterval;
		readonly World world;
		readonly Player player;

		readonly Predicate<Actor> validUnit;

		public UnpackBaseBotModule(Actor self, UnpackBaseBotModuleInfo info)
			: base(info)
		{
			world = self.World;
			player = self.Owner;
			undeployInterval = info.InitialUndeployTick;
			validUnit = a => a != null && a.Owner == player && !a.IsDead && a.IsInWorld;
		}

		void IBotTick.BotTick(IBot bot)
		{
			if (--undeployInterval <= 0)
			{
				undeployInterval = Info.UndeployTick;
				if (!world.ActorsHavingTrait<Production>().Where(a => validUnit(a) && Info.McvFactoryTypes.Contains(a.Info.Name)).Any())
				{
					var expand = world.ActorsHavingTrait<Transforms>().Where(a => validUnit(a) && Info.DeployedExpandVehicleTypes.Contains(a.Info.Name)).RandomOrDefault(world.LocalRandom);
					if (expand != null)
						bot.QueueOrder(new Order("DeployTransform", expand, true));
				}
			}
		}

		List<MiniYamlNode> IGameSaveTraitData.IssueTraitData(Actor self)
		{
			if (IsTraitDisabled)
				return null;

			return new List<MiniYamlNode>()
			{
				new MiniYamlNode("UndeployInterval", FieldSaver.FormatValue(undeployInterval))
			};
		}

		void IGameSaveTraitData.ResolveTraitData(Actor self, MiniYaml data)
		{
			if (self.World.IsReplay)
				return;

			var nodes = data.ToDictionary();

			if (nodes.TryGetValue("UndeplyTick", out var initialBaseCenterNode))
				undeployInterval = FieldLoader.GetValue<int>("UndeployInterval", initialBaseCenterNode.Value);
		}
	}
}
