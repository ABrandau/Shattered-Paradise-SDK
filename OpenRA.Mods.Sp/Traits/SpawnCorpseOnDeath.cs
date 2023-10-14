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
using OpenRA.Mods.SP.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Sp.Traits
{
	[Desc("When killed, this actor notify nearby place who use corpse for some effects, such as " + nameof(SpawnActorsOnCorpseInRadius) + ".")]
	class SpawnCorpseOnDeathInfo : TraitInfo
	{
		[Desc("Corpse type.")]
		public readonly string Type = "corpse";

		[Desc("How many times of a corpse can be used.")]
		public readonly int ReusableCount = 1;

		public override object Create(ActorInitializer init)
		{
			return new SpawnCorpseOnDeath(this);
		}
	}

	class SpawnCorpseOnDeath : INotifyKilled
	{
		readonly SpawnCorpseOnDeathInfo info;

		public SpawnCorpseOnDeath(SpawnCorpseOnDeathInfo info)
		{
			this.info = info;
		}

		void INotifyKilled.Killed(Actor self, AttackInfo e)
		{
			if (!self.IsInWorld)
				return;

			var used = 0;

			foreach (var t in self.World.ActorsWithTrait<ICorpseConsumer>())
			{
				if (t.Trait.TryAddCorpse(info.Type, self.Location, self.CenterPosition, self.Owner))
					used++;
				if (used >= info.ReusableCount)
					break;
			}
		}
	}
}
