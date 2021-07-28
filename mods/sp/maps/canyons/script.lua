--Lists
spawns = {
    T1A = {
        {Spawn=T1A1S.Location,Entry=T1A1E.Location},
        {Spawn=T1A2S.Location,Entry=T1A2E.Location},
        {Spawn=T1A3S.Location,Entry=T1A3E.Location},
    },
    T1G = {
        {Spawn=T1G1S.Location,Entry=T1G1E.Location},
        {Spawn=T1G2S.Location,Entry=T1G2E.Location},
        {Spawn=T1G3S.Location,Entry=T1G3E.Location},
    },
    T2 = {
        {Spawn=T2G1S.Location,Entry=T2G1E.Location},
        {Spawn=T2G2S.Location,Entry=T2G2E.Location},
    },
    T3 = {
        {Spawn=T3G1S.Location,Entry=T3G1E.Location},
        {Spawn=T3G2S.Location,Entry=T3G2E.Location},
        {Spawn=T3G3S.Location,Entry=T3G3E.Location},
        {Spawn=T3H1S.Location,Entry=T3H1E.Location},
        {Spawn=T3H2S.Location,Entry=T3H2E.Location},
    },
}


GDISpawnList = {
    Infantry = {
        leader = {unit="grenadier",cost=300,weight=1}, --leader should be slowest unit
        {unit="gdie1", cost=150, weight=6},
        {unit="grenadier", cost=300, weight=2},
        {unit="medic", cost=200, weight=1},
    },
    Fast = {
        {unit="hvr", cost=1100, weight=2},
        {unit="apc", cost=700, weight=4},
        {unit="smech", cost=400, weight=6},
    },
    Armoured = {
        {unit="g4tnk", cost=1500, weight=3},
        {unit="sonic", cost=1200, weight=2},
        {unit="mmch", cost=900, weight=4},
        {unit="jug", cost=1000, weight=3},
        {unit="hvr", cost=1100, weight=4},
        {unit="grenadier", cost=300, weight=5},
        {unit="apc", cost=700, weight=3},
        {unit="eagleguard", cost=600, weight=4},
        {unit="medic", cost=200, weight=1},
        {unit="jumpjet", cost=450, weight=4},
    },
    --Walkers = {
    --},
    --Mechanized = {
    --},
    --AirRaid = {
    --},
}
mutSpawnList = {
    InfLight = {
        leader = {unit="MARAUDERCOOP",cost=100,weight=1},
        {unit="MARAUDERCOOP", cost=100, weight=12},
        {unit="mutfiend", cost=250, weight=4},
    },
    InfMedium = {
        leader = {unit="MARAUDERCOOP",cost=100,weight=1},
        {unit="MARAUDERCOOP", cost=100, weight=6},
        {unit="mutfiend", cost=250, weight=2},
        {unit="e3", cost=200, weight=2},
    },
    InfHeavy = {
        {unit="MARAUDERCOOP", cost=100, weight=12},
        {unit="mutfiend", cost=250, weight=6},
        {unit="e3", cost=200, weight=4},
        {unit="seer", cost=450, weight=1},
    },
    InfCom = {
        leader = {unit="psyker", cost=1500, weight=1},
        {unit="MARAUDERCOOP", cost=100, weight=12},
        {unit="mutfiend", cost=250, weight=6},
        {unit="e3", cost=200, weight=4},
        {unit="seer", cost=450, weight=2},
    },
    TankLight = {
        {unit="lynx", cost=700, weight=8},
        {unit="mutquad", cost=600, weight=1},
    },
    TankMedium = {
        {unit="lynx", cost=700, weight=4},
        {unit="mutquad", cost=600, weight=4},
        {unit="wolf", cost=500, weight=4},
        --{unit="mrls", cost=900, weight=2},
    },
    TankHeavy = {
        {unit="lynx", cost=700, weight=3},
        {unit="mutquad", cost=600, weight=4},
        {unit="wolf", cost=500, weight=3},
        {unit="mrls", cost=900, weight=4},
        {unit="deathclaw", cost=1100, weight=3},
    },
    SURPRISE = {
        {unit="hvrtruk3", cost=1250, weight=8},
        {unit="mutquad", cost=300, weight=2},
    },
    AirLight = {
        {unit="mutheli", cost=1400, weight=1},
    },
    MixedLight = {
        {unit="MARAUDERCOOP", cost=100, weight=12},
        {unit="mutfiend", cost=250, weight=4},
        {unit="lynx", cost=700, weight=1},
        {unit="mutheli", cost=1400, weight=1},
    },
    MixedHeavy = {
        {unit="mutheli", cost=1400, weight=1},
        {unit="mutqueen", cost=1000, weight=2},
        {unit="mutfiend", cost=250, weight=6},
        {unit="lynx", cost=700, weight=3},
        {unit="mutquad", cost=600, weight=4},
        {unit="wolf", cost=500, weight=8},
        {unit="deathclaw", cost=1100, weight=3},
        {unit="mrls", cost=900, weight=3},
    },
    Artillery = {
        {unit="mrls", cost=900, weight=6},
        {unit="mutquad", cost=600, weight=2},
    },
}




--UTILITY FUNCTIONS
function round(number, decimals)
    if decimals == nil then decimals = 0 end
    local power = 10^decimals
    return math.floor(number * power) / power
end

function gridDist(pos1,pos2)
    return math.sqrt((pos1.X - pos2.X)^2 + (pos1.Y - pos2.Y)^2)
end

function Count(table)
    local counter = 0
    for k,v in pairs(table) do
        counter = counter + 1
    end
    return counter
end

function Pick(table)
    return table[Utils.RandomInteger(1, Count(table) + 1)]
end

function IsGround(actor)
    return Actor.CruiseAltitude(actor.Type) == 0
end

function PickleDict(table)
    local keyset={}
    local n=0
    for k,v in pairs(table) do
      n=n+1
      keyset[n]=k
    end
    return(table[Pick(keyset)])
end

function BestMove(unit,waypoint)
    --if unit.HasProperty(AttackBase) then unit.AttackMove(waypoint) else unit.Move(waypoint) end
    unit.AttackMove(waypoint)
end

function SideValue(player)
    local armyValue = 0
    for name,actor in ipairs(Map.ActorsInWorld()) do
        if actor.Owner == player then
            armyValue = armyValue + Actor.Cost(actor.Type)
        end
    end
end

--UNIT SPAWNERS

function SpawnSquad(team, spawn, sqPlayer)
    local squad = {}
    local index = 1
    for k,v in pairs(team) do
        squad[index] = Actor.Create(v, true, { Owner = sqPlayer, Location = spawn.Spawn }) --, SubCell = 1
        if IsGround(squad[index]) then
            squad[index].MoveIntoWorld(spawn.Entry)
        else
            squad[index].Move(spawn.Entry)
        end
        if IsGround(squad[index]) then squad[index].Scatter() end
        index = index + 1
    end
    return squad
end



function GenerateAttack(budget, spawnlist)
    local unitlist
    if spawnlist.leader == nil then 
        unitlist = {} 
    else 
        unitlist = {spawnlist.leader.unit} 
    end
    local dieSize = 0
    for i,v in ipairs(spawnlist) do
        dieSize = dieSize+v.weight
    end
    local dieRoll = 0
    r = 0
    local i = 0
    while budget > 0 do
        i = i+1
        dieRoll = Utils.RandomInteger(1, dieSize)
        --dieRoll = math.random(0, dieSize)
        t = 0 --threshold
        for i,v in ipairs(spawnlist) do
            t = t + v.weight
            if t >= dieRoll then
                unitlist[#unitlist+1] = v.unit
                budget = budget - v.cost
                --print(v.unit)
                break
            end
        end
    end
    return(unitlist)
end

--BASE BUILDER SCRIPTS
function spawnBase(blist,plr,ref) --Reference is CPos
    if ref == nil then
        ref = CPos.New(0,0)
    end
    local timerDummy = Actor.Create("waypoint", true, { Owner = Player.GetPlayer("Neutral"), Location = CPos.New(2,2) })
    local delay
    for i,v in ipairs(blist) do
        if v.Unit == "shawall" then
            delay = 1
        else
            delay = 12
        end
        timerDummy.Wait(delay)
        timerDummy.CallFunc(function() Actor.Create(v.Unit, true, { Owner = plr, Location = CPos.New(v.X+ref.X,v.Y+ref.Y) }) end)
    end
    timerDummy.Destroy()
end

function readBase(ref) --Reference is CPos
    if ref == nil then
        ref = CPos.New(0,0)
    end
    print("{")
    for i,unit in ipairs(Map.ActorsInWorld) do
        if i > 10 then
            print(string.format("\t{Unit = \"%s\", X = %s, Y = %s,},",unit.Type, unit.Location.X - ref.X, unit.Location.Y - ref.Y))
        end
    end
    print("}")
end


-- * * * SQUAD FUNCTIONS

-- * Core SQUAD
function sqNew(roster)
    local newSquad = {
        Roster = roster,
        Control = Actor.Create("waypoint", true, { Owner = Player.GetPlayer("Neutral"), Location = CPos.New(2,2) }),
        Orders = {Override={},Main={},Optional={}},
        Scout = roster[1],
        OverrideTimer = 0,
        RemoveSquad = false,
    }
    Trigger.OnAllKilled(
        newSquad.Roster, 
        function() 
            newSquad.RemoveSquad = true
        end
    )
    return(newSquad)
end

sqTick = nil
sqTick = function(squad)
    --Media.DisplayMessage("")
    --squad.Heart.Wait(squad.Heartrate)
    --squad.Heart.CallFunc( sqTick(squad) )
    local stop = false
    for i,order in ipairs(squad.Orders.Override) do
        stop = order(squad)
        if stop == 1 then return
        elseif stop == 2 then table.remove(squad.Orders.Override,i) return
        end
    end
    for i,order in ipairs(squad.Orders.Main) do
        --Media.DisplayMessage("Right order set!")
        stop = order(squad)
        if stop == 1 then return
        elseif stop == 2 then table.remove(squad.Orders.Main,i) return
        end
    end
    for i,order in ipairs(squad.Orders.Optional) do
        stop = order(squad)
        if stop == 1 then return
        elseif stop == 2 then table.remove(squad.Orders.Optional,i) return
        end
    end
end

function sqMaxDistanceFromScout(squad)
    local maxDist = 0
    for i,unit in ipairs(squad.Roster) do
        if not unit.IsDead then
            local dist = gridDist(squad.Scout.Location,unit.Location)
            if dist > maxDist then
                maxDist = dist
            end
        end
    end
    return maxDist
end

-- * SQUAD BEHAVIOURS

--Makes the squad bear down on any enemies that attack more reliably than
--with simple AttackMove
function sqSetAggro(squad,huntTimer,delayTimer)
    if huntTimer == nil then huntTimer = 3 end
    if delayTimer == nil then delayTimer = 2 end
    for i,unit in ipairs(squad.Roster) do
        Trigger.OnKilled(
            unit,
            function()
                if squad.OverrideTimer > 0 then
                    squad.OverrideTimer = delayTimer
                    return
                else squad.OverrideTimer = delayTimer end
                squad.Orders.Override[#squad.Orders.Override+1] = function(sqd)
                    if sqd.OverrideTimer > 0 then
                        sqd.OverrideTimer = sqd.OverrideTimer - 1 
                        return 1
                    else 
                        return 2 
                    end
                end
                for i,unit in ipairs(squad.Roster) do
                    if not unit.IsDead then
                        unit.Stop()
                        unit.Hunt()
                    end
                end
                squad.Control.Wait(huntTimer*25)
                squad.Control.CallFunc(
                    function()
                        for i,unit in ipairs(squad.Roster) do
                            if not unit.IsDead then
                                unit.Stop()
                            end
                        end
                    end
                )
            end
        )
    end
end

--Has squad members guard on scout, who then walks the path to target, stopping to wait for stragglers
function sqMove(squad,waypoint,closeEnough,maxDist)
    if closeEnough == nil then closeEnough = 5 end
    if maxDist == nil then maxDist = 5 end
    squad.Orders.Main[#squad.Orders.Main+1] = function(sqd)
        --Actual Order Begins, this order will be executed every squad tick unless there is a higher priority one
        local newScout
        if sqd.Scout.IsDead then newScout = true else newScout = false end
        for i,unit in ipairs(sqd.Roster) do
            if not unit.IsDead then
                if newScout then 
                    sqd.Scout = unit 
                    newScout = false
                end
                unit.Guard(sqd.Scout)
            end
        end
        if newScout then Media.DisplayMessage("Big Bad Error!") return end
        sqd.Scout.Stop()
        if gridDist(sqd.Scout.Location,waypoint) < closeEnough then
            return 2
        else
            if sqMaxDistanceFromScout(squad) > maxDist then
                sqd.Scout.Stop()
            else
                BestMove(sqd.Scout,waypoint)
            end
            return 1
        end
    end
end

--Has squad members path directly to target, 
--and only regroup on leader/scout if they stray too far
--Suitable for large groups
function sqAltMove(squad,waypoint,closeEnough,maxDist)
    if closeEnough == nil then closeEnough = 5 end
    if maxDist == nil then maxDist = 5 end
    squad.Orders.Main[#squad.Orders.Main+1] = function(sqd)
        --Actual Order Begins, this order will be executed every squad tick unless there is a higher priority one
        --Assign new scout if old one is dead
        if sqd.Scout.IsDead then 
            for i,unit in ipairs(sqd.Roster) do
                if not unit.IsDead then
                    sqd.Scout = unit
                    break
                end
            end
        end
        if sqMaxDistanceFromScout(sqd) > maxDist then
            --If the squad is too scattered, regroup on random unit
            for i,unit in ipairs(sqd.Roster) do
                if not unit.IsDead then
                    unit.Stop()
                    unit.Guard(sqd.Scout)
                end
            end
            sqd.Scout.Stop()
        else
            --Otherwise, move out!
            for i,unit in ipairs(sqd.Roster) do
                if not unit.IsDead then
                    unit.Stop()
                    BestMove(unit,waypoint)
                end
            end
        end
        
    end
end

--MISSION-SPECIFIC LOGIC, UTILITY

squadsAll = {} --THIS IS WHERE ALL SQUADS LIVE

--Set AI objectives and objective completion triggers
obj = {
    outpost = {Standing = true, Location = TgtOutpost.Location, Name = "outpost"},
    cliffs = {Standing = true, Location = TgtCliffs.Location, Name = "cliffs"},
    fortress = {Standing = true, Location = TgtFortress.Location, Name = "fortress"},
    town = {Standing = true, Location = TgtTown.Location, Name = "town"},
    laststand = {Standing = true, Location = TgtLastStand.Location, Name = "laststand"},
}

--PLAYER OBJECTIVE

function objectiveKilled()
    --TODO:
    local x
end

function miPCash(amount)
    plrs.plr0.Cash = plrs.plr0.Cash + amount
    plrs.plr1.Cash = plrs.plr1.Cash + amount
end

function MiCashTrickle(amount,duration) 
    --do yourself a favour and use cash amounts rounded to 50
    --The function might start funkying up otherwise
    local controller = Actor.Create("waypoint", true, { Owner = plrs.civ, Location = CPos.New(2,2) })
    local i = round(amount/50)
    local tLen = round(duration/i)
    while i > 0 do
        i = i-1
        controller.CallFunc(
            function()
                miPCash(50)
            end
        )
        controller.Wait(tLen)
    end
    controller.Destroy()
end

miObjectiveTriggers = function()
	print("Assigning Objectives")
	pobj0 = plrs.plr0.AddPrimaryObjective("Defend!")
	pobj1 = plrs.plr1.AddPrimaryObjective("Defend!")
	print("Assigned Objectives")
    Trigger.OnKilled(
        TgtOutpostStr1, 
        function() 
            obj.outpost.Standing = false  
            reassignSquads()
            objectiveKilled()
			Media.DisplayMessage("The outpost has been destroyed!","Mission")
        end
    )
    Trigger.OnAllKilled(
        {TgtCliffsStr1,TgtCliffsStr2,TgtCliffsStr3,TgtCliffsStr4}, 
        function() 
            obj.cliffs.Standing = false 
            reassignSquads() 
            objectiveKilled()
			Media.DisplayMessage("The Tiberium Silos have been destroyed!","Mission")
            end
    )
    Trigger.OnKilled(
        TgtFortressStr1, 
        function() 
            obj.fortress.Standing = false 
            reassignSquads()
            objectiveKilled()         
			Media.DisplayMessage("The Fortress has fallen!","Mission")
        end
    )
    Trigger.OnKilled(
        TgtTownStr1, 
        function() 
            obj.town.Standing = false 
            reassignSquads()
            objectiveKilled()
			Media.DisplayMessage("The City Hospital has fallen!","Mission")
        end
    )
    Trigger.OnKilled(
        TgtLastStandStr1, 
        function() 
            obj.laststand.Standing = false 
            reassignSquads()
            objectiveKilled()
			Media.DisplayMessage("The Last Stand has fallen!","Mission")
        end
    )
    --#TODO: Add FAIL MISSION trigger here
	Trigger.OnAllKilled(
		{TgtOutpostStr1,TgtCliffsStr1,TgtCliffsStr2,TgtCliffsStr3,TgtCliffsStr4,TgtFortressStr1,
		TgtTownStr1,TgtLastStandStr1},
		function()
			plrs.plr0.MarkFailedObjective(pobj0)
			plrs.plr1.MarkFailedObjective(pobj1)
		end
	)
    --#TODO: Add NEW SPAWNS unlocking trigger here
    --#TODO: Add TERROR LEVEL RESET trigger here
end

miActiveSpawns = spawns.T1G

miUnitTier = {
    Zero = {
        mutSpawnList.InfLight,
    },
    Early = {
        mutSpawnList.InfLight,
        mutSpawnList.InfLight,
        mutSpawnList.InfMedium,
        mutSpawnList.InfMedium,
        mutSpawnList.TankLight,
        mutSpawnList.TankLight,
        {leader={unit="mutheli",cost="2800",weight=1},{unit="mutheli",cost="2800",weight=1}},
    },
    Middle = {
        mutSpawnList.InfMedium,
        mutSpawnList.InfMedium,
        mutSpawnList.TankMedium,
        mutSpawnList.TankMedium,
        mutSpawnList.AirLight,
        mutSpawnList.MixedLight,
        mutSpawnList.MixedLight,
    },
    Late = {
        mutSpawnList.InfHeavy,
        mutSpawnList.TankMedium,
        mutSpawnList.TankMedium,
        mutSpawnList.TankHeavy,
        mutSpawnList.MixedLight,
        mutSpawnList.MixedHeavy,
        mutSpawnList.SURPRISE,
    },
    Hell = {
        mutSpawnList.TankHeavy,
        mutSpawnList.MixedHeavy,
        mutSpawnList.SURPRISE,
        mutSpawnList.InfCom,
    },
}

function miPickObjective(obj)
    local i=1
    local options = {}
    if obj.outpost.Standing then options[i] = obj.outpost i=i+1 end
    if obj.cliffs.Standing then options[i] = obj.cliffs i=i+1 end
    if obj.cliffs.Standing and obj.outpost.Standing then return Pick(options) end
    if obj.fortress.Standing then options[i] = obj.fortress i=i+1 end
    if obj.town.Standing and ((not obj.fortress.Standing) or (not obj.outpost.Standing)) 
        then options[i]=obj.town i=i+1 end
    if obj.fortress.Standing and obj.town.Standing then return Pick(options) end
    options[i] = obj.laststand 
    i=i+1
    return(Pick(options))
end

--TODO: TEMP FUNCTION
function reassignSquads()
    for i,sq in ipairs(squadsAll) do
        if #sq.Orders.Main == 0 then
            local newTarget = miPickObjective(obj)
            sqMove(sq,newTarget.Location)
        end
    end
end

function miSendWave(budget,nsquads,types,objective,spawns)
    local i = nsquads
    local budget = round(budget/nsquads,0)
    while i > 0 do
        print("Loop2")
        local spawn  = Pick(spawns)
        local spawnlist = Pick(types)
        local units = GenerateAttack(budget,spawnlist)
        local sq = sqNew(SpawnSquad(units, spawn, plrs.inv))
        sqSetAggro(sq,4,3)
        sqMove(sq,objective.Location,5,5)
        squadsAll[#squadsAll+1] = sq
        i=i-1
    end
end

function miLaunchAssault(nwaves,duration,curve,budget,types,objective,spawns)
	print("\tLaunch Ass")
    local controller = Actor.Create("waypoint", true, { Owner = plrs.civ, Location = CPos.New(2,2) })
    local i = nwaves
    local waveBudget = round(budget/nwaves,0)
    local nsquads = 1
    if waveBudget > 10000 then
        --split up big waves into several squads
        nsquads = round(waveBudget/10000,0) --Split waves above $15000 value
    end
    local interModifiers = {}
    local interSum = 0
    local interLast = 1
    while i > 0 do
        --Make wave intervals increase exponentially while fitting into duration:
        --The wave interval keeps shrinking with each wave 80% (recursive)
        --EG first wave takes 100s, the next one 80s (80% of 100s), the next one 64s (80% of 80s) etc.
        --If you have a lot of waves, most of them will end up spawning close to the end of the assault.
        --This bit of code ensures that all the intervals added together 
        --will be equal to just about the "duration" parameter of the assault
        interSum = interSum + interLast
        interModifiers[i] = interLast
        interLast = interLast * curve
        i = i-1
    end
    interScale = duration/interSum
    i = nwaves
    while i > 0 do
        controller.CallFunc(
            function()
                if not objective.Standing then
                    --if objective is completed, do not spawn the rest of the wave
                    controller.Stop()
                    controller.Destroy()
                    return
                end
                miSendWave(waveBudget,nsquads,types,objective,spawns)
            end
        )
        controller.Wait( round(interScale*interModifiers[i],0) )
        i = i-1
    end
    controller.Destroy()
end


function miPhase(bAss,bAux,bBos,tier,auxTier,waveObj1)
	print("Phase Begin")
    if auxTier == nil then auxTier = tier end
	print("Phase ObjPick")
	local waveObj1 = miPickObjective(obj)
	local phCtrl = Actor.Create("waypoint", true, { Owner = plrs.civ, Location = CPos.New(2,2) })
	dbgObjective(waveObj1)
    phCtrl.CallFunc(
        function()
            miLaunchAssault(
                8,                      --waves
                240*25,                 --duration
                0.85,                    --curve (multiplier by which wave interval decreases/increases)
                bAss,                   --budget (total sum for the entire assault)
                tier,        --table of spawnlists
                waveObj1,                --objective
                spawns.T1G             --table of spawns
            )
        end
    )
    -- WHILE THE ASSAULT IS HAPPENING, LAUNCH OCCASIONAL ATTACKS AT OTHER TARGETS
    local i = 4
    bAux = round(bAux/4)
    while i > 0 do
        i = i-1
        phCtrl.Wait(60*25)
        phCtrl.CallFunc(
            function()
                miSendWave(
                    bAux,                  --total budget
                    2,                     --nsquads
                    auxTier,       --table of spawnlists
                    miPickObjective(obj),  --objective
                    spawns.T1G             --table of spawns
                )
            end
        )
    end
    --Short delay of 10s for spacing reasons
    phCtrl.Wait(10*25)
    -- AT THE END OF THE 4 MINUTES, SPAWN BOSS WAVE
    phCtrl.CallFunc(
        function()
            miSendWave(
                bBos,                   --budget for wave, ~1300 per squad (<13 units)
                6,                      --nsquads
                tier,        --table of spawnlists
                waveObj1,                --objective
                spawns.T1G              --table of spawns
            )
        end
    )
	phCtrl.Destroy()
	print("Phase End")
end

function dbgObjective(objective)
	print("dbgObjective:")
	print("\t"..tostring(objective.Name) )
	print("\t"..tostring(objective.Standing) )
	print("\t"..tostring(objective.Location) )
end

--TODO: Screw with progression a bit, may-be?
function miMasterPlan()
    local miCtrl = Actor.Create("waypoint", true, { Owner = plrs.civ, Location = CPos.New(2,2) })
    -- DELAY 3 MINUTES BEFORE SPAWNING ANYTHING
    miCtrl.CallFunc( function() Media.DisplayMessage("In A.D.2030, war was beginning.") end)
    miCtrl.Wait(75)
    miCtrl.CallFunc( function() Media.DisplayMessage("What happen?","Commander") end)
    miCtrl.Wait(25)
    miCtrl.CallFunc( function() Media.DisplayMessage("Somebody set us up the liquid tiberium bomb!","Scientist") end)
    miCtrl.Wait(75)
    miCtrl.CallFunc( function() Media.DisplayMessage("We get signal!","EVA") end)
    miCtrl.Wait(50)
    miCtrl.CallFunc( function() Media.DisplayMessage("What!","Commander") end)
    miCtrl.Wait(25)
    miCtrl.CallFunc( function() Media.DisplayMessage("Main screen turn on.","EVA") end)
    miCtrl.Wait(50)
    miCtrl.CallFunc( function() Media.DisplayMessage("It's you!!","Commander") end)
    miCtrl.Wait(25)
    miCtrl.CallFunc( function() Media.DisplayMessage("How are you gentlemen!!","KANE") end)
    miCtrl.Wait(33)
    miCtrl.CallFunc( function() Media.DisplayMessage("All your tiberium are belong to us.","KANE") end)
    miCtrl.Wait(44)
    miCtrl.CallFunc( function() Media.DisplayMessage("You are on the way to destruction.","KANE") end)
    miCtrl.Wait(50)
    miCtrl.CallFunc( function() Media.DisplayMessage("What you say!!","Commander") end)
    miCtrl.Wait(50)
    miCtrl.CallFunc( function() Media.DisplayMessage("You have no chance to survive make your time.","KANE") end)
    miCtrl.Wait(50)
    miCtrl.CallFunc( function() Media.DisplayMessage("Ha ha ha ha","KANE") end)
    miCtrl.Wait(75)
    miCtrl.CallFunc( function() Media.DisplayMessage("Commander!!","EVA") end)
    miCtrl.Wait(75)
    miCtrl.CallFunc( function() Media.DisplayMessage("Deploy every 'MCV'!!","Commander") end)
    miCtrl.Wait(25)
    miCtrl.CallFunc( function() Media.DisplayMessage("You know what you doing.","Commander") end)
    miCtrl.Wait(25)
    miCtrl.CallFunc( function() Media.DisplayMessage("For great justice!","Commander") end)
    miCtrl.Wait(25)
    miCtrl.CallFunc( function() Media.DisplayMessage("Onslaught will begin at minute 3.","Mission") end)
    miCtrl.Wait(25)
    miCtrl.CallFunc( function() Media.DisplayMessage("Enemy waves will advance from the right and attack your tech structures.") end)
    miCtrl.Wait(25)
    miCtrl.CallFunc( function() Media.DisplayMessage("The mission will fail when all tech structures are lost.","Mission") end)
    miCtrl.Wait(138*25)
    -- * PHASE ONE
	print("Phase One")
    -- TRICKLE 5000 to each player (10000 total) over the next 3 minutes
    -- Players should have 20000 total, about 13000 after base building
    miCtrl.CallFunc(function() MiCashTrickle(5000,180*25) end)
    miCtrl.CallFunc(function() miPhase(16000,2000,8000,miUnitTier.Zero,miUnitTier.Early) end)
	-- Wait until phase ends before launching the next one
	miCtrl.Wait(240*25)
    -- * PHASE TWO
	print("Phase Two")
    -- Players should have 20000+20000 overall
    miCtrl.CallFunc(function() MiCashTrickle(10000,360*25) end)
    miCtrl.Wait(180*25)
	--Spawn Wave
    miCtrl.CallFunc(function() miPhase(24000,6000,10000,miUnitTier.Early,miUnitTier.Middle) end)
	-- Wait until phase ends before launching the next one
	miCtrl.Wait(240*25)
    -- * PHASE THREE
	print("Phase Three")
    -- Players should have 40000+20000 cash
    -- AI will be spawning       46400 worth of units
    -- This will happen over 360 seconds, with 3 simultaneous assaults that will
    -- hit their crescendos 60 seconds apart, and auxillary attacks
    miCtrl.CallFunc(function() MiCashTrickle(10000,420*25) end)
    miCtrl.Wait(180*25)
    -- Now it's three assaults instead of just one! Spooky.
	-- I did this one manually, but this is an exception
    miCtrl.CallFunc(
        function()
            miLaunchAssault(
                8,                      --waves
                240*25,                 --duration
                0.85,                    --curve (multiplier by which wave interval decreases/increases)
                15000,                   --budget (total sum for the entire assault)
                miUnitTier.Early,        --table of spawnlists
                miPickObjective(obj),                --objective
                spawns.T1G             --table of spawns
            )
        end
    )
	miCtrl.Wait(250)
    miCtrl.CallFunc(
        function()
            miLaunchAssault(
                8,                      --waves
                300*25,                 --duration
                0.80,                    --curve (multiplier by which wave interval decreases/increases)
                15000,                   --budget (total sum for the entire assault)
                miUnitTier.Early,        --table of spawnlists
                miPickObjective(obj),                --objective
                spawns.T1G             --table of spawns
            )
        end
    )
	miCtrl.Wait(250)
    miCtrl.CallFunc(
        function()
            miLaunchAssault(
                6,                      --waves
                380*25,                 --duration
                0.70,                    --curve (multiplier by which wave interval decreases/increases)
                10000,                   --budget (total sum for the entire assault)
                miUnitTier.Middle,        --table of spawnlists
                miPickObjective(obj),                --objective
                spawns.T1G             --table of spawns
            )
        end
    )
	miCtrl.Wait(250)
	miCtrl.CallFunc(function() miPhase(24000,6000,10000,miUnitTier.Early,miUnitTier.Middle) end)
	miCtrl.Wait(240*25)
    --At the same time, AUX attacks are going off as well.
    --These add up to 8*800 = 6400 credits
    local i = 8
    while i > 0 do
        i = i-1
        miCtrl.Wait(45*25)
        miCtrl.CallFunc(
            function()
                miSendWave(
                    800,                  --total budget
                    1,                     --nsquads
                    miUnitTier.Zero,       --table of spawnlists
                    miPickObjective(obj),  --objective
                    spawns.T1G             --table of spawns
                )
            end
        )
    end
    --PHASE FOUR
	print("Phase Four")
	-- Players should have 60000 + 37500 Cash
	-- AI will be spawning         75000 Worth of Cash (2x)
	miCtrl.CallFunc(function() MiCashTrickle(18750,360*25) end)
	miCtrl.Wait(180*25)
	miCtrl.CallFunc(function() miPhase(18000,9000,10500,miUnitTier.Early,miUnitTier.Middle) end)
	miCtrl.Wait(40*25)
	miCtrl.CallFunc(function() miPhase(18000,9000,10500,miUnitTier.Early,miUnitTier.Middle) end)
	miCtrl.Wait(240*25)
	--PHASE FIVE
	print("Phase Five")
	-- Players: 97500 + 45000
	-- AI:   			90000 (2x)
	miCtrl.CallFunc(function() MiCashTrickle(22500,360*25) end)
	miCtrl.Wait(180*25)
	miCtrl.CallFunc(function() miPhase(25000,6000,19000,miUnitTier.Early,miUnitTier.Late) end)
	miCtrl.Wait(40*25)
	miCtrl.CallFunc(function() miPhase(10000,5000,5000,miUnitTier.Early,miUnitTier.Middle) end)
	miCtrl.Wait(40*25)
	miCtrl.CallFunc(function() miPhase(10000,5000,5000,miUnitTier.Early,miUnitTier.Middle) end)
	--PHASE SIX
	print("Phase Six")
	miCtrl.CallFunc(function() MiCashTrickle(45000,520*25) end)
	--PHASE KILL
	--AI: Like 100k or something
    miCtrl.CallFunc(
        function()
            miLaunchAssault(
                8,                      --waves
                240*25,                 --duration
                0.80,                    --curve (multiplier by which wave interval decreases/increases)
                15000,                   --budget (total sum for the entire assault)
                miUnitTier.Late,        --table of spawnlists
                miPickObjective(obj),                --objective
                spawns.T1G             --table of spawns
            )
        end
    )
	miCtrl.Wait(60*25)
    miCtrl.CallFunc(
        function()
            miLaunchAssault(
                8,                       --waves
                240*25,                  --duration
                0.80,                    --curve (multiplier by which wave interval decreases/increases)
                15000,                   --budget (total sum for the entire assault)
                miUnitTier.Late,        --table of spawnlists
                miPickObjective(obj),    --objective
                spawns.T1G             	 --table of spawns
            )
        end
    )
	miCtrl.Wait(90*25)
    miCtrl.CallFunc(
        function()
            miLaunchAssault(
                8,                       --waves
                240*25,                  --duration
                0.80,                    --curve (multiplier by which wave interval decreases/increases)
                20000,                   --budget (total sum for the entire assault)
                miUnitTier.Hell,        --table of spawnlists
                miPickObjective(obj),    --objective
                spawns.T1G             	 --table of spawns
            )
        end
    )
	miCtrl.Wait(60*25)
    miCtrl.CallFunc(
        function()
            miLaunchAssault(
                8,                       --waves
                240*25,                  --duration
                0.80,                    --curve (multiplier by which wave interval decreases/increases)
                25000,                   --budget (total sum for the entire assault)
                miUnitTier.Late,        --table of spawnlists
                miPickObjective(obj),    --objective
                spawns.T1G             	 --table of spawns
            )
        end
    )
	miCtrl.Wait(30*25)
    miCtrl.CallFunc(
        function()
            miLaunchAssault(
                8,                       --waves
                210*25,                  --duration
                0.80,                    --curve (multiplier by which wave interval decreases/increases)
                30000,                   --budget (total sum for the entire assault)
                miUnitTier.Hell,        --table of spawnlists
                miPickObjective(obj),    --objective
                spawns.T1G             	 --table of spawns
            )
        end
    )
	miCtrl.Wait(30*25)
	miCtrl.CallFunc(function() miPhase(24000,6000,10000,miUnitTier.Early,miUnitTier.Middle) end)
	miCtrl.Wait(400*25)
	miCtrl.CallFunc( 
		function() 
			plrs.plr0.MarkCompletedObjective(pobj0)
			plrs.plr1.MarkCompletedObjective(pobj1)
		end 
	)
    --THE END
	print("Phases End")
	
end



--GAME INIT

WorldLoaded = function()
    plrs={
        plr0 = Player.GetPlayer("Multi0"),
        plr1 = Player.GetPlayer("Multi1"),
        def  = Player.GetPlayer("Defenses"),
        inv  = Player.GetPlayer("Invaders"),
        civ  = Player.GetPlayer("Neutral"),
    }
	miObjectiveTriggers()
    miMasterPlan()
	--Actor.Create("rEcOnPoStPaCkeD", true, { Owner = plrs.plr0, Location = CPos.New(196,-81) })
end


sqi=0
sqfreq = 25
mii = 10
mifreq = 500
Tick = function()
    --Squads tick once every 2 seconds
    sqi = sqi+1
    if sqi > sqfreq then
        sqi = 0
        if #squadsAll > 0 then
            for i,sq in ipairs(squadsAll) do
                if sq.RemoveSquad 
                    then table.remove(squadsAll,i)
                    else sqTick(sq)
                end
            end
        end
    end
    mii = mii+1
    if mii > mifreq then
        mii = 0
        --miTick(mifreq)
    end
end