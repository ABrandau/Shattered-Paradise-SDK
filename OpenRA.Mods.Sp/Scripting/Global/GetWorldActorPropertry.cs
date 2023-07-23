using System.Linq;
using Eluant;
using OpenRA.Mods.Common.Traits;
using OpenRA.Scripting;

namespace OpenRA.Mods.SP.Scripting
{
	[ScriptGlobal("WorldActor")]
	public class WorldActorConditionGlobal : ScriptGlobal
	{
		public WorldActorConditionGlobal(ScriptContext context)
			: base(context)
		{
			externalConditions = context.World.WorldActor.TraitsImplementing<ExternalCondition>().ToArray();
		}

		readonly ExternalCondition[] externalConditions;

		[Desc("Grant an external condition on world actor and return the revocation token.",
			"Conditions must be defined on an ExternalConditions trait on the actor.",
			"If duration > 0 the condition will be automatically revoked after the defined number of ticks.")]
		public int GrantCondition(string condition, int duration = 0)
		{
			var external = externalConditions
				.FirstOrDefault(t => t.Info.Condition == condition && t.CanGrantCondition(this));

			if (external == null)
				throw new LuaException($"Condition `{condition}` has not been listed on an enabled ExternalCondition trait");

			return external.GrantCondition(Context.World.WorldActor, this, duration);
		}

		[Desc("Revoke a condition on world actor using the token returned by GrantCondition.")]
		public void RevokeCondition(int token)
		{
			foreach (var external in externalConditions)
				if (external.TryRevokeCondition(Context.World.WorldActor, this, token))
					break;
		}

		[Desc("Check whether world actor accepts a specific external condition.")]
		public bool AcceptsCondition(string condition)
		{
			return externalConditions
				.Any(t => t.Info.Condition == condition && t.CanGrantCondition(this));
		}
	}
}
