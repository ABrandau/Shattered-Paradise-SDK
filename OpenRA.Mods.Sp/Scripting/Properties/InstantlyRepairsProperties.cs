#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Scripting;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Scripting
{
	[Desc("Deprecated: drop this class after next engine rebase to OpenRA upstream!!!")]
	[ScriptPropertyGroup("Ability")]
	public class InstantlyRepairsProperties : ScriptActorProperties, Requires<IMoveInfo>, Requires<InstantlyRepairsInfo>
	{
		readonly InstantlyRepairs[] instantlyRepairs;

		public InstantlyRepairsProperties(ScriptContext context, Actor self)
			: base(context, self)
		{
			instantlyRepairs = Self.TraitsImplementing<InstantlyRepairs>().ToArray();
		}

		[ScriptActorPropertyActivity]
		[Desc("Repair the target actor instantly.")]
		public void InstantlyRepair(Actor target)
		{
			var repair = instantlyRepairs.FirstEnabledConditionalTraitOrDefault();
			if (repair != null)
				Self.QueueActivity(new InstantRepair(Self, Target.FromActor(target), repair.Info));
		}
	}
}
