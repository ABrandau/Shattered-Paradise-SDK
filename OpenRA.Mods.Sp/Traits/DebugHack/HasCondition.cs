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

using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	[Desc("Hack: tell yaml checker there is the condition, to pass the yaml check.")]
	sealed class HasConditionInfo : ConditionalTraitInfo
	{
		[FieldLoader.Require]
		[GrantedConditionReference]
		[Desc("Condition unit has.")]
		public readonly string Condition = null;

		public override object Create(ActorInitializer init) { return new HasCondition(); }
	}

	sealed class HasCondition
	{
		public HasCondition() { }
	}
}
