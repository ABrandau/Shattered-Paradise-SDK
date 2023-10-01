--[[
   Copyright The OpenRA Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

NodIconASCII =
[[
 
 
                1000000000001                                                                                                                                                                        
             101                    101                                                                                                                                                                     
           101      0000000      101                                                                                                                                                                 
         101    0001      10001  101                                                                                                                                                               
       101    000              000    101                                                                                                                                                             
     101     001              10001    101                                                                                                                                                          
    101        10               000         101                                                                                                                                                        
  101              1         10001           101                                                                                                                                                      
    101                      0000            101                                                                                                                                                        
      101               100101          101                                                                                                                                                           
        101         100111            101                                                                                                                                                             
         100000000000000000001                                                                                                                                                               
 
 
]]

BadCodes = {
"",
"",
"",
"",
"0 1 0 1 0 1 0 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 0 1 0 1 0 0 1 0 0 0 0 0 0 1 0 1 0 1 0 0 0 0 0 1 0 1 0 1 0 0 1 0 0 1 0 0 1\n",
"1 1 1 0 1 0 1 0 1 0 1 0 1 0 0 1 1 0 0 0 0 1 0 1 0 0 1 1 0 1 0 1 0 1 0 0 1 0 0 0 1 0 1 0 1 0 1 0 1 1 1 1 0 0 1 0 1 0 1 0 0 1 0 1 0 0 1 0 1\n",
"0 0 1 0 1 0 0 0 0 1 0 0 1 1 1 1 0 1 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 1 0 0 1 0 1 0 1 0 0 0 0 0 0 1 1 1 1 1 1 0 0 0 0 0 0 1 0 1 0 1 0 1 0 1\n",
"1 0 0 1 0 1 0 1 0 0 0 1 1 0 1 1 1 0 0 1 0 1 0 1 0 1 0 1 1 0 1 0 0 1 0 1 0 1 0 1 0 1 0 0 1 0 1 0 1 0 0 1 1 0 1 0 0 0 0 0 1 1 0 0 0 0 0 1 0\n"
}


Objectives = function()
	PowerDownTrainingObjective = AddPrimaryObjective(LocalPlayer, "objective-powerdown")
	SellTrainingObjective = AddPrimaryObjective(LocalPlayer, "objective-sell")
	RepairTrainingObjective = AddPrimaryObjective(LocalPlayer, "objective-repair")
	MCVTrainingObjective = AddPrimaryObjective(LocalPlayer, "objective-mcv")
	BuildBuildingObjective = AddPrimaryObjective(LocalPlayer, "objective-build")
	PlaceHolderObjective = AddPrimaryObjective(LocalPlayer, "objective-placeholder")
end

EndGameObjectiveCheck = function()
	LocalPlayer.MarkCompletedObjective(PowerDownTrainingObjective)
	LocalPlayer.MarkCompletedObjective(SellTrainingObjective)
	LocalPlayer.MarkCompletedObjective(RepairTrainingObjective)
	LocalPlayer.MarkCompletedObjective(MCVTrainingObjective)
	LocalPlayer.MarkCompletedObjective(BuildBuildingObjective)
	LocalPlayer.MarkCompletedObjective(PlaceHolderObjective)
	HelpNodObjective = AddPrimaryObjective(LocalPlayer, "objective-helpnod")
	LocalPlayer.MarkCompletedObjective(HelpNodObjective)
end

-- ####### Mission Map Set up
MissionMapSetUp = function()
	Camera.Position = MCVBeacon.CenterPosition
	LocalPlayer.Cash = 0
	CurrentMissionText = UserInterface.Translate("mission-lua-begin")
end


Notification = function(text)
	TranslatedNotification("eva-lua-name", text, "FFFF00")
end

Tip = function(text)
	TranslatedNotification("tip-lua-name", text, "29F3CF")
end

NotificationHacked = function(text)
	TranslatedNotification("badeva-lua-name", text, "FF0000")
end

GeneralSays = function(text)
	TranslatedNotification("general-lua-name", text, "88FF00")
end

KaneSays = function(text)
	TranslatedNotification("kane-lua-name", text, "FF0000")
end

EndGame = function()
	KaneSays("kane-lua-end1")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
		KaneSays("kane-lua-end2")
	end)
	Trigger.AfterDelay(DateTime.Seconds(14), function()
		EndGameObjectiveCheck()
	end)
end

BeginPowerdownMessage = function()
	Notification("eva-lua-begin")
	Trigger.AfterDelay(DateTime.Seconds(6), function()
		Notification("eva-lua-powerdown")
	end)
	Trigger.AfterDelay(DateTime.Seconds(13), function()
		Tip("tip-lua-usage-powerdown")
	end)
end

BeginSellMessage = function()
	Notification("eva-lua-sell")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
		Tip("tip-lua-usage-sell")
	end)
end

NodAssualtTriggered = false

BeginRepairMessage = function()
	Notification("eva-lua-repair")
	MCVBeacon.Flash(HSLColor.FromHex("FFFFFF"), 25, DateTime.Seconds(1) / 4)
	Trigger.AfterDelay(DateTime.Seconds(7), function()
		if not NodAssualtTriggered then
			Tip("tip-lua-usage-repair")
		end
	end)
end

DeployMCVMessage = function()
	Notification("eva-lua-mcv")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
		if not NodAssualtTriggered then
			Tip("tip-lua-usage-mcv")
		end
	end)
end

BuildBuildingMessage = function()
	Notification("eva-lua-build")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
		if not NodAssualtTriggered then
			Tip("tip-lua-usage-build1")
		end
	end)
	Trigger.AfterDelay(DateTime.Seconds(14), function()
		if not NodAssualtTriggered then
			Tip("tip-lua-usage-build2")
		end
	end)
end

ProduceInfantryMessage = function()
	Trigger.AfterDelay(DateTime.Seconds(17), function()
		if not NodAssualtTriggered then
			Notification("eva-lua-congrat")
		end
	end)
	Trigger.AfterDelay(DateTime.Seconds(24), function()
		if not NodAssualtTriggered then
			Notification("eva-lua-gdie1")
			DramaticNodAssualt()
		end
	end)
end

BuildingBuiltList =
{
	["gacnst"] = false,
	["anypower"] = false,
	["proc"] = false,
	["gapile"] = false
}

BuildingIntroMessage = function(type)
	if type == "anypower" then
		Tip("tip-lua-powerplant")
	elseif type == "proc" then
		Tip("tip-lua-proc")
	elseif type == "gapile" then
		Tip("tip-lua-barrack")
	end
end

CheckBuildingObjective = function()
	local unfinished = 0

	for key,built in pairs(BuildingBuiltList) do
		if not built then
			if LocalPlayer.HasPrerequisites({key}) then
				BuildingBuiltList[key] = true
				BuildingIntroMessage(key)
			else
				unfinished = unfinished + 1
			end
		end
	end

	if unfinished == 0 then
		LocalPlayer.MarkCompletedObjective(BuildBuildingObjective)
		BuildingObjectiveCheckDelay = -1
	end
end

NodAssualtBegin = function()
	Reinforcements.Reinforce(Nod_AI, {"apache", "apache", "apache", "apache"}, {NodSubway.Location}, 100)
	NodAssualtLoop()
end

NodReinPoints = {NodReinWay1.Location, NodReinWay2.Location, NodReinWay3.Location}
NodAssualtLoop = function()
	local remaining = 0

	for key,v in pairs(BuildingBuiltList) do
		if LocalPlayer.HasPrerequisites({key}) then
			remaining = remaining + 1
		end
	end

	if remaining == 0 then
		EndGame()
		return
	end

	-- spawn infantry
	Reinforcements.Reinforce(Nod_AI, {"altnode1", "altnode1", "templar", "templar", "crusader"}, {Utils.Random(NodReinPoints)}, 3, function(a)
		a.AttackMove(NodAttackPoint.Location)
	end)
	
	-- spawn vehicle
	Trigger.AfterDelay(20, function()
		Reinforcements.Reinforce(Nod_AI, {"attackbike", "bggy", "ttnk", "ttnk"}, {Utils.Random(NodReinPoints)}, 10, function(a)
			a.AttackMove(NodAttackPoint.Location)
		end)
	end)
	
	-- spawn suicide, send by AI
	Trigger.AfterDelay(30, function() 
		Reinforcements.Reinforce(Nod_AI, {"bike"}, {Utils.Random(NodReinPoints)})
	end)

	Trigger.AfterDelay(250, function() 
		Reinforcements.ReinforceWithTransport(Nod_AI, "sapc",	{"cyborg", "cyborg", "cyborg", "glad", "glad"}, {NodSubway.Location, NodAttackPoint.Location}, {NodAttackPoint.Location, NodSubway.Location})
	end)

	Trigger.AfterDelay(400, function()
		NodAssualtLoop()
	end)
end

EVAHackedStage = 0
BuildingObjectiveCheckDelay = -1  -- Set -1 to disable Building Objective Check
DramaticNodAssualt = function()
	if NodAssualtTriggered then
		return
	end
	NodAssualtTriggered = true
	BuildingObjectiveCheckDelay = -1

	Trigger.AfterDelay(DateTime.Seconds(1), function()
		GeneralSays("gen-lua-say1")
	end)
	Trigger.AfterDelay(DateTime.Seconds(2), function()
		GeneralSays("gen-lua-say2")
	end)
	Trigger.AfterDelay(DateTime.Seconds(8), function()
		GeneralSays("gen-lua-say3")
	end)
	Trigger.AfterDelay(DateTime.Seconds(16), function()
		GeneralSays("gen-lua-say4")
	end)
	Trigger.AfterDelay(DateTime.Seconds(24), function()
		GeneralSays("gen-lua-say5")
	end)

	Trigger.AfterDelay(DateTime.Seconds(31), function()
		Notification("eva-lua-kane")
		EVAHackedStage = 1
		NodAssualtBegin()
	end)
	Trigger.AfterDelay(DateTime.Seconds(38), function()
		NotificationHacked("badeva-lua-kane1")
		EVAHackedStage = 2
	end)
	Trigger.AfterDelay(DateTime.Seconds(45), function()
		NotificationHacked("badeva-lua-kane2")
		EVAHackedStage = 3
	end)
	Trigger.AfterDelay(DateTime.Seconds(52), function()
		KaneSays("kane-lua-lives")
		EVAHackedStage = 4
	end)
	Trigger.AfterDelay(DateTime.Seconds(60), function()
		KaneSays("kane-lua-game1")
	end)
	Trigger.AfterDelay(DateTime.Seconds(67), function()
		KaneSays("kane-lua-game2")
	end)
end

-- ### Set Nod Hack text visual
NodHackMissionText = function(text)
	local randomBadcodes = ""
	for i=1,5*EVAHackedStage do
		randomBadcodes = randomBadcodes..Utils.Random(BadCodes)
	end

	-- Show Nod icon in ASCII when stage > 3
	if EVAHackedStage > 3 then
		if string.len(randomBadcodes) < 170 * EVAHackedStage then
			NodIconASCII = string.reverse(NodIconASCII)
		end
	
		randomBadcodes = randomBadcodes..NodIconASCII
	end

	CurrentMissionText = randomBadcodes
end

--  ####### Tick For Mission Text
CurrentMissionText = ""
Tick = function()
	-- ###### Base Building Mission Check
	if BuildingObjectiveCheckDelay == 0 then
		CheckBuildingObjective()
		BuildingObjectiveCheckDelay = DateTime.Seconds(1)
	elseif BuildingObjectiveCheckDelay > 0 then
		BuildingObjectiveCheckDelay = BuildingObjectiveCheckDelay - 1
	end

	if EVAHackedStage > 1 then
		NodHackMissionText()
	end

	UserInterface.SetMissionText(CurrentMissionText)
end

WorldLoaded = function()
	LocalPlayer = Player.GetPlayer("You")
	GDI_AI = Player.GetPlayer("GDI")
	Fake_Nod_AI = Player.GetPlayer("Creeps")
	Nod_AI = Player.GetPlayer("Nod")
	Objectives()
	MissionMapSetUp()
	GDIMCV = nil

	-- ###### Base Managing Mission
	-- PowerDown Mission
	BeginPowerdownMessage()
	Trigger.OnAllKilled(Fake_Nod_AI.GetActorsByTypes({"flamehologram","tickhologram"}), function()
		LocalPlayer.MarkCompletedObjective(PowerDownTrainingObjective)
	end)

	Trigger.OnObjectiveCompleted(LocalPlayer, function(_, objectiveID)
		-- If Kane already show himself, all missions are skipped
		if NodAssualtTriggered then
			return
		end

		-- Sell Mission
		if (objectiveID == PowerDownTrainingObjective) then
			local sellable = LocalPlayer.GetActorsByTypes({"gaplug","garadr","gavulc", "garock", "gapowr", "gacsam", "napuls"})
			-- if there is no building we consider training is finished
			if next(sellable) == nil then
				 -- Hack: add a delay to avoid `OnObjectiveCompleted` is called directly within `OnObjectiveCompleted`, which will crash the game
				Trigger.AfterDelay(2, function()
					LocalPlayer.MarkCompletedObjective(SellTrainingObjective)
				end)
			else
				BeginSellMessage()
				Trigger.OnAllRemovedFromWorld(sellable, function()
					LocalPlayer.MarkCompletedObjective(SellTrainingObjective)
				end)
			end
		-- Repair Mission
		elseif(objectiveID == SellTrainingObjective) then
			BeginRepairMessage()
			-- Give cash to avoid player stuck in this stage
			LocalPlayer.Cash = math.max(LocalPlayer.Cash, 4000)
			if not MCVBeacon.IsDead then
				MCVBeacon.Owner = LocalPlayer
				Trigger.OnKilled(MCVBeacon, function(_)
					local mcv_location = MCVBeacon.Location
					Reinforcements.ReinforceWithTransport(LocalPlayer, "dshp.high",	{"smech", "smech", "mmch"}, {McvReinWay.Location, mcv_location}, {mcv_location, McvReinWay.Location}, nil, function(trans, cargo)
						LocalPlayer.MarkCompletedObjective(RepairTrainingObjective)
						GDIMCV = Actor.Create("mcv", true, {Owner = LocalPlayer, Location = mcv_location})
					end)
				end)
			end
			-- Kane will show himself if player just stalls
			Trigger.AfterDelay(DateTime.Minutes(15), function()
				DramaticNodAssualt()
			end)
		-- Deploy MCV Mission
		elseif(objectiveID == RepairTrainingObjective) then
			-- Hack: add a delay to get MCV when it is in world
			Trigger.AfterDelay(2, function()
				if GDIMCV ~= nil and not GDIMCV.IsDead then
					DeployMCVMessage()
					GDIMCV.Flash(HSLColor.FromHex("FFFFFF"), 40, DateTime.Seconds(1) / 4)
					Trigger.OnRemovedFromWorld(GDIMCV, function(_)
						-- Hack: add a delay to get Conyard when it is in world
						Trigger.AfterDelay(2, function()
							if LocalPlayer.HasPrerequisites({"gacnst"}) then
								LocalPlayer.MarkCompletedObjective(MCVTrainingObjective)
							else
								LocalPlayer.MarkFailedObjective(MCVTrainingObjective)
							end
						end)
					end)
				end
			end)
		-- Build Building Mission
		elseif(objectiveID == MCVTrainingObjective) then
			BuildBuildingMessage()
			-- Activate Base Building Check
			BuildingObjectiveCheckDelay = DateTime.Seconds(1)
		-- Final Mission
		elseif(objectiveID == BuildBuildingObjective) then
			ProduceInfantryMessage()
		end
	end)
end
