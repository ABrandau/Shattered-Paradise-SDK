--[[
   Copyright The OpenRA Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

Objectives = function()
	FinishTrainingObjective = AddPrimaryObjective(LocalPlayer, "objective-finish-train")
end

-- ####### Mission Map Set up
BridgeTargets = {BridgeTaget1, BridgeTaget2, BridgeTaget3, BridgeTaget4}
MissionMapSetUp = function()
	Camera.Position = MissionStartpoint.CenterPosition
	for k,brigde in ipairs(BridgeTargets) do
		BridgeDemo.Demolish(brigde)
	end

	Utils.Do(LocalPlayer.GetActorsByType("gdie1"), function(a)
		a.Flash(HSLColor.FromHex("FFFFFF"), 20, DateTime.Seconds(1) / 4)
		RespawnOnFlagOnKill(a)
	end)

	IntroductionInfo()
end

-- ####### information
IntroductionInfo = function()
	UserInterface.SetMissionText(UserInterface.Translate("mission-lua-begin"))
	Notification("eva-lua-intro-1")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
			Tip("tip-lua-usage-select")
	end)
	Trigger.AfterDelay(DateTime.Seconds(14), function()
			Tip("tip-lua-usage-move")
	end)
end

FirstFlagMessage = function()
	Notification("eva-lua-repair-bridge")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
			Tip("tip-lua-usage-engineer")
			RepairHut.Flash(HSLColor.FromHex("FFFFFF"), 40, DateTime.Seconds(1) / 4)
	end)
end


SecondFlagMessage = function()
	Notification("eva-lua-enemy")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
			Tip("tip-lua-usage-attack")
	end)
	Trigger.AfterDelay(DateTime.Seconds(14), function()
			Tip("tip-lua-usage-attackmove")
	end)
end

ThirdFlagMessage = function()
	Notification("eva-lua-tiberium1")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
			Tip("tip-lua-aware-tiberium")
	end)
end

FourthFlagMessage = function()
	Notification("eva-lua-tiberium2")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
			Tip("tip-lua-usage-barrack-heal")
	end)
end

FifthFlagMessage = function()
	Notification("eva-lua-enemy")
end

SixthFlagMessage = function()
	Notification("eva-lua-vein")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
			Tip("tip-lua-aware-vein")
	end)
end

SeventhFlagMessage = function()
	Notification("eva-lua-sd")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
			Tip("tip-lua-usage-sd")
	end)
end

EighthFlagMessage = function()
	Notification("eva-lua-cargo")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
			Tip("tip-lua-usage-load")
			Utils.Do(LocalPlayer.GetActorsByType("gdie1"), function(a)
				a.Flash(HSLColor.FromHex("FFFFFF"), 40, DateTime.Seconds(1) / 4)
			end)
	end)
	Trigger.AfterDelay(DateTime.Seconds(14), function()
			Tip("tip-lua-usage-unload")
	end)
end

NinthFlagMessage = function()
	Notification("eva-lua-carryall")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
			Tip("tip-lua-usage-carry")
			Utils.Do(LocalPlayer.GetActorsByType("apc"), function(a)
				a.Flash(HSLColor.FromHex("FFFFFF"), 40, DateTime.Seconds(1) / 4)
			end)
	end)
	Trigger.AfterDelay(DateTime.Seconds(14), function()
			Tip("tip-lua-usage-deliver")
	end)
end

FinalFlagMessage = function()
	Notification("eva-lua-finish")
	UserInterface.SetMissionText(UserInterface.Translate("eva-lua-finish"))
end

Notification = function(text)
	TranslatedNotification("eva-lua-name", text, "FFFF00")
end

Tip = function(text)
	TranslatedNotification("tip-lua-name", text, "29F3CF")
end

-- ######## Reinforement Spawn
PlayerReinforementSpawn = function(units, path)
	Reinforcements.Reinforce(GDI_AI, units, { path[1] }, 10, function(a)
		a.Owner = LocalPlayer
		a.Flash(HSLColor.FromHex("FFFFFF"), 40, DateTime.Seconds(1) / 4)
		RespawnOnFlagOnKill(a)
		if path[2] ~= nil then
			a.Move(path[2])
		end
	end)

	Media.PlaySpeechNotification(LocalPlayer, "ReinforcementsArrived")
end

-- ####### Player's units revive on the last flag if get killed
RespawnOnFlagOnKill = function(actor)
	if (FlagRespawnPoint == nil) then
		return
	end

	Trigger.OnKilled(actor, function(s,k)
		respawned = Actor.Create(actor.Type, true, {Owner = actor.Owner, Location = FlagRespawnPoint.Location})
		if respawned ~= nil then
			respawned.Move(FlagRespawnPoint.Location - CVec.New(1,1))
			RespawnOnFlagOnKill(respawned)
		end
	end)
end

--  ####### WorldLoaded and Mission Main
Flags = {Flag1, Flag2, Flag3, Flag4, Flag5, Flag6, Flag7, Flag8, Flag9, Flag10}
EngineerPath = {EngRein1.Location, EngRein1_1.Location}
HeavySupportPath = {HeavyRein1.Location, HeavyRein1_1.Location}
ApcPath = {ApcRein1.Location, ApcRein1_1.Location}
TransportPath = {TransportRein1.Location, Flag9.Location}

WorldLoaded = function()
	LocalPlayer = Player.GetPlayer("You")
	GDI_AI = Player.GetPlayer("GDI")
	Nod_AI = Player.GetPlayer("Creeps")
	FlagRespawnPoint = MissionStartpoint
	Objectives()
	MissionMapSetUp()

	for flagIndex,flag in ipairs(Flags) do
		
		-- First flag is prepared for player to activate
		if flagIndex == 1 then	
			flag.Owner = Nod_AI
			Radar.Ping(LocalPlayer, flag.CenterPosition, HSLColor.FromHex("00FF00"))
		end

		Trigger.OnEnteredProximityTrigger(flag.CenterPosition, WDist.New(1024 * 2), function(a, id)
			if a.Owner == LocalPlayer and flag.Owner == Nod_AI and a.Type == "gdie1" then

				-- Avoid trigger multiple time when 2 infantry step into at the same time
				if Flags[flagIndex] ~= nil then
					Flags[flagIndex] = nil
				else
					return
				end

				Trigger.RemoveProximityTrigger(id)
				flag.Owner = GDI_AI
				FlagRespawnPoint = flag

				-- Flag event based on flag index
				if flagIndex == 1 then
					PlayerReinforementSpawn({"engineer" }, EngineerPath)
					FirstFlagMessage()
				elseif flagIndex == 2 then
					SecondFlagMessage()
				elseif flagIndex == 3 then
					Medic.Owner = GDI_AI
					ThirdFlagMessage()
				elseif flagIndex == 4 then
					Barrack.Owner = GDI_AI
					Barrack.Flash(HSLColor.FromHex("FFFFFF"), 20, DateTime.Seconds(1) / 4)
					FourthFlagMessage()
				elseif flagIndex == 5 then
					FifthFlagMessage()
				elseif flagIndex == 6 then
					PlayerReinforementSpawn({"smech", "mmch", "hvr" }, HeavySupportPath)
					SixthFlagMessage()
				elseif flagIndex == 7 then
					ServeDepot.Owner = GDI_AI
					ServeDepot.Flash(HSLColor.FromHex("FFFFFF"), 20, DateTime.Seconds(1) / 4)
					SeventhFlagMessage()
				elseif flagIndex == 8 then
					PlayerReinforementSpawn({"apc" }, ApcPath)
					EighthFlagMessage()
				elseif flagIndex == 9 then
					PlayerReinforementSpawn({"trnsport" }, TransportPath)
					NinthFlagMessage()
				elseif flagIndex == 10 then
					LocalPlayer.MarkCompletedObjective(FinishTrainingObjective)
					FinalFlagMessage()
				end

				-- Next flag is prepared for player to activate
				local nextIndex = flagIndex + 1
				local nextFlag = Flags[nextIndex]
				if nextFlag == nil then
					return
				end
				
				nextFlag.Owner = Nod_AI
				Radar.Ping(LocalPlayer, nextFlag.CenterPosition, HSLColor.FromHex("00FF00"))
			end
		end)
	end
end
