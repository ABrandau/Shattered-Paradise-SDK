local completeObjective = function(self)
	for player, id in pairs(self.__handles) do
		player.MarkCompletedObjective(id);
	end
end

local failObjective = function(self)
	for player, id in pairs(self.__handles) do
		player.MarkFailedObjective(id);
	end
end

local isObjectiveCompleted = function(self)
	for player, id in pairs(self.__handles) do
		return player.IsObjectiveCompleted(id);
	end
end

local isObjectiveFailed = function(self)
	for player, id in pairs(self.__handles) do
		return player.IsObjectiveFailed(id);
	end
end

local module = {
	__objectives = {},
	add = function(self, id, primary, description, players)
		local instance = {
			complete = completeObjective,
			fail = failObjective,
			isCompleted = isObjectiveCompleted,
			isFailed = isObjectiveFailed,
			__handles = {}
		};
		for _, player in ipairs(players) do
			local p = Player.GetPlayer(player);
			if primary then
				instance.__handles[p] = p.AddPrimaryObjective(description);
			else
				instance.__handles[p] = p.AddSecondaryObjective(description);
			end
		end
		self.__objectives[id] = instance;
		return instance;
	end,
	get = function(self, id)
		if self.__objectives[id] == nil then
			error("Objective " .. id .. " is undefined.");
		end
		return self.__objectives[id];
	end
};

createModule(module, "objectives");

load(function()
	Player.GetPlayers(function(player)
		if player.IsLocalPlayer then
			Trigger.OnObjectiveAdded(player, function(p, id)
				Media.DisplayMessage(p.GetObjectiveDescription(id), "New " .. string.lower(p.GetObjectiveType(id)) .. " objective");
			end);
			Trigger.OnObjectiveCompleted(player, function(p, id)
				Media.DisplayMessage(p.GetObjectiveDescription(id), string.lower(p.GetObjectiveType(id)):gsub("^%l", string.upper) .. " objective completed");
			end);
			Trigger.OnObjectiveFailed(player, function(p, id)
				Media.DisplayMessage(p.GetObjectiveDescription(id), string.lower(p.GetObjectiveType(id)):gsub("^%l", string.upper) .. " objective failed" );
			end);
		end
		return false;
	end);
end);
