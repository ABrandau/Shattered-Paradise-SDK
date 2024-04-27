--[[
   Copyright (c) The OpenRA Developers and Contributors
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

Objectives = function()
	SecondaryObjectiveCaptureMCV = AddSecondaryObjective(LocalPlayer, "objective-mcv")
	SecondaryObjectiveHackAllArray = AddSecondaryObjective(LocalPlayer, "objective-hack-array")
	ObjectiveHackOneArray = AddPrimaryObjective(LocalPlayer, "objective-hack-one-array")
	ObjectiveFindAlien = AddPrimaryObjective(LocalPlayer, "objective-find-alien")
	ObjectiveCaptureAlien = AddPrimaryObjective(LocalPlayer, "objective-capture-alien")
	ObjectiveProtectHacker = AddPrimaryObjective(LocalPlayer, "objective-hacker-survive")
end

-- ####### Mission Map Set up
MissionMapSetUp = function()
	SpawnPatrollers()
	SpawnUpgrade()
	DifficultySetUp()
	Mut_AI.GrantCondition("disable-ai-load") -- disable load AI for shop keeper
	Camera.Position = MissionStartpoint.CenterPosition
	CabHacker.Patrol({WayPoint1662.Location, WayPoint1742.Location, WayPoint1743.Location}, false)
	Trigger.AfterDelay(DateTime.Seconds(12), function()
		CabHacker.Owner = LocalPlayer
	end)
end

Difficulty = Map.LobbyOption("difficulty")
DifficultySetUp = function()
	if Difficulty == "hard" then
		for key,unit in ipairs(Cab_AI.GetActorsByType("moth")) do
			unit.Destroy()
		end
	elseif Difficulty == "normal" then
		Engineer4.Destroy()
		Engineer5.Destroy()

		for key,unit in ipairs(Nod_AI2.GetActorsByTypes({"tdadvgtwr", "minelayer"})) do
			unit.Destroy()
		end

		for key,unit in ipairs(Nod_AI.GetActorsByType("scrin")) do
			unit.Destroy()
		end

		NodMcv1.Destroy()
	end
end


-- ####### End game check
CheckObjectivesOnMissionEnd = function(success)
	-- check the SecondaryObjective first
	-- 1. check if LocalPlayer has mcv or conyard at the end of the game
	if not LocalPlayer.IsObjectiveCompleted(SecondaryObjectiveHackAllArray) then
		LocalPlayer.MarkFailedObjective(SecondaryObjectiveHackAllArray)
	end

	-- 2. check if LocalPlayer has mcv or conyard at the end of the game
	if not LocalPlayer.IsObjectiveCompleted(SecondaryObjectiveCaptureMCV) then
		for key,unit in ipairs(LocalPlayer.GetActorsByType("cabmcv")) do
			LocalPlayer.MarkCompletedObjective(SecondaryObjectiveCaptureMCV)
			break
		end

		for key,unit in ipairs(LocalPlayer.GetActorsByType("cabyard")) do
			LocalPlayer.MarkCompletedObjective(SecondaryObjectiveCaptureMCV)
			break
		end

		if not LocalPlayer.IsObjectiveCompleted(SecondaryObjectiveCaptureMCV) then
			LocalPlayer.MarkFailedObjective(SecondaryObjectiveCaptureMCV)
		end
	end

	if success then
		LocalPlayer.MarkCompletedObjective(ObjectiveCaptureAlien)
	else
		LocalPlayer.MarkFailedObjective(ObjectiveCaptureAlien)
	end

	if not LocalPlayer.IsObjectiveCompleted(ObjectiveHackOneArray) then
		LocalPlayer.MarkFailedObjective(ObjectiveHackOneArray)
	end

	if not LocalPlayer.IsObjectiveCompleted(ObjectiveFindAlien) then
		LocalPlayer.MarkFailedObjective(ObjectiveFindAlien)
	end
	
	if CabHacker.IsDead then
		LocalPlayer.MarkFailedObjective(ObjectiveProtectHacker)
	else
		LocalPlayer.MarkCompletedObjective(ObjectiveProtectHacker)
	end

end

-- ####### information
IntroductionInfo = function()
	CurrentMissionText = UserInterface.Translate("mission-lua-begin")
	Notification("cabal-lua-intro-1")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
			Notification("cabal-lua-intro-2")
			if not CabHacker.IsDead then
				CabHacker.Flash(HSLColor.FromHex("FFFFFF"), 20, DateTime.Seconds(1) / 4)
			end
			if not Cradar3.IsDead then
				Cradar3.Flash(HSLColor.FromHex("FFFFFF"), 20, DateTime.Seconds(1) / 4)
			end
	end)
	Trigger.AfterDelay(DateTime.Seconds(14), function()
			Tip("tip-lua-usage-hacker")
	end)
end

MCVFoundMessage = function()
	Notification("cabal-lua-mcv-located")
	CabConyard.Flash(HSLColor.FromHex("FFFFFF"), 20, DateTime.Seconds(1) / 4)

	-- Warning before Nod AI triggered
	if not AwaredByNod then
		Trigger.AfterDelay(DateTime.Seconds(7), function()
			Warning("warning-lua-mcv-located")
		end)
		Trigger.AfterDelay(DateTime.Seconds(14), function()
			Tip("tip-lua-mcv-located")
		end)
	end
end

MCVThreatMessage = function()
	Notification("warning-lua-mcv-being-captured")
end

MCVSuccessMessage = function()
	if LocalPlayer.IsObjectiveCompleted(SecondaryObjectiveHackAllArray) and not Mech2.IsDead then
		Notification("cabal-lua-mcv-captured-success-2")
	elseif not Mech1.IsDead then
		Notification("cabal-lua-mcv-captured-success-1")
	end

	if Difficulty == "normal" then
		Trigger.AfterDelay(DateTime.Seconds(21), function()
			Notification("cabal-lua-mcv-captured-success-reinforce-1")
		end)
	elseif Difficulty == "hard" then
		Trigger.AfterDelay(DateTime.Seconds(21), function()
			Notification("cabal-lua-mcv-captured-success-reinforce-2")
		end)
	end
end

NodWarnedOnMCVMessage = function()
	Trigger.AfterDelay(DateTime.Seconds(11), function()
			Warning("warning-lua-mcv-captured-success-1")
	end)
	Trigger.AfterDelay(DateTime.Seconds(18), function()
			Warning("warning-lua-mcv-captured-success-2")
	end)
end

MCVFailedMessage = function()
	Notification("cabal-lua-mcv-captured-failed-1")

	if not (LocalPlayer.IsObjectiveCompleted(SecondaryObjectiveHackAllArray) or AwaredByNod) then
		Trigger.AfterDelay(DateTime.Seconds(15), function()
				Notification("cabal-lua-mcv-captured-failed-2")
		end)
		Trigger.AfterDelay(DateTime.Seconds(22), function()
				Notification("cabal-lua-mcv-captured-failed-3")
		end)
	end
end


NodAlertedMessage = function()
	Warning("warning-lua-nod-aware-attack")
end


HackOneArrayMessage = function()
	Notification("cabal-lua-hack-one-array-1")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
		Notification("cabal-lua-hack-one-array-2")
	end)
	Trigger.AfterDelay(DateTime.Seconds(14), function()
		Notification("cabal-lua-hack-one-array-3")
	end)

	CurrentMissionText = UserInterface.Translate("mission-lua-locate-alien")
end

HackTwoArrayMessage = function()
	Notification("cabal-lua-hack-two-array")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
		Tip("tip-lua-hack-two-array")
	end)
end

HackThreeArrayMessage = function()
	Notification("cabal-lua-hack-three-array-1")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
		Notification("cabal-lua-hack-three-array-2")
	end)

	-- Warning before Nod AI triggered
	if not AwaredByNod then
		Trigger.AfterDelay(DateTime.Seconds(14), function()
			Warning("warning-lua-hack-three-array")
		end)
	end
end

HackFourArrayMessage = function()
	Notification("cabal-lua-hack-four-array-1")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
		Notification("cabal-lua-hack-four-array-2")
	end)
end

NodWarnedOnFourHackedMessage = function()
	Trigger.AfterDelay(DateTime.Seconds(14), function()
		Warning("warning-lua-hack-four-array-1")
	end)
	Trigger.AfterDelay(DateTime.Seconds(21), function()
		Warning("warning-lua-hack-four-array-2")
	end)
end

ReplicatorFoundMessage = function()
	Notification("cabal-lua-find-alien")
	CurrentMissionText = UserInterface.Translate("mission-lua-capture-alien")
	LocalPlayer.MarkCompletedObjective(ObjectiveFindAlien)
end

MissionCompleteMessage = function()
	CurrentMissionText = UserInterface.Translate("mission-lua-complete")
	LocalPlayer.MarkCompletedObjective(ObjectiveCaptureAlien)
end

MercenaryFoundMessage = function()
	Notification("cabal-lua-find-mercenary-1")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
		Notification("cabal-lua-find-mercenary-2")
	end)
end

MeetMercenaryMessage = function()
	if IsShopAngry then
		return
	end

	ShopSays("warlord-lua-name","warlord-lua-intro-1")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
		if IsShopAngry then
			return
		end
		if LocalPlayer.IsObjectiveFailed(SecondaryObjectiveCaptureMCV) then
			ShopSays("soldier-lua-name","soldier-lua-intro-1")
		elseif MCVlostAgain then
			ShopSays("soldier-lua-name","soldier-lua-intro-2")
		elseif  not LocalPlayer.IsObjectiveCompleted(SecondaryObjectiveCaptureMCV) then
			ShopSays("soldier-lua-name","soldier-lua-intro-3")
		else
			ShopSays("soldier-lua-name","soldier-lua-intro-4")
		end
	end)
	Trigger.AfterDelay(DateTime.Seconds(14), function()
		if IsShopAngry then
			return
		end
		ShopSays("warlord-lua-name", "warlord-lua-intro-2")
	end)
	Trigger.AfterDelay(DateTime.Seconds(21), function()
		if IsShopAngry then
			return
		end
		ShopSays("warlord-lua-name", "warlord-lua-intro-3")
	end)
	Trigger.AfterDelay(DateTime.Seconds(28), function()
		if IsShopAngry then
			return
		end
		ShopSays("warlord-lua-name", "warlord-lua-intro-4")
	end)
	Trigger.AfterDelay(DateTime.Seconds(35), function()
		if IsShopAngry then
			return
		end
		Tip("tip-lua-trade-mercenary")
	end)
end

MercenaryMessageMadMessage = function()
	ShopSays("warlord-lua-name","warlord-lua-mad")
end

Notification = function(text)
	TranslatedNotification("cabal-lua-name", text, "1E90FF")
end

Tip = function(text)
	TranslatedNotification("tip-lua-name", text, "29F3CF")
end

Warning = function(text)
	TranslatedNotification("warning-lua-name", text, "FF1111")
end

ShopSays = function(who, text)
	TranslatedNotification(who, text, "0CBB01")
end


-- ####### Upgrade
SpawnUpgrade = function()
	Actor.Create("upgrade.tib_core_missiles", true, { Owner = Nod_AI })

	Actor.Create("upgrade.lynx_rockets", true, { Owner = Mut_AI })
	Actor.Create("upgrade.lynx_rockets", true, { Owner = Mut_AI2 })
	Actor.Create("upgrade.lynx_rockets", true, { Owner = LocalPlayer })
	Actor.Create("upgrade.fortified_upg", true, { Owner = Mut_AI })
	Actor.Create("upgrade.fortified_upg", true, { Owner = Mut_AI2 })
	Actor.Create("upgrade.fortified_upg", true, { Owner = LocalPlayer })
	Actor.Create("upgrade.tunnel_repairs", true, { Owner = Mut_AI })
	Actor.Create("upgrade.tunnel_repairs", true, { Owner = Mut_AI2 })
	Actor.Create("upgrade.tiberium_infusion", true, { Owner = Mut_AI })
	Actor.Create("upgrade.tiberium_infusion", true, { Owner = Mut_AI2 })
	Actor.Create("upgrade.tiberium_infusion", true, { Owner = LocalPlayer })
	Actor.Create("upgrade.tiberium_gas_warheads", true, { Owner = Mut_AI })
	Actor.Create("upgrade.tiberium_gas_warheads", true, { Owner = Mut_AI2 })
end

-- ######## Reinforement Spawn
PlayerReinforementSpawn = function(units, path, pingwhere, transport)
	if (transport ~= nil) then
		Reinforcements.ReinforceWithTransport(LocalPlayer, transport, units, path)
	else
		Reinforcements.Reinforce(LocalPlayer, units, path)
	end

	Media.PlaySpeechNotification(LocalPlayer, "ReinforcementsArrived")

	if (pingwhere ~= nil) then
		Radar.Ping(LocalPlayer, pingwhere, HSLColor.FromHex("00FF00"))
	end
end

-- ####### AI Patrol: We need an self made patrol only forces on Move instead of AttackMove
Patrol2A = function(unit, waypoints, delay)
	if unit.IsDead then
		return
	end

	unit.Move(waypoints[1])
	Trigger.AfterDelay(delay, function()
		Patrol2B(unit, waypoints, delay)
	end)
end

Patrol2B = function(unit, waypoints, delay)
	if unit.IsDead then
		return
	end

	unit.Move(waypoints[2])
	Trigger.AfterDelay(delay, function()
		Patrol2A(unit, waypoints, delay)
	end)
end

ScanvegerAPath = { WayPoint1399.Location, WayPoint1208.Location }
ScanvegerBPath = { WayPoint1208.Location, WayPoint1399.Location }
ScanvegerCPath = { WayPoint1544.Location, WayPoint1404.Location }
ScanvegerDPath = { WayPoint1404.Location, WayPoint1544.Location }
ScanvegerEPath = { WayPoint1400.Location, WayPoint1640.Location }
ScanvegerFPath = { WayPoint1640.Location, WayPoint1400.Location }

TanksPath1 = {WayPoint1346.Location, WayPoint1210.Location}
TanksPath2 = {WayPoint1346.Location, WayPoint1396.Location}
BikePath1 = { WayPoint1540.Location, WayPoint1207.Location }
BikePath2 = { WayPoint1540.Location, WayPoint1738.Location }

SpawnPatrollers = function()
		local scanvegerA = Actor.Create("truckb", true, { Owner = Nod_AI2, Location = ScanvegerAPath[2]})
		local scanvegerB = Actor.Create("truckb", true, { Owner = Nod_AI2, Location = ScanvegerBPath[2]})
		local scanvegerC = Actor.Create("truckb", true, { Owner = Nod_AI2, Location = ScanvegerCPath[2]})
		local scanvegerD = Actor.Create("truckb", true, { Owner = Nod_AI2, Location = ScanvegerDPath[2]})
		local scanvegerE = Actor.Create("truckb", true, { Owner = Nod_AI2, Location = ScanvegerEPath[2]})
		local scanvegerF = Actor.Create("truckb", true, { Owner = Nod_AI2, Location = ScanvegerFPath[2]})
		Patrol2A(scanvegerA, ScanvegerAPath, 600)
		Patrol2A(scanvegerB, ScanvegerBPath, 600)
		Patrol2A(scanvegerC, ScanvegerCPath, 2000)
		Patrol2A(scanvegerD, ScanvegerDPath, 2000)
		Patrol2A(scanvegerE, ScanvegerEPath, 1900)
		Patrol2A(scanvegerF, ScanvegerFPath, 1900)

		Patrol2A(Ctnk1, TanksPath1, 900)
		Patrol2A(Ctnk2, TanksPath1, 900)
		Patrol2A(Ctnk3, TanksPath1, 900)
		Patrol2A(Ctnk4, TanksPath2, 1300)
		Patrol2A(Ctnk5, TanksPath2, 1300)
		Patrol2A(Ctnk6, TanksPath2, 1300)

		Patrol2A(Bike1, BikePath1, 600)
		Patrol2A(Bike2, BikePath2, 600)
end

-- ####### Mech lose control function. Needs actor with "error" condition
GoMechError = function(unit, initial_owner)
	if not unit.IsDead then
		unit.GrantCondition("empdisable", 200)
		Trigger.AfterDelay(200, function()
			unit.Owner = initial_owner
			unit.GrantCondition("error")
		end)
		Trigger.AfterDelay(400, function()
			MechError(unit)
		end)
	end
end

MechError = function(unit)
	if unit.IsDead then
		return
	end

	unit.Stop()

	if unit.Owner == Nod_AI then
		unit.Owner = Creep_AI
		Trigger.AfterDelay(200, function()
			MechError(unit)
		end)
	elseif unit.Owner == Creep_AI then
		unit.Owner = LocalPlayer
		Trigger.AfterDelay(200, function()
			MechError(unit)
		end)
	elseif unit.Owner == LocalPlayer then
		unit.Owner = Nod_AI
		Trigger.AfterDelay(200, function()
			MechError(unit)
		end)
	end
end

-- ####### AI sell actors
AISell = function(unit)
	if (not unit.IsDead) and unit.Owner ~= LocalPlayer then
		unit.Sell()
	end
end

AISellMCVResearchBases = function()
	AISell(LabPowerplant1)
	AISell(LabPowerplant2)
	AISell(LabPowerplant3)
	AISell(LabPowerplant4)
	AISell(NodLab)
	AISell(NodGate2)
	AISell(NodGate1)
	AISell(NodGate3)
	AISell(NodGate4)
	AISell(NodGate5)
	AISell(Laser1)
	AISell(Laser2)
	AISell(Laser3)
	AISell(Laser4)
	AISell(Laser5)
	AISell(NodObli1)
	AISell(NodObli2)
	AISell(NodObli3)
	AISell(NodObli4)
	AISell(NodSam)
end

-- ####### AI capture actor

AICapture = function(unit, target)
	if not (unit.IsDead or target.IsDead) then
		unit.Capture(target)
	end
end

AICaptureMCV = function()
	AICapture(Engineer1,CabConyard)
	AICapture(Engineer2,CabConyard)
	AICapture(Engineer3,CabConyard)
	AICapture(Engineer4,CabConyard)
	AICapture(Engineer5,CabConyard)
	AICapture(Engineer6,CabConyard)
	AICapture(Engineer7,CabConyard)
	AICapture(Engineer8,CabConyard)
	AICapture(Engineer9,CabConyard)
	AICapture(Engineer10,CabConyard)
	AICapture(Engineer11,CabConyard)
	AICapture(Engineer12,CabConyard)
	AICapture(Engineer13,CabConyard)
end

-- ####### CybrogCommando Reinforcements
StartPointReinforcePath = { Reinforcepoint2.Location, WayPoint1440.Location, WayPoint1662.Location, WayPoint1742.Location, WayPoint1743.Location }
SpawnCybrogCommandoReinforcement = function()
	local haveCommando = false
	for key,unit in ipairs(LocalPlayer.GetActorsByType("cyc2")) do
		haveCommando = true
		break
	end

	if haveCommando then
		PlayerReinforementSpawn({"moth", "moth", "moth", "moth", "reapercab", "reapercab", "cborg", "cborg", "cborg", "glad", "glad", "glad"}, StartPointReinforcePath, WayPoint1743.CenterPosition, nil)
	else
		PlayerReinforementSpawn({"cyc2", "moth", "moth", "moth", "moth", "reapercab", "reapercab", "cborg", "cborg"}, StartPointReinforcePath, WayPoint1743.CenterPosition, nil)
	end
end

-- ####### Hack Array stroy line
ArraysNeedHacked = {Cradar1, Cradar2, Cradar3, Cradar4}

OnArrayHacked = function(hackedArray)
	NumberOfArrayHacked = NumberOfArrayHacked + 1
	Trigger.ClearAll(hackedArray)

	if NumberOfArrayHacked == 1 then
		HackOneArrayMessage()
		LocalPlayer.MarkCompletedObjective(ObjectiveHackOneArray)
		Nod_AI2.GrantCondition("revealbase")
		-- Pass the squad control to LocalPlayer
		for key,actor in ipairs(Cab_AI.GetGroundAttackers()) do
			actor.Owner = LocalPlayer
		end


	elseif NumberOfArrayHacked == 2 then
		HackTwoArrayMessage()
		CabHacker.GrantCondition("allow-disable")
		PlayerReinforementSpawn({"pdrone", "pdrone", "pdrone",  "cyborg", "cyborg", "cyborg", "cborg", "cborg", "cborg"}, StartPointReinforcePath, WayPoint1743.CenterPosition, nil)

		Trigger.AfterDelay(700, function()
			MercenaryFoundMessage()
			Mut_AI.GrantCondition("revealbase")
			Mut_AI2.GrantCondition("revealbase")
			if not Mound1.IsDead then
				Radar.Ping(LocalPlayer, Mound1.CenterPosition, HSLColor.FromHex("00FF00"))
			end
		end)

	elseif NumberOfArrayHacked == 3 then
		HackThreeArrayMessage()
		Nod_AI.GrantCondition("revealbase")
		Nod_AI.GrantCondition("revealunit")
		Nod_AI2.GrantCondition("revealunit")
		Creep_AI.GrantCondition("revealunit")
		Creep_AI.GrantCondition("revealbase")
		Neutral_AI.GrantCondition("revealbase")
		PlayerReinforementSpawn({"reapercab", "reapercab", "reapercab", "glad", "glad", "glad"}, StartPointReinforcePath, WayPoint1743.CenterPosition, nil)

	elseif NumberOfArrayHacked == 4 then
		LocalPlayer.MarkCompletedObjective(SecondaryObjectiveHackAllArray)
		HackFourArrayMessage()
		-- All Nod mech error
		for key,unit in ipairs(Nod_AI.GetActorsByType("avatar")) do
			if unit.AcceptsCondition("error") then
				GoMechError(unit, LocalPlayer)
			end
		end
		if (not Mechfac1.IsDead) and Mechfac1.Owner == Nod_AI then
			GoMechError(Mechfac1, LocalPlayer)
		end
		-- Reinforement: give commando if LocalPlayer don't have. Put to different tick for perf
		Trigger.AfterDelay(1, function()
			SpawnCybrogCommandoReinforcement()
		end)

		-- Power sabotage
		RemainingSabotogeTime = 4800
		local poweroff_dummy = Actor.Create("poweroffdummy", true, { Owner = Nod_AI})
		Trigger.AfterDelay(RemainingSabotogeTime, function() poweroff_dummy.Destroy() end)

		 -- Trigger AI if LocalPlayer haven't triggered
		if not AwaredByNod then
			AwaredByNod = true
			NodWarnedOnFourHackedMessage()
			Trigger.AfterDelay(300, function()
				Nod_AI.GrantCondition("enable-ai-combat")
				AICaptureMCV()
			end)

			if NodBaseAlertTrigger ~= nil then
				Trigger.RemoveFootprintTrigger(NodBaseAlertTrigger)
				NodBaseAlertTrigger = nil
			end
		end
	end
end

AttackArrayTeam = {Subtank1, Subtank2, Subtank3, Subtank4}
AIAttackArrays = function()
	local arraysNotDead = {}

	for k,actor in ipairs(ArraysNeedHacked) do
		if not actor.IsDead then
			table.insert(arraysNotDead, actor)
		end
	end

	if #arraysNotDead == 0 then
		return
	end

	local index = 1
	for k, attacker in ipairs(AttackArrayTeam) do
		if not attacker.IsDead then
			attacker.Attack(arraysNotDead[index], true, true)
			index = (index % #arraysNotDead) + 1
		end
	end
end

--  ####### Mercenaries Angry
Shopkeepers = {Shopkeeper1, Shopkeeper2, Shopkeeper3, Shopkeeper4, Shopkeeper5, Shopkeeper6, Shopkeeper7, Shopkeeper8,
Shopkeeper9, Shopkeeper10, Shopkeeper11, Shopkeeper12, Shopkeeper13, Shopkeeper14, Shopkeeper15, Shopkeeper16, Shopkeeper17}

EnrageMercenaries = function()
	if IsShopAngry then
		return
	end

	IsShopAngry = true
	MercenaryMessageMadMessage()

	if FindMercenaryTrigger ~= nil then
		Trigger.RemoveProximityTrigger(FindMercenaryTrigger)
		FindMercenaryTrigger = nil
	end

	if TunnelShop ~= nil then
		Trigger.ClearAll(TunnelShop)
		TunnelShop.Owner = Mut_AI2
	end

	if not Mound1.IsDead then
		Mound1.Destroy()
		Actor.Create("mutambushvent", true, {Owner = Mut_AI2, Location = Mound1.Location})
	end

	--Destroy true production dummy and production script
	Trigger.ClearAll(HireDummy)
	HireDummy.Owner = Neutral_AI -- HACK: Actor.Destroy() calls an activity, if actor 's current activity is not cancelled, the Destroy() will wait
	HireDummy.Destroy()

	--Destroy true production dummy and production script
	for key,actor in ipairs(Shopkeepers) do
		if not actor.IsDead then
			actor.Owner = Mut_AI2
			Trigger.ClearAll(actor) 
		end
	end
	
	--Trigger Tunnel Spawning Weapon
	Actor.Create("mutangry.weapon.dummy", true, {Owner = Mut_AI2, Location = HireDummy.Location})
end


-- ///////// Main function //////////--

--  ####### Tick For Timer and Mission Text
Tick = function()
	if RemainingSabotogeTime > 0 then
		RemainingSabotogeTime = RemainingSabotogeTime - 1
		UserInterface.SetMissionText(CurrentMissionText .. "\n" .. UserInterface.Translate("mission-lua-timmer") .. " " .. Utils.FormatTime(RemainingSabotogeTime))
	else
		UserInterface.SetMissionText(CurrentMissionText)
	end
end

--  ####### WorldLoaded and Mission Main

CaptureMCVReinforcePathWater = { Reinforcepoint1.Location, WayPoint1399.Location }
CaptureMCVReinforcePathGround = { Reinforcepoint2.Location, WayPoint1399.Location }

WorldLoaded = function()
	LocalPlayer = Player.GetPlayer("Cabal")
	Nod_AI = Player.GetPlayer("Nod")
	Nod_AI2 = Player.GetPlayer("Armed Civilians")
	Mut_AI = Player.GetPlayer("Mercenaries")
	Mut_AI2 = Player.GetPlayer("Angry Mercenaries")
	Creep_AI = Player.GetPlayer("Creeps")
	Cab_AI = Player.GetPlayer("Cyborg Squad")
	Neutral_AI = Player.GetPlayer("Neutral")

	AwaredByNod = false -- Nod will enable AI and attack LocalPlayer when 'true'

	NumberOfArrayHacked = 0
	RemainingSabotogeTime = 0

	CurrentMissionText = ""

	Objectives()
	MissionMapSetUp()
		
	Trigger.AfterDelay(DateTime.Seconds(5), function()
		IntroductionInfo()
	end)


	-- ##### Secondary Storyline: Hack Clivian Array
	for k,actor in ipairs(ArraysNeedHacked) do
		Trigger.OnInfiltrated(actor, function(a, i) OnArrayHacked(a) end)
	end
	-- ##### End of Secondary Storyline: Hack Clivian Array



	-- ##### Secondary Storyline: Captrue MCV
	--  Player Find the MCV by getting close
	local isFoundMcv = false

	FindMCVTrigger = Trigger.OnEnteredProximityTrigger(WayPoint1399.CenterPosition, WDist.New(1024 * 15), function(a, id)
		if a.Owner == LocalPlayer and FindMCVTrigger ~= nil then
			MCVFoundMessage()
			Neutral_AI.GrantCondition("revealbase")
			isFoundMcv = true

			Trigger.RemoveProximityTrigger(FindMCVTrigger)
			FindMCVTrigger = nil
		end
	end)

	--  Player threats the MCV research, Nod trying to restart MCV
	StealMCVTrigger = Trigger.OnEnteredProximityTrigger(WayPoint1399.CenterPosition, WDist.New(1024 * 6), function(a, id)
		if isFoundMcv and a.Owner == LocalPlayer and a.Type ~= "qdrone" and StealMCVTrigger ~= nil then
			MCVThreatMessage()
			AICaptureMCV()

			Trigger.RemoveProximityTrigger(StealMCVTrigger)
			StealMCVTrigger = nil
		end
	end)

	-- MCV captured.
	-- 1. if LocalPlayer successfully capture the MCV, goes to face to face storyline
	-- 2. if Nod successfully capture the MCV, goes to sneaky and hack storyline
	MCVlostAgain = false

	Trigger.OnCapture(CabConyard, function()
		AISellMCVResearchBases()

		-- if LocalPlayer get the MCV, the Mech1 will be LocalPlayer's, and Mech2 will go error (switch between ally and foe)
		-- if AI get the MCV, the Mech2 will be AI's, and Mech1 will go error (switch between ally and foe)
		if CabConyard.Owner == LocalPlayer then
			LocalPlayer.MarkCompletedObjective(SecondaryObjectiveCaptureMCV)
			MCVSuccessMessage()

			if not Mech1.IsDead then
				Mech1.Owner = LocalPlayer
			end

			-- give reinforement that help player to protect the MCV
			PlayerReinforementSpawn({"cabharv","cabharv", "repairvehicle", "repairvehicle"}, CaptureMCVReinforcePathWater, WayPoint1399.CenterPosition, "cabapc")
			Trigger.AfterDelay(100, function()
				PlayerReinforementSpawn({"limped","limped", "limped", "limped", "limped", "basilisk", "basilisk", "basilisk", "wasp", "wasp", "wasp", "wasp"}, CaptureMCVReinforcePathWater, nil, nil)
			end)
			if Difficulty == "normal" then
				Trigger.AfterDelay(1000, function()
					PlayerReinforementSpawn({"reapercab","reapercab", "reapercab","reapercab", "centurion", "centurion", "centurion", "centurion", "centurion"}, StartPointReinforcePath, MissionStartpoint.CenterPosition, nil)
				end)
				Trigger.AfterDelay(2000, function()
					PlayerReinforementSpawn({"paladin", "paladin", "paladin", "paladin", "paladin", "spiderarty", "spiderarty"}, StartPointReinforcePath, MissionStartpoint.CenterPosition, nil)
				end)
			elseif Difficulty == "hard" then
				Trigger.AfterDelay(1000, function()
					SpawnCybrogCommandoReinforcement()
				end)
				Trigger.AfterDelay(2000, function()
					PlayerReinforementSpawn({"cborg", "glad", "cborg", "glad", "cborg", "glad", "cborg", "glad", "cborg", "glad", "cborg", "glad", "cborg", "glad", "cborg", "glad"}, StartPointReinforcePath, MissionStartpoint.CenterPosition, nil)
				end)
			end
			-- restart mechs
			if not Mech2.IsDead then
				if LocalPlayer.IsObjectiveCompleted(SecondaryObjectiveHackAllArray) then
					Mech2.Owner = LocalPlayer -- when LocalPlayer finish hacking all Array, Nod mech can start without error by LocalPlayer
				else
					Mech2.GrantCondition("empdisable")
				end
			end

			 -- Trigger Nod AI if LocalPlayer haven't triggered
			if not AwaredByNod then
				AwaredByNod = true
				NodWarnedOnMCVMessage()
				Trigger.AfterDelay(400, function()
					AIAttackArrays()
				end)
				Trigger.AfterDelay(500, function()
					Nod_AI.GrantCondition("enable-ai-combat")
				end)

				if NodBaseAlertTrigger ~= nil then
					Trigger.RemoveFootprintTrigger(NodBaseAlertTrigger)
					NodBaseAlertTrigger = nil
				end
			end


		elseif CabConyard.Owner == Nod_AI then
			-- Considering LocalPlayer can take back the MCV later, so we don't mark it fail yet
			MCVlostAgain = true
			MCVFailedMessage()
			PlayerReinforementSpawn({"limped","limped", "limped", "limped", "limped"}, StartPointReinforcePath, WayPoint1743.CenterPosition, nil)

			if not Mech2.IsDead then
				if LocalPlayer.IsObjectiveCompleted(SecondaryObjectiveHackAllArray) then
					GoMechError(Mech2, Nod_AI) -- when LocalPlayer finish hacking all Array, All of Avatars will be mad
				else
					Mech2.Owner = Nod_AI
				end
			end
			if not Mech1.IsDead then
				Mech1.GrantCondition("empdisable")
			end


		end

		if FindMCVTrigger ~= nil then
			Trigger.RemoveProximityTrigger(FindMCVTrigger)
			FindMCVTrigger = nil
		end
		if StealMCVTrigger ~= nil then
			Trigger.RemoveProximityTrigger(StealMCVTrigger)
			StealMCVTrigger = nil
		end
		Trigger.ClearAll(CabConyard)
	end)

	-- MCV killed before captured.
	Trigger.OnKilled(CabConyard, function(self, killer)
		if FindMCVTrigger ~= nil then
			Trigger.RemoveProximityTrigger(FindMCVTrigger)
			FindMCVTrigger = nil
		end
		if StealMCVTrigger ~= nil then
			Trigger.RemoveProximityTrigger(StealMCVTrigger)
			StealMCVTrigger = nil
		end
		Trigger.ClearAll(CabConyard)

		if not Mech1.IsDead then
			Mech1.GrantCondition("empdisable")
		end
		if not Mech2.IsDead then
			Mech2.GrantCondition("empdisable")
		end

		AISellMCVResearchBases()
		LocalPlayer.MarkFailedObjective(SecondaryObjectiveCaptureMCV)

		MCVFailedMessage()
	end)
	-- ##### End of Secondary Storyline: Captrue MCV




	-- ##### Shopkeeper: Mercenaries

	TunnelShop = nil
	HireDummy = Actor.Create("hire.production.dummy", true, {Owner = Neutral_AI, Location = Mound1.Location})
	IsShopAngry = false

	--- fake produce property:
	local production_facing = Angle.New(640)
	local production_woffset = WVec.New(0, 768, 0)
	local production_coffset = CVec.New(2,1)
	local rallypoint1_offset = CVec.New(4,1)
	local rallypoint2_offset = CVec.New(4,4)

	HireMercenarys = {
			["vhire.lynx.dummy"] = "lynx",
			["vhire.quad.dummy"] = "mutquad",
			["vhire.struck.dummy"] = "struck",
			["ihire.mar.dummy"] = "marauder",
			["ihire.fiend.dummy"] = "mutfiend",
			["ihire.e3.dummy"] = "e3",
	}

	Trigger.AfterDelay(700, function() -- give enough time to load shoopkeepers to initial the tunnel
		Tunnel1.Destroy()
		Tunnel2.Destroy()
	end)

	FindMercenaryTrigger = Trigger.OnEnteredProximityTrigger(Mound1.CenterPosition, WDist.New(1024 * 4), function(a, id)
		if IsShopAngry then
			if FindMercenaryTrigger ~= nil then
				Trigger.RemoveProximityTrigger(FindMercenaryTrigger)
				FindMercenaryTrigger = nil
			end
			return
		end

		if a.Owner == LocalPlayer and a.Type ~= "qdrone" and FindMercenaryTrigger ~= nil then
			Trigger.RemoveProximityTrigger(FindMercenaryTrigger)
			FindMercenaryTrigger = nil

			Mound1.Destroy()
			Media.PlaySound("ssneakat.wav")

			TunnelShop = Actor.Create("mutventshop", true, {Owner = Mut_AI, Location = Mound1.Location})
			Trigger.OnKilled(TunnelShop, function(a, k)
				EnrageMercenaries()
			end)

			MeetMercenaryMessage()

			-- Enable production
			Trigger.AfterDelay(620, function()
				if IsShopAngry or TunnelShop == nil or TunnelShop.IsDead or HireDummy.IsDead then
					return
				end

				TunnelShop.Flash(HSLColor.FromHex("FFFFFF"), 15, DateTime.Seconds(1) / 4)

				-- Enable the true production dummy
				HireDummy.Owner = LocalPlayer
				Trigger.OnProduction(HireDummy, function(producer, product)
					if producer == nil or producer.IsDead then
						return
					end

					Media.FloatingText(string.format("$%d",Actor.Cost(product.Type)), producer.CenterPosition ,30, HSLColor.FromHex("0CBB01") )

					-- HACK HACK HACK: see how Production.cs init an Actor
					local name = HireMercenarys[product.Type]
					if name == nil then
						return
					end
					local unit = Actor.Create(name, true, { Owner = LocalPlayer,Facing = production_facing, Location = producer.Location + production_coffset, CenterPosition = producer.CenterPosition + production_woffset })
					if unit ~= nil then
						unit.MoveIntoWorld(producer.Location + rallypoint1_offset)
						unit.Move(producer.Location + rallypoint2_offset)
					end
				end)
			end)
		end
	end)

	Trigger.OnAnyKilled(Shopkeepers, function(a)
		EnrageMercenaries()
	end)
	-- ##### End of Shopkeeper: Mercenaries


	-- ###### If LocalPlayer enters the main base of Nod, Nod will enable AI
	local alerts = {Nodbase_alert1, Nodbase_alert2, Nodbase_alert3, Nodbase_alert4, Nodbase_alert5, Nodbase_alert6, Nodbase_alert7,
	Nodbase_alert8, Nodbase_alert9, Nodbase_alert10, Nodbase_alert11, Nodbase_alert12, Nodbase_alert13, Nodbase_alert14, Nodbase_alert15,
	Nodbase_alert16, Nodbase_alert17, Nodbase_alert18, Nodbase_alert19, Nodbase_alert20, Nodbase_alert21, Nodbase_alert22, Nodbase_alert23,
	Nodbase_alert24, Nodbase_alert25, Nodbase_alert26, Nodbase_alert27, Nodbase_alert28, Nodbase_alert29, Nodbase_alert30, Nodbase_alert31,
	Nodbase_alert32, Nodbase_alert33, Nodbase_alert34, Nodbase_alert35,Nodbase_alert36, Nodbase_alert37, Nodbase_alert38}

	local alert_footprints = {}

	for k,v in ipairs(alerts) do
		table.insert(alert_footprints, v.Location)
		v.Destroy()
	end
	alerts = nil

	NodBaseAlertTrigger = Trigger.OnEnteredFootprint(alert_footprints, function(a, id)
		if a.Owner == LocalPlayer then
			if not AwaredByNod then
				AwaredByNod = true
				NodAlertedMessage()
				Nod_AI.GrantCondition("enable-ai-combat")
			end

			Trigger.RemoveFootprintTrigger(NodBaseAlertTrigger)
			NodBaseAlertTrigger = nil
		end
	end)


	-- ###### Player find the location of alien stuff
	ScrinRepFoundTrigger = Trigger.OnEnteredProximityTrigger(ScrinRep.CenterPosition, WDist.New(1024 * 15), function(a, id)
		if a.Owner == LocalPlayer and ScrinRepFoundTrigger ~= nil then
			Trigger.RemoveProximityTrigger(ScrinRepFoundTrigger)
			ScrinRepFoundTrigger = nil

			Actor.Create("camera", true, { Owner = LocalPlayer, Location = ScrinRep.Location })

			if not ScrinRep.IsDead then
				ScrinRep.Flash(HSLColor.FromHex("FFFFFF"), 20, DateTime.Seconds(1) / 4)
			end
			ReplicatorFoundMessage()
		end
	end)

	-- ###### Player capture the alien stuff, gg
	Trigger.OnCapture(ScrinRep, function()
		MissionCompleteMessage()
		CheckObjectivesOnMissionEnd(true)
	end)

	-- ###### Player fail the mission
	Trigger.OnKilled(ScrinRep, function(self, killer)
		CheckObjectivesOnMissionEnd(false)
	end)

	Trigger.OnKilled(CabHacker, function(self, killer)
		CheckObjectivesOnMissionEnd(false)
	end)
end
