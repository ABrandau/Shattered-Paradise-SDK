local callbacksLoaded = {};
local callbacksTick = {};

function createModule(module, name)
	_G[name] = module;
end

function load(callback)
	table.insert(callbacksLoaded, callback);
end

function tick(callback)
	table.insert(callbacksTick, callback);
end

WorldLoaded = function()
	for _, callback in ipairs(callbacksLoaded) do
		callback();
	end
end

Tick = function()
	for _, callback in ipairs(callbacksTick) do
		callback();
	end
end
