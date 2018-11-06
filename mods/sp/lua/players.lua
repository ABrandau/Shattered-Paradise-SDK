local getId = function(self)
	return self.__handle.InternalName;
end

local isLocal = function(self)
	return self.__handle.IsLocalPlayer;
end

local module = {
	getById = function(self, id)
		local handle = Player.GetPlayer(id)
		if handle == nil then
			error("Failed to get player " .. id .. ".");
		end
		return self:getByHandle(handle);
	end,
	getByHandle = function(self, handle)
		if handle == nil then
			error("Player handle is nil.");
		end
		return {
			__module = self,
			__handle = handle,
			getId = getId,
			isLocal = isLocal
		};
	end
};

createModule(module, "players");
