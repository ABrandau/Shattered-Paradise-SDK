--[[
   Copyright (c) The OpenRA Developers and Contributors
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

---Adds a new mandatory objective, translates it and announces it via in-game chat message.
---@param player player recipient of the objective
---@param description string key of the translation string
---@return number id used to query for the objective later
AddPrimaryObjective = function(player, description)
	local translation = UserInterface.Translate(description)
	return player.AddObjective(translation, UserInterface.Translate("primary-objective"), true)
end

---Adds a new optional objective, translates it and announces it via in-game chat message.
---@param player player recipient of the objective
---@param description string key of the translation string
---@return number id used to query for the objective later
AddSecondaryObjective = function(player, description)
	local translation = UserInterface.Translate(description)
	return player.AddObjective(translation, UserInterface.Translate("secondary-objective"), false)
end

---Adds a new mandatory objective, translates it and announces it via in-game chat message.
---@param player player recipient of the objective
---@param description string key of the translation string
---@return number id used to query for the objective later
AddPrimaryObjective = function(player, description)
	local translation = UserInterface.Translate(description)
	return player.AddObjective(translation, UserInterface.Translate("primary-objective"), true)
end

---Adds a new optional objective, translates it and announces it via in-game chat message.
---@param player player recipient of the objective
---@param description string key of the translation string
---@return number id used to query for the objective later
AddSecondaryObjective = function(player, description)
	local translation = UserInterface.Translate(description)
	return player.AddObjective(translation, UserInterface.Translate("secondary-objective"), false)
end

TranslatedNotification = function(who, text, color)
	Media.DisplayMessage(UserInterface.Translate(text), UserInterface.Translate(who), HSLColor.FromHex(color))
end
