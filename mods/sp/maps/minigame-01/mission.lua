--[[
   Copyright The OpenRA Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

Difficulty = Map.LobbyOption("difficulty")

Tick = function()
	if RemainingTime >= 0 then
		RemainingTime = RemainingTime - 1
		UserInterface.SetMissionText( "Remaining Time: " .. Utils.FormatTime(RemainingTime))
	else
		UserInterface.SetMissionText("You Win!")
		player.MarkCompletedObjective(Objective1)
	end
end

WorldLoaded = function()
	if Difficulty == "hard" then
		bandits_ai = Player.GetPlayer("Bandits")
		Actor.Create("upgrade.tiberium_gas_warheads", true, { Owner =  bandits_ai})
		Actor.Create("upgrade.lynx_rockets",  true, { Owner =  bandits_ai})
		Actor.Create("upgrade.tiberium_infusion", true, { Owner =  bandits_ai})
	end
	
	player = Player.GetPlayer("GDI")
	Objective1 = player.AddPrimaryObjective("Survive!")
	RemainingTime = 4000

	Media.DisplayMessage("Select this defence and attack target manually!", "Tip", HSLColor.FromHex("29F3CF"))
	IonTur.Flash(HSLColor.FromHex("FFFFFF"), 15, DateTime.Seconds(1) / 4)

	Trigger.OnKilled(IonTur, function(self, killer)
		player.MarkFailedObjective(Objective1)
	end)
end
