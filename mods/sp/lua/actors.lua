local addCaptureListener = function(self, callback)
	Trigger.OnCapture(self.__handle, function(handle, captorHandle, oldOwnerHandle, newOwnerHandle)
		local captor = nil;
		if captorHandle ~= nil then
			captor = self.__module:getByHandle(captorHandle);
		end
		local oldOwner = nil;
		if oldOwnerHandle ~= nil then
			oldOwner = players:getByHandle(oldOwnerHandle);
		end
		local newOwner = nil;
		if newOwnerHandle ~= nil then
			newOwner = players:getByHandle(newOwnerHandle);
		end
		callback(self, captor, oldOwner, newOwner);
	end);
end

local getEffectiveOwner = function(self)
	return players:getByHandle(self.__handle.EffectiveOwner);
end

local getOwner = function(self)
	return players:getByHandle(self.__handle.Owner);
end

local setOwner = function(self, owner)
	self.__handle.Owner = owner.__handle;
end

local getType = function(self)
	return self.__handle.Type;
end

local getPosition = function(self)
	return self.__handle.CenterPosition;
end

local module = {
	getById = function(self, id)
		local handle = Map.NamedActor(id)
		if handle == nil then
			error("Failed to get named actor " .. id .. ".");
		end
		return self:getByHandle(handle);
	end,
	getByHandle = function(self, handle)
		if handle == nil then
			error("Actor handle is nil.");
		end
		return {
			__module = self,
			__handle = handle,
			addCaptureListener = addCaptureListener,
			getEffectiveOwner = getEffectiveOwner,
			getOwner = getOwner,
			setOwner = setOwner,
			getType = getType,
			getPosition = getPosition
		};
	end,
	getInBox = function(self, topLeft, bottomRight)
		local list = {}
		for _, value in ipairs(Map.ActorsInBox(Map.CenterOfCell(topLeft), Map.CenterOfCell(bottomRight))) do
			table.insert(list, self:getByHandle(value));
		end
		return list;
	end
};

createModule(module, "actors");
