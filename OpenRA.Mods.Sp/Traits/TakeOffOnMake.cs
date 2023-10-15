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

using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits
{
	[Desc("Take off imediately after creation. For SP jumjet")]
	public class TakeOffOnMakeInfo : TraitInfo, Requires<AircraftInfo>
	{
		public override object Create(ActorInitializer init) { return new TakeOffOnMake(); }
	}

	public class TakeOffOnMake : INotifyIdle
	{
		bool hasTakenOff;
		public TakeOffOnMake() { }

		void INotifyIdle.TickIdle(Actor self)
		{
			if (hasTakenOff)
				return;

			hasTakenOff = true;
			self.QueueActivity(new TakeOff(self));
		}
	}
}
