using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Sp.Traits
{
	public class GrantConditionWhenDockHostInfo : TraitInfo
	{
		[FieldLoader.Require]
		[GrantedConditionReference]
		[Desc("The condition to grant to self")]
		public readonly string Condition = null;

		[Desc("How long condition is applid even after undock. Use -1 for infinite.")]
		public readonly int AfterDockDuration = 0;

		[Desc("Client actor leading to the condition being granted. Enable for all at default.")]
		public readonly HashSet<string> DockClientNames = null;

		public override object Create(ActorInitializer init) { return new GrantConditionWhenDockHost(this); }
	}

	public class GrantConditionWhenDockHost : INotifyDockHost, ITick, ISync
	{
		GrantConditionWhenDockHostInfo info;
		int token;
		int delayedtoken;

		[Sync]
		public int Duration { get; private set; }

		public GrantConditionWhenDockHost(GrantConditionWhenDockHostInfo info)
		{
			this.info = info;
			token = Actor.InvalidConditionToken;
			delayedtoken = Actor.InvalidConditionToken;
		}

		void INotifyDockHost.Docked(Actor self, Actor client)
		{
			if (info.Condition != null && info.DockClientNames == null ? true : info.DockClientNames.Contains(client.Info.Name))
			{
				if (token == Actor.InvalidConditionToken)
					token = self.GrantCondition(info.Condition);
			}
		}

		void INotifyDockHost.Undocked(Actor self, Actor client)
		{
			if (token != Actor.InvalidConditionToken)
			{
				if (info.AfterDockDuration >= 0)
				{
					if (info.AfterDockDuration == 0)
						token = self.RevokeCondition(token);
					else
					{
						delayedtoken = token;
						token = Actor.InvalidConditionToken;
						Duration = info.AfterDockDuration;
					}
				}
			}
		}

		void ITick.Tick(Actor self)
		{
			if (delayedtoken != Actor.InvalidConditionToken && --Duration <= 0)
				delayedtoken = self.RevokeCondition(delayedtoken);
		}
	}
}
