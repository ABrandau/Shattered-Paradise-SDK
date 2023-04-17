using System;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Sp.Traits
{
	public class GrantConditionWhenDockInfo : TraitInfo
	{
		[GrantedConditionReference]
		[Desc("The condition to grant to target, the condition must be included in the target actor's ExternalConditions list.")]
		public readonly string ConditionToClient = null;

		[Desc("How long condition is applid even after undock. Use -1 for infinite.")]
		public readonly int ConditionToClientDuration = 0;

		[GrantedConditionReference]
		[Desc("The condition to grant to self")]
		public readonly string ConditionToSelf = null;

		[Desc("How long condition is applid even after undock. Use -1 for infinite.")]
		public readonly int ConditionToSelfDuration = 0;

		[Desc("Events leading to the condition being granted. Possible values currently are: Rearm, Repair.")]
		public readonly ResupplyType ConditionOn = ResupplyType.Rearm | ResupplyType.Repair;

		public override object Create(ActorInitializer init) { return new GrantConditionWhenDock(this); }
	}

	public class GrantConditionWhenDock : INotifyDockHost, ITick, ISync
	{
		GrantConditionWhenDockInfo info;
		ExternalCondition targetExternalCondition;
		int token;
		int delayedtoken;
		int clientToken;

		[Sync]
		public int Duration { get; private set; }

		public GrantConditionWhenDock(GrantConditionWhenDockInfo info)
		{
			this.info = info;
			token = Actor.InvalidConditionToken;
			delayedtoken = Actor.InvalidConditionToken;
			clientToken = Actor.InvalidConditionToken;
		}

		void INotifyDockHost.Docked(Actor self, Actor client)
		{
			if (info.ConditionToSelf != null)
			{
				if (token == Actor.InvalidConditionToken)
					token = self.GrantCondition(info.ConditionToSelf);
			}

			if (info.ConditionToClient != null)
			{
				targetExternalCondition = client.TraitsImplementing<ExternalCondition>()
						.FirstOrDefault(t => t.Info.Condition == info.ConditionToClient && t.CanGrantCondition(self));

				clientToken = targetExternalCondition != null ? targetExternalCondition.GrantCondition(client, self) : Actor.InvalidConditionToken;
			}
		}

		void INotifyDockHost.Undocked(Actor self, Actor client)
		{
			if (token != Actor.InvalidConditionToken)
			{
				if (info.ConditionToSelfDuration >= 0)
				{
					if (info.ConditionToSelfDuration == 0)
						token = self.RevokeCondition(token);
					else
					{
						delayedtoken = token;
						token = Actor.InvalidConditionToken;
						Duration = info.ConditionToSelfDuration;
					}
				}
			}

			if (clientToken != Actor.InvalidConditionToken)
			{
				if (info.ConditionToClientDuration >= 0)
				{
					targetExternalCondition.TryRevokeCondition(client, self, clientToken);
					if (info.ConditionToClientDuration > 0)
						targetExternalCondition.GrantCondition(client, self, info.ConditionToClientDuration);
				}
				else
					clientToken = Actor.InvalidConditionToken;
			}
		}

		void ITick.Tick(Actor self)
		{
			if (delayedtoken != Actor.InvalidConditionToken && --Duration <= 0)
				delayedtoken = self.RevokeCondition(delayedtoken);
		}
	}
}
