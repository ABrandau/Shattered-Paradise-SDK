using System.Collections.Generic;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Sp.Traits
{
	public sealed class GrantConditionWhenDockClientInfo : TraitInfo
	{
		[FieldLoader.Require]
		[GrantedConditionReference]
		[Desc("The condition to grant to self")]
		public readonly string Condition = null;

		[Desc("How long condition is applid even after undock. Use -1 for infinite.")]
		public readonly int AfterDockDuration = 0;

		[Desc("Host actor leading to the condition being granted. Enable for all at default.")]
		public readonly HashSet<string> DockHostNames = null;

		public override object Create(ActorInitializer init) { return new GrantConditionWhenDockClient(this); }
	}

	public sealed class GrantConditionWhenDockClient : INotifyDockClient, ITick, ISync
	{
		readonly GrantConditionWhenDockClientInfo info;
		int token;
		int delayedtoken;

		[Sync]
		public int Duration { get; private set; }

		public GrantConditionWhenDockClient(GrantConditionWhenDockClientInfo info)
		{
			this.info = info;
			token = Actor.InvalidConditionToken;
			delayedtoken = Actor.InvalidConditionToken;
		}

		void INotifyDockClient.Docked(Actor self, Actor host)
		{
			if (info.Condition != null && (info.DockHostNames == null || info.DockHostNames.Contains(host.Info.Name)))
			{
				if (token == Actor.InvalidConditionToken)
					token = self.GrantCondition(info.Condition);
			}
		}

		void INotifyDockClient.Undocked(Actor self, Actor host)
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
