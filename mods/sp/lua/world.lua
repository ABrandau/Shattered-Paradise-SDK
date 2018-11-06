local cancelProximityTrigger = function(self)
	Trigger.RemoveProximityTrigger(self.__handle);
end

local module = {
	addDiscoveredListener = function(self, actor, callback)
		Trigger.OnDiscovered(actor.__handle, function(actorHandle, playerHandle)
			local actor = nil;
			if actorHandle ~= nil then
				actor = actors:getByHandle(actorHandle);
			end
			local player = nil;
			if playerHandle ~= nil then
				player = players:getByHandle(playerHandle);
			end
			callback(actor, player);
		end);
	end,
	addProximityEnterListener = function(self, pos, range, callback)
		local listener = {
			cancel = cancelProximityTrigger
		};
		listener.__handle = Trigger.OnEnteredProximityTrigger(Map.CenterOfCell(pos), range, function(handle, id)
			local actor = nil;
			if handle ~= nil then
				actor = actors:getByHandle(handle);
			end
			callback(listener, actor);
		end);
		return listener;
	end,
	moveCamera = function(self, pos)
		Camera.Position = pos;
	end,
	initCamera = function(self, positions)
		for player, pos in pairs(positions) do
			if player:isLocal() then
				self:moveCamera(pos);
			end
		end
	end
};

createModule(module, "world");
