--[[
   Copyright 2007-2022 The OpenRA Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

Objectives = function()
	Objective1 = player.AddPrimaryObjective("Recapture our lost M.C.V.")
	Objective2 = player.AddPrimaryObjective("Identify the source the unkown signal.")
	Objective3 = player.AddPrimaryObjective("Secure the source the unkown signal.")
	Camera.Position = Actor1022.CenterPosition
end

IntroductionInfo = function()
	UserInterface.SetMissionText("An artifact of alien origin has been detected nearby, explore the area and recapture our lost M.C.V.\nTip: You can recycle the Work Unit for credits by using our Stasis Chamber.")
	Trigger.AfterDelay(DateTime.Seconds(40), CleanMissionText)
end

MCVFoundMessage = function()
	UserInterface.SetMissionText("M.C.V. located, Nod had no time to hide it. Recapture the M.C.V. before Nod restart it!")
	ai2.GrantCondition("enable-ai")
	Trigger.AfterDelay(DateTime.Seconds(15), CleanMissionText)
end

NodWarnedMessage = function()
	UserInterface.SetMissionText("Nod has been notified of our M.C.V. and start to engage. Some of our technology is leaked, be careful.")
	Trigger.AfterDelay(DateTime.Seconds(15), CleanMissionText)
	player.MarkCompletedObjective(Objective1)
	ai.GrantCondition("enable-ai")
end

ReplicatorFoundMessage = function()
	UserInterface.SetMissionText("It seems this structure is the source of the unkown signals, capture it.")
	Trigger.AfterDelay(DateTime.Seconds(15), CleanMissionText)
	player.MarkCompletedObjective(Objective2)
end

MissionCompleteMessage = function()
	UserInterface.SetMissionText("Mission Complete.")
	Trigger.AfterDelay(DateTime.Seconds(15), CleanMissionText)
	player.MarkCompletedObjective(Objective3)
end


CleanMissionText = function()
	UserInterface.SetMissionText("")
end

Militants = {Actor502, Actor503, Actor504}
MilitantPatrolPath = { Actor1207, Actor1208, Actor1209, Actor1210, Actor1211, Actor1212}

WorldLoaded = function()
	player = Player.GetPlayer("Cabal")
	ai = Player.GetPlayer("Nod1")
	ai2 = Player.GetPlayer("Nod2")
	Objectives()
	IntroductionInfo()
	Trigger.OnEnteredProximityTrigger(Actor1023.CenterPosition, WDist.New(1024 * 15), function(a, id)
		if a.Owner == player then
			Trigger.RemoveProximityTrigger(id)
			baseCamera = Actor.Create("camera", true, { Owner = player, Location = Actor1023.Location })
			MCVFoundMessage()
		end
	end)

	Trigger.OnEnteredProximityTrigger(replicator.CenterPosition, WDist.New(1024 * 15), function(a, id)
		if a.Owner == player then
			Trigger.RemoveProximityTrigger(id)
			baseCamera = Actor.Create("camera", true, { Owner = player, Location = replicator.Location })
			ReplicatorFoundMessage()
		end
	end)

	Trigger.OnCapture(Actor1023, function() NodWarnedMessage()
	end)

	Trigger.OnCapture(replicator, function() MissionCompleteMessage()
	end)

end 
