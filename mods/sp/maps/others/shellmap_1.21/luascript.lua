GDIAttackForce = { "gdie1", "gdie1", "gdie1", "gdie1", "gdie1", "grenadier", "grenadier", "grenadier", "smech", "mmch", "mmch", "apc" }
GDIAirForce = { "orca", "orca", "orca" }
NodTemplarRush = { "templar", "templar", "templar", "templar", "templar" }
NodStealthTeam = { "stnk", "stnk" }
NodAirForce = { "scrin", "scrin" }
MutantBusGuys = { "marauder", "marauder", "marauder", "marauder", "mutfiend" }
MutantDemoTruck = { "hvrtruk3" }
MutantFalcon = { "wetp" }
ScrinAttackForce = { "shark", "shark", "shark", "shark", "legio", "legio", "float", "scrscorpion", "scrscorpion", "scrscorpion" }
ScrinAirForce = { "stormrider", "stormrider", "stormrider", "stormrider" }
CABALInfantry = { "cyborg", "cyborg", "cyborg", "cyborg", "cyborg" }
CABALToBeEMPed = { "centurion", "centurion", "reapercab", "paladin" }
CABALAirForce = { "basilisk" }

NodLaserFencePoints = { NodLaserFencePoint1, NodLaserFencePoint2, NodLaserFencePoint3, NodLaserFencePoint4, NodLaserFencePoint5, NodLaserFencePoint6, NodLaserFencePoint7, NodLaserFencePoint8, NodLaserFencePoint9, NodLaserFencePoint10, NodLaserFencePoint11, NodLaserFencePoint12, NodLaserFencePoint13, NodLaserFencePoint14 }

GDIPowerLocation = GDIDestructablePower.Location

GDIPatrol1 = { GDIPatrolA1.Location, GDIPatrolA2.Location }
GDIPatrol2 = { GDIPatrolB1.Location, GDIPatrolB2.Location }
GDIPatrol3 = { GDIPatrolC1.Location, GDIPatrolC2.Location }
NodPatrol1 = { NodPatrolA1.Location, NodPatrolA2.Location }
NodPatrol2 = { NodPatrolB1.Location, NodPatrolB2.Location }
NodPatrol3 = { NodPatrolC1.Location, NodPatrolC2.Location }
NodPatrol4 = { NodPatrolD1.Location, NodPatrolD2.Location }
NodPatrol5 = { NodPatrolE1.Location, NodPatrolE2.Location }
NodPatrol6 = { NodPatrolF1.Location, NodPatrolF2.Location }
ScrinPatrol1 = { ScrinPatrolA1.Location, ScrinPatrolA2.Location }
ScrinPatrol2 = { ScrinPatrolB1.Location, ScrinPatrolB2.Location, ScrinPatrolB3.Location }
MutantPatrol1 = { MutantPatrolA1.Location, MutantPatrolA2.Location, MutantPatrolA3.Location, MutantPatrolA2.Location }
CABALPatrol1 = { CABALPatrolA1.Location, CABALPatrolA2.Location }

SpawnPartrollers = function()
	local marine1 = Actor.Create("gdie1", true, { Owner = gdi, Location = GDIPatrol1[1], SubCell = 1 })
	local phlanx1 = Actor.Create("grenadier", true, { Owner = gdi, Location = GDIPatrol1[1], SubCell = 2 })
	local marine2 = Actor.Create("gdie1", true, { Owner = gdi, Location = GDIPatrol2[1], SubCell = 1 })
	phlanx2 = Actor.Create("grenadier", true, { Owner = gdi, Location = GDIPatrol2[1], SubCell = 2 })
	local wolverine1 = Actor.Create("smech", true, { Owner = gdi, Location = GDIPatrol3[1], Facing = 96 })
	Patrol2A(marine1, GDIPatrol1, DateTime.Seconds(7))
	Patrol2A(phlanx1, GDIPatrol1, DateTime.Seconds(7))
	Patrol2A(marine2, GDIPatrol2, DateTime.Seconds(7))
	Patrol2A(phlanx2, GDIPatrol2, DateTime.Seconds(7))
	Patrol2A(wolverine1, GDIPatrol3, DateTime.Seconds(6))

	local buggy1 = Actor.Create("bggy", true, { Owner = nod, Location = NodPatrol1[1] })
	local buggy2 = Actor.Create("bggy", true, { Owner = nod, Location = NodPatrol2[1] })
	local buggy3 = Actor.Create("rocketbggy", true, { Owner = nod, Location = NodPatrol3[1] })
	local buggy4 = Actor.Create("rocketbggy", true, { Owner = nod, Location = NodPatrol4[1] })
	local crusader1 = Actor.Create("crusader", true, { Owner = nod, Location = NodPatrol5[1], SubCell = 1 })
	local crusader2 = Actor.Create("crusader", true, { Owner = nod, Location = NodPatrol5[1], SubCell = 2 })
	local crusader3 = Actor.Create("crusader", true, { Owner = nod, Location = NodPatrol5[1], SubCell = 3 })
	local crusader4 = Actor.Create("crusader", true, { Owner = nod, Location = NodPatrol6[1], SubCell = 1 })
	local crusader5 = Actor.Create("crusader", true, { Owner = nod, Location = NodPatrol6[2], SubCell = 1 })
	Patrol2A(buggy1, NodPatrol1, DateTime.Seconds(7))
	Patrol2A(buggy2, NodPatrol2, DateTime.Seconds(7))
	Patrol2A(buggy3, NodPatrol3, DateTime.Seconds(6))
	Patrol2A(buggy4, NodPatrol4, DateTime.Seconds(6))
	Patrol2A(crusader1, NodPatrol5, DateTime.Seconds(5))
	Patrol2A(crusader2, NodPatrol5, DateTime.Seconds(5))
	Patrol2A(crusader3, NodPatrol5, DateTime.Seconds(5))
	Patrol2A(crusader4, NodPatrol6, DateTime.Seconds(3))
	Patrol2B(crusader5, NodPatrol6, DateTime.Seconds(3))

	local hovertank1 = Actor.Create("scrmbt", true, { Owner = scr, Location = ScrinPatrol1[1] })
	local glider1 = Actor.Create("scrglyder2", true, { Owner = scr, Location = ScrinPatrol2[1] })
	Patrol2A(hovertank1, ScrinPatrol1, DateTime.Seconds(5))
	Patrol3A(glider1, ScrinPatrol2, DateTime.Seconds(5))

	local fiend1 = Actor.Create("mutfiend", true, { Owner = mut, Location = MutantPatrol1[1], SubCell = 1 })
	local fiend2 = Actor.Create("mutfiend", true, { Owner = mut, Location = MutantPatrol1[3], SubCell = 1 })
	Patrol4A(fiend1, MutantPatrol1, DateTime.Seconds(9))
	Patrol4C(fiend2, MutantPatrol1, DateTime.Seconds(11))

	local dhost1 = Actor.Create("spiderarty", true, { Owner = cab, Location = CABALPatrol1[1] })
	Patrol2A(dhost1, CABALPatrol1, DateTime.Seconds(10))
end

Patrol2A = function(unit, waypoints, delay)
	if unit.IsDead then
		return
	end

	unit.AttackMove(waypoints[1])
	Trigger.AfterDelay(delay, function()
		Patrol2B(unit, waypoints, delay)
	end)
end

Patrol2B = function(unit, waypoints, delay)
	if unit.IsDead then
		return
	end

	unit.AttackMove(waypoints[2])
	Trigger.AfterDelay(delay, function()
		Patrol2A(unit, waypoints, delay)
	end)
end

Patrol3A = function(unit, waypoints, delay)
	if unit.IsDead then
		return
	end

	unit.AttackMove(waypoints[1])
	Trigger.AfterDelay(delay, function()
		Patrol3B(unit, waypoints, delay)
	end)
end

Patrol3B = function(unit, waypoints, delay)
	if unit.IsDead then
		return
	end

	unit.AttackMove(waypoints[2])
	Trigger.AfterDelay(delay, function()
		Patrol3C(unit, waypoints, delay)
	end)
end

Patrol3C = function(unit, waypoints, delay)
	if unit.IsDead then
		return
	end

	unit.AttackMove(waypoints[3])
	Trigger.AfterDelay(delay, function()
		Patrol3A(unit, waypoints, delay)
	end)
end

Patrol4A = function(unit, waypoints, delay)
	if unit.IsDead then
		return
	end

	unit.AttackMove(waypoints[1])
	Trigger.AfterDelay(delay, function()
		Patrol4B(unit, waypoints, delay)
	end)
end

Patrol4B = function(unit, waypoints, delay)
	if unit.IsDead then
		return
	end

	unit.AttackMove(waypoints[2])
	Trigger.AfterDelay(delay, function()
		Patrol4C(unit, waypoints, delay)
	end)
end

Patrol4C = function(unit, waypoints, delay)
	if unit.IsDead then
		return
	end

	unit.AttackMove(waypoints[3])
	Trigger.AfterDelay(delay, function()
		Patrol4D(unit, waypoints, delay)
	end)
end

Patrol4D = function(unit, waypoints, delay)
	if unit.IsDead then
		return
	end

	unit.AttackMove(waypoints[4])
	Trigger.AfterDelay(delay, function()
		Patrol4A(unit, waypoints, delay)
	end)
end

MutantBusService = function()
	Trigger.AfterDelay(DateTime.Seconds(10), function()
		mut.Build(MutantBusGuys, function(actors)
			local bus = Actor.Create("struck", true, { Owner = mut, Location = MutantBusEntry.Location })
			bus.Move(MutantBusStop.Location)

			Trigger.AfterDelay(DateTime.Seconds(5), function()
				Utils.Do(actors, function(actor)
					actor.EnterTransport(bus)
				end)

				Trigger.AfterDelay(DateTime.Seconds(3), function()
					bus.Move(MutantBusEntry.Location)
					bus.Destroy()

					MutantBusService()
				end)
			end)
		end)
	end)
end

GDIAttack = function()
	Trigger.AfterDelay(DateTime.Seconds(7), function()
		gdi.Build(GDIAttackForce, function(actors)
			Utils.Do(actors, function(actor)
				actor.Attack(ScrinTripod)
			end)

			Trigger.OnAllRemovedFromWorld(actors, function()
				Trigger.AfterDelay(DateTime.Seconds(15), function()
					GDIAttack()
				end)
			end)
		end)
	end)
end

GDIAirAttack = function()
	Trigger.AfterDelay(DateTime.Seconds(2), function()
		gdi.Build(GDIAirForce, function(actors)
			Utils.Do(actors, function(actor)
				actor.Attack(CABALRefinery)
			end)

			Trigger.OnAllRemovedFromWorld(actors, function()
				Trigger.AfterDelay(DateTime.Seconds(18), function()
					GDIAirAttack()
				end)
			end)
		end)
	end)
end

NodTemplarAttack = function()
	Trigger.AfterDelay(DateTime.Seconds(20), function()
		nod.Build(NodTemplarRush, function(actors)
			Utils.Do(actors, function(actor)
				actor.Attack(MutantConYard)
			end)

			Trigger.OnAllRemovedFromWorld(actors, function()
				Trigger.AfterDelay(DateTime.Seconds(5), function()
					NodTemplarAttack()
				end)
			end)
		end)
	end)
end

NodStealthAttack = function()
	Trigger.AfterDelay(DateTime.Seconds(5), function()
		nod.Build(NodStealthTeam, function(actors)
			Utils.Do(actors, function(actor)
				actor.Attack(GDIConYard)
			end)

			Trigger.OnAllRemovedFromWorld(actors, function()
				Trigger.AfterDelay(DateTime.Seconds(15), function()
					NodStealthAttack()
				end)
			end)
		end)
	end)
end

NodAirAttack = function()
	Trigger.AfterDelay(DateTime.Seconds(2), function()
		nod.Build(NodAirForce, function(actors)
			Utils.Do(actors, function(actor)
				actor.Attack(MutantTech)
			end)

			Trigger.OnAllRemovedFromWorld(actors, function()
				Trigger.AfterDelay(DateTime.Seconds(15), function()
					NodAirAttack()
				end)
			end)
		end)
	end)
end

MutantAirAttack = function()
	Trigger.AfterDelay(DateTime.Seconds(15), function()
		mut.Build(MutantFalcon, function(actors)
			Utils.Do(actors, function(actor)
				actor.Attack(FalconTarget)
			end)

			Trigger.OnAllRemovedFromWorld(actors, function()
				Trigger.AfterDelay(DateTime.Seconds(10), function()
					MutantAirAttack()
				end)
			end)
		end)
	end)
end

ScrinAttack = function()
	Trigger.AfterDelay(DateTime.Seconds(5), function()
		scr.Build(ScrinAttackForce, function(actors)
			Utils.Do(actors, function(actor)
				actor.Move(ScrinAttackPoint.Location)
				actor.Attack(GDIRax)
				actor.Attack(GDIEagle2)
			end)

			Trigger.OnAllRemovedFromWorld(actors, function()
				Trigger.AfterDelay(DateTime.Seconds(15), function()
					ScrinAttack()
				end)
			end)
		end)
	end)
end

ScrinAirAttack = function()
	Trigger.AfterDelay(DateTime.Seconds(6), function()
		scr.Build(ScrinAirForce, function(actors)
			Utils.Do(actors, function(actor)
				actor.Attack(NodStealthGen)
			end)

			Trigger.OnAllRemovedFromWorld(actors, function()
				Trigger.AfterDelay(DateTime.Seconds(14), function()
					ScrinAirAttack()
				end)
			end)
		end)
	end)
end

CABALInfantryAttack = function()
	Trigger.AfterDelay(DateTime.Seconds(8), function()
		cab.Build(CABALInfantry, function(actors)
			Utils.Do(actors, function(actor)
				actor.Attack(MutantBunker1)
			end)

			Trigger.OnAllRemovedFromWorld(actors, function()
				Trigger.AfterDelay(DateTime.Seconds(15), function()
					CABALInfantryAttack()
				end)
			end)
		end)
	end)
end

CABALVehicleAttack = function()
	Trigger.AfterDelay(DateTime.Seconds(5), function()
		Trigger.AfterDelay(DateTime.Seconds(25), function()
			carryall = Actor.Create("trnsport", true, { Owner = mut, Location = MutantCarryallEntry.Location })
			carryall.Move(MutantCarryallRally.Location)
		end)
		mut.Build(MutantDemoTruck, function(actors)
			Utils.Do(actors, function(actor)
				actor.EnterTransport(carryall)
				demo = actor
			end)
		end)

		cab.Build(CABALToBeEMPed, function(actors)
			Utils.Do(actors, function(actor)
				Trigger.AfterDelay(DateTime.Seconds(4), function()
					actor.Attack(NodEMP)
				end)
			end)

			Trigger.AfterDelay(DateTime.Seconds(2), function()
				carryall.Move(MutantCarryallDropWaypoint.Location)
				carryall.UnloadPassengers(NodEMPTarget.Location)
				carryall.Move(MutantCarryallDropWaypoint.Location)
				carryall.Move(MutantCarryallExit.Location)
				carryall.Destroy()
			end)

			Trigger.AfterDelay(DateTime.Seconds(15), function()
				NodEMP.Attack(NodEMPTarget)
				Trigger.AfterDelay(DateTime.Seconds(4), function()
					NodEMP.Stop()
				end)
			end)
			Trigger.AfterDelay(DateTime.Seconds(22), function()
				if not demo.IsDead then
					demo.Kill()
				end
			end)

			Trigger.OnAllRemovedFromWorld(actors, function()
				Trigger.AfterDelay(DateTime.Seconds(7), function()
					CABALVehicleAttack()
				end)
			end)
		end)
	end)
end

CABALAirAttack = function()
	Trigger.AfterDelay(DateTime.Seconds(4), function()
		cab.Build(CABALAirForce, function(actors)
			Utils.Do(actors, function(actor)
				actor.Attack(GDIDestructablePower)
				actor.Attack(phlanx2)
			end)

			Trigger.OnAllRemovedFromWorld(actors, function()
				Trigger.AfterDelay(DateTime.Seconds(36), function()
					CABALAirAttack()
				end)
			end)
		end)
	end)
end

SetWaypoints = function()
	GDIRax.RallyPoint = GDIRaxRally.Location
	GDIWFac.RallyPoint = GDIWFacRally.Location
	GDIHPad.RallyPoint = GDIHPadRally.Location
	NodRax.RallyPoint = NodRaxRally.Location
	NodWFac.RallyPoint = NodWFacRally.Location
	NodHPad.RallyPoint = NodHPadRally.Location
	MutantRax.RallyPoint = MutantRaxRally.Location
	MutantWFac.RallyPoint = MutantWFacRally.Location
	MutantHPad.RallyPoint = MutantHPadRally.Location
	ScrinRax.RallyPoint = ScrinRaxRally.Location
	ScrinWFac.RallyPoint = ScrinWFacRally.Location
	ScrinHPad.RallyPoint = ScrinHPadRally.Location
	CABALRax.RallyPoint = CABALRaxRally.Location
	CABALWFac.RallyPoint = CABALWFacRally.Location
	CABALHPad.RallyPoint = CABALHPadRally.Location

	GDIHPad.IsPrimaryBuilding = true
	NodHPad.IsPrimaryBuilding = true
	MutantHPad.IsPrimaryBuilding = true
	ScrinHPad.IsPrimaryBuilding = true
end

SetCash = function()
	gdi.Cash = 10000000
	nod.Cash = 10000000
	mut.Cash = 10000000
	scr.Cash = 10000000
	cab.Cash = 10000000
end

SetUpGDIDestructablePP = function()
	GDIDestructablePower.GrantCondition("produced")

	Trigger.OnKilled(GDIDestructablePower, function()
		Trigger.AfterDelay(Actor.BuildTime("gapowr"), function()
			GDIDestructablePower = Actor.Create("gapowr", true, { Owner = gdi, Location = GDIPowerLocation })
			SetUpGDIDestructablePP()
		end)
	end)
end

ticks = 1250
speed = 7

Tick = function()
	ticks = ticks + 1

	local t = (ticks + 45) % (360 * speed) * (math.pi / 180) / speed;
	Camera.Position = viewportOrigin + WVec.New(46080 * math.sin(t), 35840 * math.cos(t), 0)
end

WorldLoaded = function()
	gdi = Player.GetPlayer("GDI")
	nod = Player.GetPlayer("Nod")
	cab = Player.GetPlayer("Cabal")
	mut = Player.GetPlayer("Mutants")
	scr = Player.GetPlayer("Scrin")

	viewportOrigin = Camera.Position

	SetCash()
	SetWaypoints()
	SpawnPartrollers()
	SetUpGDIDestructablePP()
	--SpawnLaserFences(1)
	GDIAttack()
	GDIAirAttack()
	NodTemplarAttack()
	NodStealthAttack()
	NodAirAttack()
	MutantBusService()
	MutantAirAttack()
	ScrinAttack()
	ScrinAirAttack()
	CABALInfantryAttack()
	Trigger.AfterDelay(DateTime.Seconds(48), CABALVehicleAttack)
	CABALAirAttack()
end
