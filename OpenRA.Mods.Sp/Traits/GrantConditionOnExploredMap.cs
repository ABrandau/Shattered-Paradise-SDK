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
	[Desc("Grant condition when explored map is enabled.",
		"Used for mega-wealth well to show itself.")]
	sealed class GrantConditionOnExploredMapInfo : TraitInfo
	{
		[FieldLoader.Require]
		[GrantedConditionReference]
		[Desc("The condition to grant to self")]
		public readonly string Condition = null;

		public override object Create(ActorInitializer init) { return new GrantConditionOnExploredMap(this); }
	}

	class GrantConditionOnExploredMap : INotifyCreated
	{
		readonly GrantConditionOnExploredMapInfo info;
		int token;

		public GrantConditionOnExploredMap(GrantConditionOnExploredMapInfo info)
		{
			this.info = info;
			token = Actor.InvalidConditionToken;
		}

		void INotifyCreated.Created(Actor self)
		{
			if (self.Owner.PlayerActor.Trait<Shroud>().ExploreMapEnabled && token == Actor.InvalidConditionToken)
				token = self.GrantCondition(info.Condition);
		}
	}
}
