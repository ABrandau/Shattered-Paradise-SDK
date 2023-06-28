--[[
   Copyright The OpenRA Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

Difficulty = Map.LobbyOption("difficulty")

Objectives = function()
	SecondaryObjectiveCaptureMCV = player.AddSecondaryObjective("Recapture our lost M.C.V.")
	SecondaryObjectiveHackAllArray = player.AddSecondaryObjective("Hack all the Civilian Array")
	ObjectiveHackOneArray = player.AddPrimaryObjective("Hack One Civilian Array.")
	ObjectiveFindAlien = player.AddPrimaryObjective("Identify the source of the unknown signal.")
	ObjectiveCaptureAlien = player.AddPrimaryObjective("Secure the source of the unknown signal.")
	ObjectiveProtectHacker = player.AddPrimaryObjective("Hacker Drone must survive.")
end

-- ####### Mission Map Set up
MissionMapSetUp = function()
	SpawnPatrollers()
	SpawnUpgrade()
	Camera.Position = startpoint.CenterPosition
	hacker.Patrol({Actor1662.Location, Actor1742.Location, Actor1743.Location}, false)
	Trigger.AfterDelay(DateTime.Seconds(12), function()
		hacker.Owner = player
	end)

	if Difficulty == "hard" then

		for key,unit in ipairs(ally.GetActorsByType("moth")) do
			unit.Destroy()
		end

	elseif Difficulty == "normal" then

		AISell(nodob1)
		AISell(nodob2)
		AISell(nodob3)
		AISell(nodob4)
		Engineer4.Destroy()
		Engineer5.Destroy()

		for key,unit in ipairs(nod_ai2.GetActorsByType("tdadvgtwr")) do
			unit.Destroy()
		end

		for key,unit in ipairs(nod_ai.GetActorsByType("scrin")) do
			unit.Destroy()
		end

		nodmcv1.Destroy()
	end
end

-- ####### End game check
CheckObjectivesOnMissionEnd = function(success)
	-- check the SecondaryObjective first
	-- 1. check if player has mcv or conyard at the end of the game
	if not player.IsObjectiveCompleted(SecondaryObjectiveHackAllArray) then
		player.MarkFailedObjective(SecondaryObjectiveHackAllArray)
	end

	-- 2. check if player has mcv or conyard at the end of the game
	if not player.IsObjectiveCompleted(SecondaryObjectiveCaptureMCV) then
		for key,unit in ipairs(player.GetActorsByType("cabmcv")) do
			player.MarkCompletedObjective(SecondaryObjectiveCaptureMCV)
			break
		end

		for key,unit in ipairs(player.GetActorsByType("cabyard")) do
			player.MarkCompletedObjective(SecondaryObjectiveCaptureMCV)
			break
		end

		if not player.IsObjectiveCompleted(SecondaryObjectiveCaptureMCV) then
			player.MarkFailedObjective(SecondaryObjectiveCaptureMCV)
		end
	end

	if success then
		player.MarkCompletedObjective(ObjectiveCaptureAlien)
	else
		player.MarkFailedObjective(ObjectiveCaptureAlien)
	end

	if not player.IsObjectiveCompleted(ObjectiveHackOneArray) then
		player.MarkFailedObjective(ObjectiveHackOneArray)
	end

	if not player.IsObjectiveCompleted(ObjectiveFindAlien) then
		player.MarkFailedObjective(ObjectiveFindAlien)
	end
	
	if hacker.IsDead then
		player.MarkFailedObjective(ObjectiveProtectHacker)
	else
		player.MarkCompletedObjective(ObjectiveProtectHacker)
	end

end

-- ####### information
IntroductionInfo = function()
	CurrentMissionText = "Use the Hacker Drone to hack the Civilian Array."
	Notification("A cyborg squad and a Hacker Drone will aid you in this mission. We have to use smaller squad to hide our purpose at the beginning.")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
			Notification("Use the Hacker Drone to hack the Civilian Array for local intelligence. Hacker Drone is the key to this mission, protect it.")
			if not hacker.IsDead then
				hacker.Flash(HSLColor.FromHex("FFFFFF"), 15, DateTime.Seconds(1) / 4)
			end
			if not cradar3.IsDead then
				cradar3.Flash(HSLColor.FromHex("FFFFFF"), 15, DateTime.Seconds(1) / 4)
			end
	end)
	Trigger.AfterDelay(DateTime.Seconds(14), function()
			Tip("How to use Hacker Drone:\n1.Target hackable (such as Civilian Array) to send its Quadrotor to hack (Range: 20 cells).\n2.Target capturable building to capture the building by itself.\n3.Force fire at location to send its Quadrotor to recon (Range: 20 cells). Can detect cloaked")
	end)
end

MCVFoundMessage = function()
	Notification("M.C.V. located, we find our 'Minotaur' and a Nod's mech prototype. We can restart both mech once the M.C.V. is captured.")
	cabconyard1.Flash(HSLColor.FromHex("FFFFFF"), 20, DateTime.Seconds(1) / 4)

	  -- Warning before Nod AI triggered
	if not player.IsObjectiveCompleted(SecondaryObjectiveHackAllArray) then
		Trigger.AfterDelay(DateTime.Seconds(7), function()
			Warning("Successfully capture the MCV will start the direct conflict between Nod! Be prepared!")
		end)
		Trigger.AfterDelay(DateTime.Seconds(14), function()
			Tip("Explore the map more before you recapture the MCV. You needs to find somewhere close to resource and easy to defend.")
		end)
	end
end

MCVThreatMessage = function()
	Notification("Nod has noticed our attempt and tries to restart the MCV!")
end

NodWarnedOnMCVMessage = function()
	Notification("Nod's mech prototype restart failed. After restart, it will lose control after a while. Destroy it now or move it away from our squad.")
	Trigger.AfterDelay(DateTime.Seconds(11), function()
			Warning("Nod has been awared of the activation of our MCV and our hacking to Civilian Arrays, in the intercepted message.")
	end)
	Trigger.AfterDelay(DateTime.Seconds(18), function()
			Warning("Be prepared for a face to face combat. \nNod plans to destroy unhacked Civilian Arrays to prevent further security codes leaking.")
	end)
end

MCVFailedMessage = function()
	Notification("The M.C.V. is lost again, but it is not over")

	if not player.IsObjectiveCompleted(SecondaryObjectiveHackAllArray) then
		Trigger.AfterDelay(DateTime.Seconds(15), function()
				Notification("Nod believes our failure is inevitable and no longer pay attention to us, in the intercepted message.")
		end)
		Trigger.AfterDelay(DateTime.Seconds(22), function()
				Notification("It is an oppotunity, let us hack remaining Civilian Arrays and try a cyber attack.")
		end)
	end
end

HackOneArrayMessage = function()
	Notification("We can now intercept some of their communication and 3 other Civilian Arrays can be located.")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
		Notification("It is confirmed that the unknow signal is from an alien origin. Use our cyborg squad to find it.")
	end)
	Trigger.AfterDelay(DateTime.Seconds(14), function()
		Notification("There are two enemies: Nod and their local Armed Civilians. \nThe Armed Civilians has a rather old armory, should be easier to deal with.")
	end)

	CurrentMissionText = "An artifact of alien origin has been detected nearby, explore the area."
end

HackTwoArrayMessage = function()
	Notification("Hacker Drone can now disable the control system of enemy defence and vehicle by hacking.")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
		Tip("Hacker Drone can now use its Quadrotor to disable enemy's defence and vehicle!")
	end)
end

HackThreeArrayMessage = function()
	Notification("With more accessibility, we get some information of Nod's research of our MCV. Nod steals our technology for their 'Avatar' project.")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
		Notification("We have revealed all the 'Avatar' project related by hacking. We will launch a cyber attack when the final Civilian Array is hacked")
	end)
		-- Warning before Nod AI triggered
	if not player.IsObjectiveCompleted(SecondaryObjectiveCaptureMCV) then
		Trigger.AfterDelay(DateTime.Seconds(14), function()
			Warning("Successfully hack into the final Civilian Array will start the direct conflict between Nod! Be prepared!")
		end)
	end
end

HackFourArrayMessage = function()
	Notification("Our cyber attack will \n1.Make 'Avatar' project lose control. \n2.Sabotage Nod's power system for a short period.")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
		Notification("Take the chance to destroy Nod's base and find that alien obejct.")
	end)
end

NodWarnedOnFourHackedMessage = function()
	Trigger.AfterDelay(DateTime.Seconds(14), function()
		Warning("Nod has been awared of our hacking and consider us a major threat, in the intercepted message.")
	end)
	Trigger.AfterDelay(DateTime.Seconds(21), function()
		Warning("Be prepared for a face to face combat. \nNod are likely to restart our lost MCV to against us.")
	end)
end

ReplicatorFoundMessage = function()
	Notification("It seems this alien structure is the source of the unkown signals, capture it.")
	CurrentMissionText = "Capture the alien artifact."
	player.MarkCompletedObjective(ObjectiveFindAlien)
end

MissionCompleteMessage = function()
	CurrentMissionText = "Mission Complete."
	player.MarkCompletedObjective(ObjectiveCaptureAlien)
end

MeetMercenaryMessage = function()
	if IsShopAngry then
		return
	end

	ShopSays("Warlord","Well well well, bros and gals, look what do we have here! I never expect those cyborgs can walk this far.")
	Trigger.AfterDelay(DateTime.Seconds(7), function()
		if IsShopAngry then
			return
		end
		if player.IsObjectiveFailed(SecondaryObjectiveCaptureMCV) then
			ShopSays("Soldier","Yeah, I just saw those idiots \"PERFECTLY\" executed their own MCV, plz allow me LMAO for a second.")
		elseif MCVlostAgain then
			ShopSays("Soldier","Yeah, I just saw Brotherhood Of Nerds got that MCV again, the timming was perfect LOL.")
		elseif  not player.IsObjectiveCompleted(SecondaryObjectiveCaptureMCV) then
			ShopSays("Soldier","Feh, glad that they cannot do s**t here. Brotherhood Of Nerds still has their MCV.")
		else
			ShopSays("Soldier","(thumbs safety catch) Feh, I guess they can walk no further if their legs are broken.")
		end
	end)
	Trigger.AfterDelay(DateTime.Seconds(14), function()
		if IsShopAngry then
			return
		end
		ShopSays("Warlord", "Cool it, boys. Anyway, cyborgs, we are businessmen, and I am sure CABAL taught you how to deal.")
	end)
	Trigger.AfterDelay(DateTime.Seconds(21), function()
		if IsShopAngry then
			return
		end
		ShopSays("Warlord", "If you want, we can help you, for a price, in FULL PAYMENT and no refund.")
	end)
	Trigger.AfterDelay(DateTime.Seconds(28), function()
		if IsShopAngry then
			return
		end
		ShopSays("Warlord", "And always remember, don't mess up with us, kiddo.")
	end)
	Trigger.AfterDelay(DateTime.Seconds(35), function()
		if IsShopAngry then
			return
		end
		Tip("You can deal with Mercenaries by moving your unit to the Black Market, then build unit on the 'Infantry' production tab. \nKill/Destroy their property will make them mad!")
	end)
end

MercenaryMessageMadMessage = function()
	ShopSays("Warlord","Enough! KILL EVERYTHING THAT IS NOT MUTANT!")
end

Notification = function(text)
	Media.DisplayMessage(text, "C.A.B.A.L", HSLColor.FromHex("1E90FF"))
end

Tip = function(text)
	Media.DisplayMessage(text, "Tip", HSLColor.FromHex("29F3CF"))
end

Warning = function(text)
	Media.DisplayMessage(text, "Warning", HSLColor.FromHex("FF1111"))
end

ShopSays = function(who, text)
	Media.DisplayMessage(text, who, HSLColor.FromHex("0CBB01"))
end





-- ####### Upgrade
SpawnUpgrade = function()
	Actor.Create("upgrade.tib_core_missiles", true, { Owner = nod_ai })

	Actor.Create("upgrade.lynx_rockets", true, { Owner = mercenary_ai })
	Actor.Create("upgrade.lynx_rockets", true, { Owner = mercenary_ai2 })
	Actor.Create("upgrade.lynx_rockets", true, { Owner = player })
	Actor.Create("upgrade.fortified_upg", true, { Owner = mercenary_ai })
	Actor.Create("upgrade.fortified_upg", true, { Owner = mercenary_ai2 })
	Actor.Create("upgrade.fortified_upg", true, { Owner = player })
	Actor.Create("upgrade.tunnel_repairs", true, { Owner = mercenary_ai })
	Actor.Create("upgrade.tunnel_repairs", true, { Owner = mercenary_ai2 })
	Actor.Create("upgrade.tiberium_infusion", true, { Owner = mercenary_ai })
	Actor.Create("upgrade.tiberium_infusion", true, { Owner = mercenary_ai2 })
	Actor.Create("upgrade.tiberium_infusion", true, { Owner = player })
	Actor.Create("upgrade.tiberium_gas_warheads", true, { Owner = mercenary_ai })
	Actor.Create("upgrade.tiberium_gas_warheads", true, { Owner = mercenary_ai2 })
	Actor.Create("upgrade.tiberium_gas_warheads", true, { Owner = player })
end

-- ######## Reinforement Spawn
PlayerReinforementSpawn = function(units, path, pingwhere, transport)
	if (transport ~= nil) then
		Reinforcements.ReinforceWithTransport(player, transport, units, path)
	else
		Reinforcements.Reinforce(player, units, path)
	end

	Media.PlaySpeechNotification(player, "ReinforcementsArrived")

	if (pingwhere ~= nil) then
		Radar.Ping(player, pingwhere, HSLColor.FromHex("00FF00"))
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

ScanvegerAPath = { Actor1399.Location, Actor1208.Location }
ScanvegerBPath = { Actor1208.Location, Actor1399.Location }
ScanvegerCPath = { Actor1544.Location, Actor1404.Location }
ScanvegerDPath = { Actor1404.Location, Actor1544.Location }
ScanvegerEPath = { Actor1400.Location, Actor1640.Location }
ScanvegerFPath = { Actor1640.Location, Actor1400.Location }

TanksPath1 = {Actor1346.Location, Actor1210.Location}
TanksPath2 = {Actor1346.Location, Actor1396.Location}
BikePath1 = { Actor1540.Location, Actor1207.Location }
BikePath2 = { Actor1540.Location, Actor1738.Location }

SpawnPatrollers = function()
		local ScanvegerA = Actor.Create("truckb", true, { Owner = nod_ai2, Location = ScanvegerAPath[2]})
		local ScanvegerB = Actor.Create("truckb", true, { Owner = nod_ai2, Location = ScanvegerBPath[2]})
		local ScanvegerC = Actor.Create("truckb", true, { Owner = nod_ai2, Location = ScanvegerCPath[2]})
		local ScanvegerD = Actor.Create("truckb", true, { Owner = nod_ai2, Location = ScanvegerDPath[2]})
		local ScanvegerE = Actor.Create("truckb", true, { Owner = nod_ai2, Location = ScanvegerEPath[2]})
		local ScanvegerF = Actor.Create("truckb", true, { Owner = nod_ai2, Location = ScanvegerFPath[2]})
		Patrol2A(ScanvegerA, ScanvegerAPath, 600)
		Patrol2A(ScanvegerB, ScanvegerBPath, 600)
		Patrol2A(ScanvegerC, ScanvegerCPath, 2000)
		Patrol2A(ScanvegerD, ScanvegerDPath, 2000)
		Patrol2A(ScanvegerE, ScanvegerEPath, 1900)
		Patrol2A(ScanvegerF, ScanvegerFPath, 1900)

		Patrol2A(ctnk1, TanksPath1, 900)
		Patrol2A(ctnk2, TanksPath1, 900)
		Patrol2A(ctnk3, TanksPath1, 900)
		Patrol2A(ctnk4, TanksPath2, 1300)
		Patrol2A(ctnk5, TanksPath2, 1300)
		Patrol2A(ctnk6, TanksPath2, 1300)

		Patrol2A(bike1, BikePath1, 600)
		Patrol2A(bike2, BikePath2, 600)
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

	if unit.Owner == nod_ai then
		unit.Owner = creep
		Trigger.AfterDelay(200, function()
			MechError(unit)
		end)
	elseif unit.Owner == creep then
		unit.Owner = player
		Trigger.AfterDelay(200, function()
			MechError(unit)
		end)
	elseif unit.Owner == player then
		unit.Owner = nod_ai
		Trigger.AfterDelay(200, function()
			MechError(unit)
		end)
	end
end

-- ####### AI sell actors
AISell = function(unit)
	if (not unit.IsDead) and unit.Owner ~= player then
		unit.Sell()
	end
end

AISellMCVResearchBases = function()
	AISell(labpowerplant1)
	AISell(labpowerplant2)
	AISell(labpowerplant3)
	AISell(labpowerplant4)
	AISell(nodlab)
	AISell(nodgate2)
	AISell(nodgate1)
	AISell(nodgate3)
	AISell(nodgate4)
	AISell(nodgate5)
	AISell(laser1)
	AISell(laser2)
	AISell(laser3)
	AISell(laser4)
	AISell(laser5)
	AISell(nodob1)
	AISell(nodob2)
	AISell(nodob3)
	AISell(nodob4)
	AISell(nodsam)
end

-- ####### AI capture actor
AICapture = function(unit, target)
	if not (unit.IsDead or target.IsDead) then
		unit.Capture(target)
	end
end

AICaptureMCV = function()
	AICapture(Engineer1,cabconyard1)
	AICapture(Engineer2,cabconyard1)
	AICapture(Engineer3,cabconyard1)
	AICapture(Engineer4,cabconyard1)
	AICapture(Engineer5,cabconyard1)
	AICapture(Engineer6,cabconyard1)
	AICapture(Engineer7,cabconyard1)
	AICapture(Engineer8,cabconyard1)
	AICapture(Engineer9,cabconyard1)
	AICapture(Engineer10,cabconyard1)
	AICapture(Engineer11,cabconyard1)
	AICapture(Engineer12,cabconyard1)
	AICapture(Engineer13,cabconyard1)
end

-- ####### Hack Array stroy line
HackArrayReinforcePath = { reinforcepoint2.Location, Actor1440.Location, Actor1662.Location, Actor1742.Location, Actor1743.Location }
ArraysHacked = {[cradar1] = false, [cradar2] = false, [cradar3] = false, [cradar4] = false}

OnArrayHacked = function(hackedArray)

	NumberOfArrayHacked = NumberOfArrayHacked + 1

	if NumberOfArrayHacked == 1 then
		HackOneArrayMessage()
		player.MarkCompletedObjective(ObjectiveHackOneArray)
		nod_ai2.GrantCondition("revealbase")
		-- Pass the squad control to player
		for key,actor in ipairs(ally.GetGroundAttackers()) do
			actor.Owner = player
		end

	elseif NumberOfArrayHacked == 2 then
		HackTwoArrayMessage()
		hacker.GrantCondition("allow-disable")
		PlayerReinforementSpawn({"pdrone", "pdrone", "pdrone",  "cyborg", "cyborg", "cyborg", "cborg", "cborg", "cborg"}, HackArrayReinforcePath, Actor1743.CenterPosition, nil)


	elseif NumberOfArrayHacked == 3 then
		HackThreeArrayMessage()
		nod_ai.GrantCondition("revealbase")
		nod_ai.GrantCondition("revealunit")
		nod_ai2.GrantCondition("revealunit")
		creep.GrantCondition("revealunit")
		creep.GrantCondition("revealbase")
		neutral.GrantCondition("revealbase")
		PlayerReinforementSpawn({"reapercab", "reapercab", "reapercab", "glad", "glad", "glad"}, HackArrayReinforcePath, Actor1743.CenterPosition, nil)

	elseif NumberOfArrayHacked == 4 then
		player.MarkCompletedObjective(SecondaryObjectiveHackAllArray)
		HackFourArrayMessage()
		-- All Nod mech error
		for key,unit in ipairs(nod_ai.GetActorsByType("avatar")) do
			if unit.AcceptsCondition("error") then
				GoMechError(unit, player)
			end
		end
		if (not mechfac.IsDead) and mechfac.Owner == nod_ai then
			GoMechError(mechfac, player)
		end
		-- Reinforement: give commando if player don't have. Put to different tick for perf
		Trigger.AfterDelay(1, function()
			local haveCommando = false
			for key,unit in ipairs(player.GetActorsByType("cyc2")) do
				haveCommando = true
				break
			end

			if haveCommando then
				PlayerReinforementSpawn({"moth", "moth", "moth", "moth", "reapercab", "reapercab", "cborg", "cborg", "cborg", "glad", "glad", "glad"}, HackArrayReinforcePath, Actor1743.CenterPosition, nil)
			else
				PlayerReinforementSpawn({"cyc2", "moth", "moth", "moth", "moth", "reapercab", "reapercab", "cborg", "cborg"}, HackArrayReinforcePath, Actor1743.CenterPosition, nil)
			end
		end)

		-- Power sabotage
		RemainingSabotogeTime = 4800
		local poweroff_dummy = Actor.Create("poweroffdummy", true, { Owner = nod_ai})
		Trigger.AfterDelay(RemainingSabotogeTime, function() poweroff_dummy.Destroy() end)

		 -- Trigger AI if player haven't triggered
		if not player.IsObjectiveCompleted(SecondaryObjectiveCaptureMCV) then
			NodWarnedOnFourHackedMessage()
			Trigger.AfterDelay(300, function()
				nod_ai.GrantCondition("enable-ai-combat")
				AICaptureMCV()
			end)
		end
	end
	
	Trigger.ClearAll(hackedArray)
	ArraysHacked[hackedArray] = hackedArray
end

AttackArrayTeam = {subtank1, subtank2, subtank3, subtank4}
AIAttackUnhackedArray = function()
	local unhacked_arrays = {}

	for actor,hacked in pairs(ArraysHacked) do
		if not (actor.IsDead or hacked) then
			table.insert(unhacked_arrays, actor)
		end
	end

	if #unhacked_arrays == 0 then
		return
	end

	local index = 1
	for key, attacker in ipairs(AttackArrayTeam) do
		if not attacker.IsDead then
			attacker.Attack(unhacked_arrays[index], true, true)
			index = (index % #unhacked_arrays) + 1
		end
	end
end

--  ####### Mercenaries Angry
Shopkeepers = {shopkeeper1, shopkeeper2, shopkeeper3, shopkeeper4, shopkeeper5, shopkeeper6, shopkeeper7, shopkeeper8,
shopkeeper9, shopkeeper10, shopkeeper11, shopkeeper12, shopkeeper13, shopkeeper14, shopkeeper15, shopkeeper16}

EnrageMercenaries = function()
	if IsShopAngry then
		return
	end

	IsShopAngry = true
	MercenaryMessageMadMessage()

	if pid3 ~= nil then
		Trigger.RemoveProximityTrigger(pid3)
		pid3 = nil
	end

	if TunnelShop ~= nil then
		Trigger.ClearAll(TunnelShop)
		TunnelShop.Owner = mercenary_ai2
	end

	if not mound1.IsDead then
		mound1.Destroy()
		Actor.Create("mutambushvent", true, {Owner = mercenary_ai2, Location = mound1.Location})
	end

	--Destroy true production dummy and production script
	Trigger.ClearAll(HireDummy)
	HireDummy.Owner = neutral -- HACK: Actor.Destroy() calls an activity, if actor 's current activity is not cancelled, the Destroy() will wait
	HireDummy.Destroy()

	--Destroy true production dummy and production script
	for key,actor in ipairs(Shopkeepers) do
		if not actor.IsDead then
			actor.Owner = mercenary_ai2
			Trigger.ClearAll(actor) 
		end
	end
	
	--Trigger Tunnel Spawning Weapon
	Actor.Create("mutangry.weapon.dummy", true, {Owner = mercenary_ai2, Location = HireDummy.Location})
end


-- ///////// Main function //////////--

--  ####### Tick For Timer and Mission Text
Tick = function()
	if RemainingSabotogeTime > 0 then
		RemainingSabotogeTime = RemainingSabotogeTime - 1
		UserInterface.SetMissionText(CurrentMissionText .. "\nNod regains power in: " .. Utils.FormatTime(RemainingSabotogeTime))
	else
		UserInterface.SetMissionText(CurrentMissionText)
	end
end

--  ####### WorldLoaded and Mission Main

CaptureMCVReinforcePathWater = { reinforcepoint1.Location, Actor1399.Location }
CaptureMCVReinforcePathGround = { reinforcepoint2.Location, Actor1399.Location }

WorldLoaded = function()
	player = Player.GetPlayer("Cabal")
	nod_ai = Player.GetPlayer("Nod")
	nod_ai2 = Player.GetPlayer("Armed Civilians")
	mercenary_ai = Player.GetPlayer("Mercenaries")
	mercenary_ai2 = Player.GetPlayer("Angry Mercenaries")
	creep = Player.GetPlayer("Creeps")
	ally = Player.GetPlayer("Cyborg Squad")
	neutral = Player.GetPlayer("Neutral")

	NumberOfArrayHacked = 0
	RemainingSabotogeTime = 0

	CurrentMissionText = ""

	Objectives()
	MissionMapSetUp()
		
	Trigger.AfterDelay(DateTime.Seconds(5), function()
		IntroductionInfo()
	end)


	-- ##### Secondary Storyline: Hack Clivian Array
	for actor,stat in pairs(ArraysHacked) do
		Trigger.OnInfiltrated(actor, function(a, i) OnArrayHacked(a) end)
	end
	-- ##### End of Secondary Storyline: Hack Clivian Array



	-- ##### Secondary Storyline: Captrue MCV
	--  Player Find the MCV by getting close
	local isFoundMcv = false

	pid1 = Trigger.OnEnteredProximityTrigger(Actor1399.CenterPosition, WDist.New(1024 * 15), function(a, id)
		if a.Owner == player then
			MCVFoundMessage()
			neutral.GrantCondition("revealbase")
			isFoundMcv = true

			Trigger.RemoveProximityTrigger(id)
			pid1 = nil
		end
	end)

	--  Player threats the MCV research, Nod trying to restart MCV
	pid2 = Trigger.OnEnteredProximityTrigger(Actor1399.CenterPosition, WDist.New(1024 * 6), function(a, id)
		if isFoundMcv and a.Owner == player and a.Type ~= "qdrone" then
			MCVThreatMessage()
			AICaptureMCV()

			Trigger.RemoveProximityTrigger(id)
			pid2 = nil
		end
	end)

	-- MCV captured.
	-- 1. if player successfully capture the MCV, goes to face to face storyline
	-- 2. if Nod successfully capture the MCV, goes to sneaky and hack storyline
	MCVlostAgain = false

	Trigger.OnCapture(cabconyard1, function()
		AISellMCVResearchBases()

		-- if player get the MCV, the mech1 will be player's, and mech2 will go error (switch between ally and foe)
		-- if AI get the MCV, the mech2 will be AI's, and mech1 will go error (switch between ally and foe)
		if cabconyard1.Owner == player then
			player.MarkCompletedObjective(SecondaryObjectiveCaptureMCV)

			if not mech1.IsDead then
				mech1.Owner = player
			end

			-- give proper reinforement
			PlayerReinforementSpawn({"cabharv","cabharv", "repairvehicle", "repairvehicle"}, CaptureMCVReinforcePathWater, Actor1399.CenterPosition, "cabapc")
			Trigger.AfterDelay(100, function()
				PlayerReinforementSpawn({"limped","limped", "limped", "limped", "limped", "basilisk", "basilisk", "basilisk", "wasp", "wasp", "wasp", "wasp"}, CaptureMCVReinforcePathWater, nil, nil)
			end)

			-- restart mechs
			if player.IsObjectiveCompleted(SecondaryObjectiveHackAllArray) then
				mech2.Owner = player -- when player finish hacking all Array, Nod mech can start without error by player
			else
				GoMechError(mech2, player)
			end

			if not player.IsObjectiveCompleted(SecondaryObjectiveHackAllArray) then -- Trigger AI if player haven't triggered
				NodWarnedOnMCVMessage()
				Trigger.AfterDelay(400, function()
					AIAttackUnhackedArray()
				end)
				Trigger.AfterDelay(500, function()
					nod_ai.GrantCondition("enable-ai-combat")
				end)
			end


		elseif cabconyard1.Owner == nod_ai then
			-- Considering player can take back the MCV later, so we don't mark it fail yet
			MCVlostAgain = true

			PlayerReinforementSpawn({"limped","limped", "limped", "limped", "limped"}, HackArrayReinforcePath, Actor1743.CenterPosition, nil)

			if not mech2.IsDead then
				if player.IsObjectiveCompleted(SecondaryObjectiveHackAllArray) then
					GoMechError(mech2, nod_ai) -- when player finish hacking all Array, Nod cannot start their mech normally
				else
					mech2.Owner = nod_ai
				end
			end
			GoMechError(mech1, nod_ai)

			MCVFailedMessage()
		end

		if pid1 ~= nil then
			Trigger.RemoveProximityTrigger(pid1)
			pid1 = nil
		end
		if pid2 ~= nil then
			Trigger.RemoveProximityTrigger(pid2)
			pid2 = nil
		end
		Trigger.ClearAll(cabconyard1)
	end)

	-- MCV killed before captured.
	Trigger.OnKilled(cabconyard1, function(self, killer)
		if pid1 ~= nil then
			Trigger.RemoveProximityTrigger(pid1)
			pid1 = nil
		end
		if pid2 ~= nil then
			Trigger.RemoveProximityTrigger(pid2)
			pid2 = nil
		end
		Trigger.ClearAll(cabconyard1)

		AISellMCVResearchBases()
		player.MarkFailedObjective(SecondaryObjectiveCaptureMCV)

		MCVFailedMessage()
	end)
	-- ##### End of Secondary Storyline: Captrue MCV




	-- ##### Shopkeeper: Mercenaries

	TunnelShop = nil
	HireDummy = Actor.Create("hire.production.dummy", true, {Owner = neutral, Location = mound1.Location})
	IsShopAngry = false

	--- fake produce property:
	local production_facing = Angle.New(640)
	local production_woffset = WVec.New(256, 256, 0)
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

	Trigger.AfterDelay(700, function() -- give enough time to load shoopkeeper to initial the tunnel
		AISell(tunnel1)
		AISell(tunnel2)
	end)

	pid3 = Trigger.OnEnteredProximityTrigger(mound1.CenterPosition, WDist.New(1024 * 4), function(a, id)
		if (IsShopAngry) then
			Trigger.RemoveProximityTrigger(id)
			return
		end

		if a.Owner == player and a.Type ~= "qdrone" then
			Trigger.RemoveProximityTrigger(id)
			mound1.Destroy()
			Media.PlaySound("ssneakat.wav")

			TunnelShop = Actor.Create("mutventshop", true, {Owner = mercenary_ai, Location = mound1.Location})
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
				HireDummy.Owner = player
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
					local unit = Actor.Create(name, true, { Owner = player,Facing = production_facing, Location = producer.Location + production_coffset, CenterPosition = producer.CenterPosition + production_woffset })
					if unit ~= nil then
						unit.Patrol({producer.Location + rallypoint1_offset, producer.Location + rallypoint2_offset}, false)
					end
				end)
			end)
		end
	end)

	Trigger.OnAnyKilled(Shopkeepers, function(a)
		EnrageMercenaries()
	end)
	-- ##### End of Shopkeeper: Mercenaries



	-- ###### Player find the location of alien stuff
	Trigger.OnEnteredProximityTrigger(replicator.CenterPosition, WDist.New(1024 * 15), function(a, id)
		if a.Owner == player then
			Trigger.RemoveProximityTrigger(id)
			baseCamera = Actor.Create("camera", true, { Owner = player, Location = replicator.Location })
			replicator.Flash(HSLColor.FromHex("FFFFFF"), 20, DateTime.Seconds(1) / 4)
			ReplicatorFoundMessage()
		end
	end)

	-- ###### Player capture the alien stuff, gg
	Trigger.OnCapture(replicator, function()
		MissionCompleteMessage()
		CheckObjectivesOnMissionEnd(true)
	end)

	-- ###### Player fail the mission
	Trigger.OnKilled(replicator, function(self, killer)
		CheckObjectivesOnMissionEnd(false)
	end)

	Trigger.OnKilled(hacker, function(self, killer)
		CheckObjectivesOnMissionEnd(false)
	end)
end
