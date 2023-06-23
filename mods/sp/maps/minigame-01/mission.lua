--[[
   Copyright The OpenRA Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

Difficulty = Map.LobbyOption("difficulty")

GDIWays ={
	{Reinforce_1.Location, Way1.Location, Way2.Location, Way2_up_1.Location, Way2_up_2.Location, Way2_up_3.Location},
	{Reinforce_1.Location, Way1.Location, Way2.Location, Way2_down_1.Location, Way2_down_2.Location, Way2_down_3.Location, Way2_down_4.Location}
}

GDIForces = {"hvr", "hvr", "apc", "mmch", "mmch", "sonic", "sonic", "g4tnk", "g4tnk", "mcv" }

MissionText = function()
	Objective1 = player.AddPrimaryObjective("Survive and protect MCV as many as you can!")
	SecondaryObjective1 = player.AddSecondaryObjective("Protect at least 4 MCVs.")
	Media.DisplayMessage("Select this defence and attack target manually!", "Tip", HSLColor.FromHex("29F3CF"))
	IonTur.Flash(HSLColor.FromHex("FFFFFF"), 15, DateTime.Seconds(1) / 4)
end

CheckObjectivesOnMissionEnd = function(survived)
	if survived then
		player.MarkCompletedObjective(Objective1)
	else
		player.MarkFailedObjective(Objective1)
	end

	if MCVprotected >= 4 then
		player.MarkCompletedObjective(SecondaryObjective1)
	else
		player.MarkFailedObjective(SecondaryObjective1)
	end
end

SendWaveLoop = function()
	Reinforcements.Reinforce(gdi_ai, GDIForces, Utils.Random(GDIWays), 60,  function(a)
		if a.Type == "mcv" then
			MCVprotected = MCVprotected + 1
			Media.DisplayMessage(string.format("A MCV has been secured! You have saved %d.", MCVprotected), "Notification", HSLColor.FromHex("EEEE66"))
		end
		a.Destroy()
	end)

	Waves = Waves - 1
	if Waves <= 0 then
		return
	end

	Trigger.AfterDelay(1000, function()
		SendWaveLoop()
	end)
end


Tick = function()
	if RemainingTime >= 0 then
		RemainingTime = RemainingTime - 1
		UserInterface.SetMissionText( "Remaining Time: " .. Utils.FormatTime(RemainingTime))
	else
		UserInterface.SetMissionText("You Survived!")
		CheckObjectivesOnMissionEnd(true)
	end
end

WorldLoaded = function()
	bandits_ai = Player.GetPlayer("Bandits")
	Actor.Create("upgrade.tiberium_gas_warheads", true, { Owner =  bandits_ai})
	if Difficulty == "hard" then
		Actor.Create("upgrade.lynx_rockets",  true, { Owner =  bandits_ai})
		Actor.Create("upgrade.tiberium_infusion", true, { Owner =  bandits_ai})
	end
	
	gdi_ai = Player.GetPlayer("GDI")
	player = Player.GetPlayer("You")
	MCVprotected = 0
	Waves = 6
	RemainingTime = 6800

	MissionText()
	Trigger.AfterDelay(200, function()
		SendWaveLoop()
	end)

	Trigger.OnKilled(IonTur, function(s, k)
		CheckObjectivesOnMissionEnd(false)
	end)
end
