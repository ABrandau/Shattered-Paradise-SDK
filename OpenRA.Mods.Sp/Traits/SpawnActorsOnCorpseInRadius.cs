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

using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.SP.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Sp.Traits
{
	public enum RevieveOwnerType { SelfOwner, CorpseOwner, InternalName }

	class SpawnActorsOnCorpseInRadiusInfo : PausableConditionalTraitInfo
	{
		[Desc("Corpse type.")]
		public readonly HashSet<string> Types = new() { "corpse" };

		[Desc("Accepts corpse of those player relationships.")]
		public readonly PlayerRelationship ValidRelationships = PlayerRelationship.Ally | PlayerRelationship.Neutral | PlayerRelationship.Enemy;

		[Desc("Valid corpse horizontal distance.")]
		public readonly WDist Range = WDist.FromCells(5);

		[FieldLoader.Require]
		[ActorReference]
		[Desc("Actor type.")]
		public readonly string ActorType = null;

		[Desc("Owner of the spawned actor. Allowed keywords:" +
			"'Attacker' and 'InternalName'.")]
		public readonly RevieveOwnerType OwnerType = RevieveOwnerType.SelfOwner;

		[Desc("Map player to use when 'InternalName' is defined on 'OwnerType'.")]
		public readonly string InternalOwner = "Neutral";

		[Desc("Map player to transfer this actor to if the owner lost the game.")]
		public readonly string FallbackOwner = "Creeps";

		public override object Create(ActorInitializer init)
		{
			return new SpawnActorsOnCorpseInRadius(this, init.Self);
		}
	}

	class SpawnActorsOnCorpseInRadius : PausableConditionalTrait<SpawnActorsOnCorpseInRadiusInfo>, ICorpseConsumer, ITick
	{
		readonly Actor self;
		readonly List<(Player Owner, WPos Pos, CPos Loc)> recordedSpawn = new();

		public SpawnActorsOnCorpseInRadius(SpawnActorsOnCorpseInRadiusInfo info, Actor self)
			: base(info)
		{
			this.self = self;
		}

		public void Tick(Actor self)
		{
			if (IsTraitDisabled || IsTraitPaused)
				return;

			foreach ((var owner, var pos, var loc) in recordedSpawn)
			{
				var validowner = owner;
				switch (Info.OwnerType)
				{
					case RevieveOwnerType.SelfOwner:
						validowner = self.Owner;
						break;
					case RevieveOwnerType.InternalName:
						validowner = self.World.Players.First(p => p.InternalName == Info.InternalOwner);
						break;
					case RevieveOwnerType.CorpseOwner:
						break;
				}

				validowner = (validowner != null && validowner.WinState != WinState.Lost) ? validowner : self.World.Players.First(p => p.InternalName == Info.FallbackOwner);

				var td = new TypeDictionary
				{
					new LocationInit(loc),
					new OwnerInit(validowner),
					new CenterPositionInit(pos)
				};

				self.World.AddFrameEndTask(w => w.CreateActor(Info.ActorType, td));
			}

			recordedSpawn.Clear();
		}

		public bool TryAddCorpse(string type, CPos loc, WPos pos, Player owner)
		{
			if (!Info.Types.Contains(type) || IsTraitDisabled || !Info.ValidRelationships.HasRelationship(owner.RelationshipWith(self.Owner)) || (pos - self.CenterPosition).HorizontalLengthSquared > Info.Range.LengthSquared)
				return false;
			else
			{
				recordedSpawn.Add((owner, pos, loc));
				return true;
			}
		}
	}
}
