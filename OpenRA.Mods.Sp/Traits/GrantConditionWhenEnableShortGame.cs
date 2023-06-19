using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Sp.Traits
{
	sealed class GrantConditionWhenEnableShortGameInfo : TraitInfo
	{
		[FieldLoader.Require]
		[GrantedConditionReference]
		[Desc("The condition to grant to self")]
		public readonly string Condition = null;

		public override object Create(ActorInitializer init) { return new GrantConditionWhenEnableShortGame(this); }
	}

	sealed class GrantConditionWhenEnableShortGame: INotifyCreated
	{
		GrantConditionWhenEnableShortGameInfo info;
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
