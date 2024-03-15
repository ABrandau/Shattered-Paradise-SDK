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

using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.SP.Traits.Sound
{
	[Desc("Plays a voice clip when the actor is built.",
		"HACK: production building must have Exit to enable this")]
	public class VoiceAnnouncementOnProductionExitInfo : TraitInfo
	{
		[VoiceReference]
		[FieldLoader.Require]
		[Desc("Voice to play.")]
		public readonly string Voice = null;

		[Desc("Player relationships who can hear this voice. When set to None, play to owner only.")]
		public readonly PlayerRelationship ValidRelationships = PlayerRelationship.Ally | PlayerRelationship.Neutral | PlayerRelationship.Enemy;

		public override object Create(ActorInitializer init) { return new VoiceAnnouncementOnProductionExit(init, this); }
	}

	public class VoiceAnnouncementOnProductionExit : INotifyCreated
	{
		readonly bool hasNoCreationActivityDelay;
		readonly VoiceAnnouncementOnProductionExitInfo info;

		public VoiceAnnouncementOnProductionExit(ActorInitializer init, VoiceAnnouncementOnProductionExitInfo info)
		{
			hasNoCreationActivityDelay = init.GetOrDefault<CreationActivityDelayInit>() == null;
			this.info = info;
		}

		void INotifyCreated.Created(Actor self)
		{
			if (hasNoCreationActivityDelay)
				return;

			var player = self.World.LocalPlayer ?? self.World.RenderPlayer;
			if (player == null)
				return;

			if (info.ValidRelationships.HasRelationship(self.Owner.RelationshipWith(player)))
				self.PlayVoice(info.Voice);
			else if (info.ValidRelationships == PlayerRelationship.None && self.Owner == player)
				self.PlayVoice(info.Voice);
		}
	}
}
