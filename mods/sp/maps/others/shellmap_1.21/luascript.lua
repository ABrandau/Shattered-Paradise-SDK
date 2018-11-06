cameras = { CAMERA_NOD_CABAL, CAMERA_MUTANT_CABAL, CAMERA_SCRIN_GDI }

WorldLoaded = function()
	Camera.Position = CAMERA_NOD_AMBUSH.CenterPosition
end

ticks = 0
index = 1
interval = 250
--speed = 1

Tick = function()
	ticks = ticks + 1
	
	if ticks % interval == 0 then
		Camera.Position = cameras[index].CenterPosition
		if index + 1 > #cameras then
			index = 1
		else
			index = index + 1
		end
	end
	--local t = (ticks + 45) % (360 * speed) * (math.pi / 180) / speed;
	--Camera.Position = ViewPortCam.CenterPosition + WVec.New(25000 * math.sin(t) * 1.5, 30000 * math.cos(t) * 1.5, 0) - WVec.New(1024 * 45, -1024 * 20, 0)
end