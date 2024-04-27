--[[
   Copyright (c) The OpenRA Developers and Contributors
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

local target_time = DateTime.Minutes(6)
local timer = target_time

local beacon
local beacon_owner
local neutral
local players = {}
local in_play = false

EachKotHInterval = function()
	KotHText = "\n\nNod Pyramid has no owner."

	if beacon_owner ~= beacon.Owner then
		timer = target_time

		beacon_owner = beacon.Owner
	end

	if beacon_owner ~= neutral then
		timer = timer - 1

		KotHText = "\n\nNod Pyramid is held by: " .. beacon_owner.Name .. "\nTacitus will be found in: " .. Utils.FormatTime(timer)
	end

	local localPlayerIsNull = true
	for i,player in pairs(players) do
		if player.IsLocalPlayer then
			localPlayerIsNull = false
			UserInterface.SetMissionText(DominationText .. KotHText, player.Color)
		end
	end

	if localPlayerIsNull then
		UserInterface.SetMissionText(DominationText .. KotHText)
	end

	local players_still_in = 0
	for i,player in pairs(players) do
		if player.alive == true then
			if player.object.HasNoRequiredUnits() then
				player.alive = false
			else
				players_still_in = players_still_in + 1
			end
		end
	end

	if players_still_in <= 1 then
		for i,player in pairs(players) do
			if player.alive then
				player.object.MarkCompletedObjective(player.objective)
			end
		end
	end

	if timer <= 0 then

		KotHText = "\n\nTacitus is held by " .. beacon_owner.Name
		for i,player in pairs(players) do
			if player.IsLocalPlayer then
				UserInterface.SetMissionText(DominationText .. KotHText, player.Color)
			end
		end

		-- beacon.GrantCondition("activated")

		Trigger.AfterDelay(160, function()
			for i,player in pairs(players) do
				if player.object == beacon_owner or player.object.IsAlliedWith(beacon_owner) then
					player.object.MarkCompletedObjective(player.objective)
				else
					player.object.MarkFailedObjective(player.objective)
				end
			end
		end)

		in_play = false
	end
end

TickKotH = function()
	if in_play then
		EachKotHInterval()
	end
end

WorldLoadedKotH = function()
	neutral = Player.GetPlayer('Neutral')
	beacon_owner = neutral

	if neutral.HasPrerequisites( {"global-koth"} ) then
		local beacons = neutral.GetActorsByType('ntpyra.koth')

		if #beacons > 0 then
			beacon = beacons[1]

			for j,player in pairs(Player.GetPlayers(nil)) do
				if not player.IsNonCombatant then
					players[player.InternalName] = {
						object = player,
						objective = player.AddPrimaryObjective("Hold the Nod Pyramid for " .. Utils.FormatTime(target_time) .. " or destroy all enemy forces."),
						alive = true
					}
				end
			end

			in_play = true
		end
	end
end
