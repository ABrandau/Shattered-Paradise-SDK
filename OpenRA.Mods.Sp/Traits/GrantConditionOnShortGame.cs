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
	[Desc("Grant condition when short game is enabled.",
		"Used for short game is enable on no-base mod.")]
	sealed class GrantConditionOnShortGameInfo : TraitInfo
	{
		[FieldLoader.Require]
		[GrantedConditionReference]
		[Desc("The condition to grant to self")]
		public readonly string Condition = null;

		public override object Create(ActorInitializer init) { return new GrantConditionOnShortGame(this); }
	}

	sealed class GrantConditionOnShortGame : INotifyCreated
	{
		readonly GrantConditionOnShortGameInfo info;
		int token;

		public GrantConditionOnShortGame(GrantConditionOnShortGameInfo info)
		{
			this.info = info;
			token = Actor.InvalidConditionToken;
		}

		void INotifyCreated.Created(Actor self)
		{
			if (self.Owner.World.WorldActor.Trait<MapOptions>().ShortGame && token == Actor.InvalidConditionToken)
				token = self.GrantCondition(info.Condition);
		}
	}
}
