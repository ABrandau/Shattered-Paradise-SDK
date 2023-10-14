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

using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Sp.Traits
{
	[Desc("Hack: used for short game is disabled and no-base mod.")]
	sealed class GrantConditionWhenEnableShortGameInfo : TraitInfo
	{
		[FieldLoader.Require]
		[GrantedConditionReference]
		[Desc("The condition to grant to self")]
		public readonly string Condition = null;

		public override object Create(ActorInitializer init) { return new GrantConditionWhenEnableShortGame(this); }
	}

	sealed class GrantConditionWhenEnableShortGame : INotifyCreated
	{
		readonly GrantConditionWhenEnableShortGameInfo info;
		int token;

		public GrantConditionWhenEnableShortGame(GrantConditionWhenEnableShortGameInfo info)
		{
			this.info = info;
			token = Actor.InvalidConditionToken;
		}

		public void Created(Actor self)
		{
			if (self.Owner.World.WorldActor.Trait<MapOptions>().ShortGame)
			{
				if (token == Actor.InvalidConditionToken)
					token = self.GrantCondition(info.Condition);
			}
		}
	}
}
