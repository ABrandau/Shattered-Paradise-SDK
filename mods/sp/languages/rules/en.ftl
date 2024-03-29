## Player

options-tech-level =
    .tech-level1 = 1
    .tech-level2 = 2
    .tech-level3 = 3
    .tech-level4 = 4
    .tech-level5 = 5
    .tech-level6 = 6

checkbox-instant-capture =
    .label = Instant Capture
    .description = Engineers can enter a building without waiting to capture

checkbox-free-minimap =
    .label = Free Minimap
    .description = Minimap is active without a radar building

checkbox-limit-superweapons =
    .label = Limit Super Weapons
    .description = Only 1 of each super weapon can be built by a player

checkbox-multiqueue =
    .label = MultiQueue
    .description = Each production facility can produce individually

checkbox-upgrades =
    .label = Upgrades
    .description = Enables researching upgrades that improve units
        When disabled, several units will change:
        - Mammoth Tank will have railgun but price increased
        - Demo Bike will have +50% damage
        - Blighter MLRS will have tib-gas warhead
        - Glider can transform into aircraft but price increased
        - Limped Drone can target air unit.

checkbox-domination =
    .label = Domination
    .description = Control the flags on the map to win

checkbox-megawealth =
    .label = Megawealth
    .description =  Makes the economy only dependent on Tiberium Extractors
        When enabled, several units will change:
        - Removes all the Tiberium and Tiberium spawner on the map
        - Tiberium Extractors immune to damage
        - Tiberium Extractors gains +200% income
        - Units with Tiberium Conversion upgrade drain from Extractors.

checkbox-king-of-the-hill =
    .label = King of the Hill
    .description = Capture and hold the Kane's Pyramid on the map to win

checkbox-sudden-death =
    .label = Sudden Death
    .description = Players can't build another MCV and get defeated when they lose it

checkbox-campaignunit =
    .label = Campaign-Only Units
    .description = Allow player to build units that appear in the campaign

## World
options-starting-units =
    .no-bases = No Bases
    .mcv-only = MCV Only
    .light-support = Light Support
    .medium-support = Medium Support
    .heavy-support = Heavy Support
    .unholy-alliance = Unholy Alliance

## Armor
label-armor-class =
    .Light = Light
    .Heavy = Heavy
    .Infantry = Infantry
    .Building = Building
    .Defense = Defense
    .Concrete = Concrete
    .Aircraft = Aircraft
    .Shield = Shield

## Text Notification

notification-structure-sold = Structure sold.
notification-unit-sold = Unit sold.
notification-promoted = Unit promoted.
notification-select-target = Select target.
notification-cannot-deploy = Cannot deploy here. 
notification-building-online = Building online.
notification-building-offline = Building offline.
notification-primary-building = Primary building set.
notification-unit-lost = Unit lost.
notification-repair-bridge = Bridge repaired.
notification-building-captured = Building captured.
notification-building-lost-captured = Our building has been captured.
notification-building-ready = Construction complete.
notification-unit-ready = Unit ready.
notification-upgrade-ready = Upgrade complete.
notification-unable-to-comply = Unable to comply. Building in progress.
notification-new-construction-options = New construction options.
notification-low-power = Low power.
notification-low-credits = Insufficient funds.
notification-base-threated = Base under attack.
notification-base-threated-ally = Our ally is under attack.
notification-harvester-threated = Harvester under attack.
notification-more-silo = Silos needed.
notification-repairing = Repairing.
notification-unit-repaired = Unit repaired.
notification-hack-done = Hack Complete.

notification-crate-cloak = Find a personal stealth generator (+cloak)
notification-crate-shield = Find a personal sheild generator (+sheild)
notification-crate-speedup = Find a personal psychic enforcer (+move&fire speed)

notification-engineer = Enemy engineer detected.
notification-subterranean = Subterranean unit detected.
notification-commando = Enemy commando detected.
notification-scrin-epic = Enemy Battlecruiser detected.
notification-gdi-epic = Enemy Mammoth Mk. II detected.
notification-nod-epic = Enemy Lightbringer detected.
notification-mutant-epic = Enemy Weasaurus Lord detected.
notification-cabal-epic = Enemy C.A.B.A.L. Defender detected.
notification-cabal-boss = Enemy Core Defender detected.
notification-gdi-boss = Enemy Mammoth Mk.III detected.
notification-scrin-boss = Enemy Scrin Council detected.


notification-superweapon-detected = Warning: A super weapon has been detected.
notification-empcannon-ready = EMP cannon ready.
notification-superweapon-ready = Our superweapon is online.
notification-superweapon-launched = Warning: A superweapon has been launched.
notification-sneaktunnel-ready = Our Sneak Attack is ready.


#### Unit name and desc

## Default
crate-name = Crate
civilian-name = Civilian
engineer-name = Engineer
civilianbuilding-name = Civilian Building
blossomtree-name = Blossom Tree
tree-name = Tree
rock-name = Rock
tiberiumformation-name = Tiberium Formation
palette-name = Palette
railway-name2 = Railway
gate-name = Gate
gate-desc = Automated barrier that opens for allied units.
    Can be rotated.
sharedwall-name = Wall
sharedwall-desc = Stops infantry, Vehicles and blocks enemy fire.

## Complex prerequisite
pwarfactory-name = Vehicle Production
pbarracks-name = Infantry Production
pradar-name = Radar
pfactoryorair-name = Vehicle or Aircraft Production
ptech-name = Tech Center
mcvgdi-name = Play as GDI or has GDI Conyard
mcvnod-name = Play as Nod or has Nod Conyard
mcvmut-name = Play as Mutant or has Mutant Conyard
mcvcab-name = Play as Cabal or has Cabal Conyard
mcvscr-name = Play as Scrin or has Scrin Conyard

## Bridge
cabhut-name = Bridge Repair Hut
lowbridge-name = Bridge
deadbridge-name = Dead Bridge
bridgeramp-name = Bridge Ramp

## Shared
e1-name = Light Infantry

engineer-desc = Support infantry.

    Good vs: Buildings

    Special:
     - Can capture neutral and enemy buildings
     - Can repair buildings and bridges
     - Crush class: crushable

gdimcv-name = GDI MCV
mcv-desc = Deploys into a Construction Yard.

    Special:
     - Can crush infantry
     - Provides a build radius for structures when deployed
     - Has increased HP when deployed

gdiharv-name = GDI Harvester
harv-desc = Collects Tiberium for processing.

    Special:
     - Harvests Tiberium
     - Immune to Veins
     - Can crush infantry
     - Selfheal

nodharv-name = Nod Harvester
mutharv-name = Mutant Harvester
cabharv-name = C.A.B.A.L. Harvester
scrharv-name = Scrin Harvester

gacnst-name = GDI Construction Yard
gacnst-desc = Builds base structures.

drefinery-name = Tiberium Refinery
dwarfactory-name = War Factory
dhelipad-name = Helipad
dpowerplant-name = Power Plant
gadept-name = Service Depot
gadept-desc = Provides 3 Repair Drones to repair nearby damaged vehicles.

    Special:
     - Vehicles standing in the center of the building can be sold

scrdepot-name = Scrin Maintenance Depot

napost-name = Laser Fence Section
napost-desc = Stops infantry, vehicles and blocks enemy fire

    Special:
     - Can NOT be crushed
     - Requires power to operate

nafnce-name = Laser Fence

gasilo-name = Silo
gasilo-desc = Stores excess Tiberium.

    Special:
     - Stores 3000$
     - Power loss does not affect storage capacity

dcarryall-name = Carryall
carryall-desc = VTOL aircraft for transporting vehicles.

    Special:
     - Can transport one vehicle

trnsport-name = Orca Carryall
nodtrnsport-name = Nod Carryall
mutrnsport-name = Chinook Carryall
cabtrnsport-name = C.A.B.A.L. Carryall
scrtrnsport-name = Scrin Carryall

napuls-name = EMP Cannon
napuls-desc = Cannon that uses EMP projectiles.

    Special:
     - Fires a pulse blast which disables all ground machines in the area
         - For example: Buildings, Defences, Vehicles and Cyborgs
     - Requires power to operate

scrshield-name = EMP Field Generator

king-name = King Drone
suddendeath-ally = Protect it at all cost!
suddendeath-foe = Destroy it to checkmate!

## Structure
gapowr-name = GDI Power Plant
gapowr-desc = Provides power for other structures.

napowr-name = Nod Power Plant
napowr-desc = Provides power for other structures.

mupowr-name = Mutant Power Plant
mupowr-desc = Provides power for other structures.

naapwr-name = Advanced Power Plant
naapwr-desc = Provides more power than the standard powerplant
    using less space.

gapile-name = GDI Barracks
gapile-desc = Arms Infantry.

    Special:
     - Heals infantry in an area around it.

gdiref-name = GDI Refinery
gdiref-desc = Processes raw Tiberium into useable resources.

    Special:
     - Stores 2000$
     - Can be rotated

nodref-name = Nod Refinery
nodref-desc = Processes raw Tiberium into useable resources.

    Special:
     - Stores 2000$
     - Can be rotated

muproc-name = Mutant Refinery
muproc-desc = Processes raw Tiberium into useable resources.

    Special:
     - Stores 2000$
     - Can be rotated

scrproc-name = Scrin Refinery
scrproc-desc = Processes raw Tiberium into useable resources.

    Special:
     - Stores 4000$
     - Can be rotated

cabref-name = C.A.B.A.L Refinery
cabref-desc = Processes raw Tiberium into useable resources.

    Special:
     - Stores 2000$
     - Can be rotated

gaweap-name = GDI War Factory
gaweap-desc = Produces vehicles.

naweap-name = Nod War Factory
naweap-desc = Produces vehicles.

muweap-name = Mutant War Factory
muweap-desc = Produces vehicles and large beasts.

cabweap-name = C.A.B.A.L. War Factory
cabweap-desc = Produces vehicles and large drones.

srcweap-name = Scrin Forge
srcweap-desc = Produces vehicles.

garadr-name = GDI Radar
garadr-desc = Provides an overview of the battlefield and provides the Spy Satellite ability.

    Special:
     - Provides minimap
     - Stealth detection
     - Provides Spy Satellite
     - Requires power to operate

naradr-name = Nod Radar
naradr-desc = Provides an overview of the battlefield and provides the Spy Satellite ability.

    Special:
     - Provides minimap
     - Stealth detection
     - Provides Spy Satellite
     - Requires power to operate

muradr-name = Mutant Radar
muradr-desc = Provides an overview of the battlefield and provides the Spy Satellite ability.

    Special:
     - Provides minimap
     - Stealth detection
     - Provides Spy Satellite
     - Requires power to operate

cabradr-name = C.A.B.A.L. Radar
cabradr-desc = Provides an overview of the battlefield and provides the Spy Satellite ability.

    Special:
     - Provides minimap
     - Stealth detection
     - Provides Spy Satellite
     - Requires power to operate

scrradr-name = Signal Transmitter
scrradr-desc = Provides an overview of the battlefield and provides the Spy Satellite ability.

    Special:
     - Provides minimap
     - Stealth detection
     - Provides Spy Satellite
     - Requires power to operate

gahpad-name = GDI Helipad
gahpad-desc = Produces and rearms aircraft.

nahpad-name = Nod Helipad
nahpad-desc = Produces and rearms aircraft.

muair-name = Mutant Helipad
muair-desc = Produces and rearms aircraft.

scrair-name = Stargate
scrair-desc = Warps in warships from a nomad galaxy.

cabair-name = Air Factory
cabair-desc = Produces aircraft.

gatech-name = GDI Tech Center
gatech-desc = Provides access to advanced GDI technologies.

natech-name = Nod Tech Center
natech-desc = Provides access to advanced Nod technologies.

muhall-name = Forgotten Hall
muhall-desc = Provides access to contact with Forgotten Leaders.

scrtech-name = Scrin Laboratory
scrtech-desc = Provides samples to enable the Scrins most complex technologies.

cabtech-name = Supercomputer
cabtech-desc = Provides access to advanced C.A.B.A.L. technologies.

nahfac-name = Lightbringer's Altar
nahfac-desc = Produces vehicles, including the Lightbringer.

    Special:
    - Maximum 1 can be built
    - Spawns Templar when sold

gtdrop-name = Dropship Bay
gtdrop-desc = Staging area for drop pod assault.

    Special:
     - Provides access to Helldiver Drop support
     - Produces the Mammoth Mk. II
     - Requires power to operate

nodyard-name = Nod Construction Yard
mutyard-name = Mutant Construction Yard
cabyard-name = C.A.B.A.L. Construction Yard

nahand-name = Hand of Nod
nahand-desc = Arms Infantry.

    Special:
     - Heals infantry in an area around it

namisl-name = Cluster Missile Silo
namisl-desc = Provides access to Nod Chemical Missiles.

    Special:
     - Provides access to the Chemical Missile
     - Requires power to operate

murax-name = Mutant Armory
murax-desc = Arms Infantry.

    Special:
     - Heals infantry in an area around it

mutsw2-name = Mother Veinhole
mutsw2-desc = With this structure Mutant commanders are able to summon veinhole monster.

    Special:
     - Provides access to Veinhole Monsters
     - Requires power to operate

drached-name = Scrin Host Station

scrpowr-name = Ichor Generator
scrpowr-desc = Provides power for other structures.

scrrax-name = Landing Zone
scrrax-desc = Calls down Infantry.

    Special:
     - Heals infantry in an area around it

scradvpowr-name = Wormhole Generator
scradvpowr-desc = Teleports Infantry and Vehicles to a selected area.

    Special:
     - Provides access to the Instant Wormhole support power
     - Requires power to operate
     - Ally units won't be harmed no matter the destination.
     - Enemies will be put into deadly cells in teleport cells.

cabpowr-name = Firestorm Power Plant
cabpowr-desc = Provides more power than the standard powerplant

cabclaw-name = C.A.B.A.L. Claw
cabclaw-desc = Processes Infantry.

    Special:
     - Heals infantry in an area around it

cabobelisk-name = Nanomachine Core
cabobelisk-desc = C.A.B.A.L.'s Support Superweapon.

    Special:
     - Provides access to Nanomachine Swarms
     - Requires power to operate


## Support
gapowrup-name = Power Turbine
gapowrup-desc = Provides extra power generation, place them on the smokestack of the structure.

gavulc-name = Vulcan Tower
gavulc-desc = Anti-infantry base defense.

    Good vs: Infantry

    Special:
     - Provides stealth detection
     - Requires power to operate

    Upgrades:
     - AP Ammunition

garock-name = RPG Tower
garock-desc = Anti-Armor base defense.

    Good vs: Vehicles, Structures

    Special:
     - Provides stealth detection
     - Requires power to operate

gacsam-name = SAM Tower
gacsam-desc = Anti-Air base defense.

    Good vs: Aircraft

    Special
     - Can attack Air
     - Requires power to operate

gasonc-name = Sonic Emitter
gasonc-desc = High-tech base defense with a powerful harmonic resonance turret.

    Good vs: Ground targets

    Special:
     - Deals heavy damage over time to enemy units in the line of fire
     - Requires power to operate

nalasr-name = Laser Turret
nalasr-desc = Basic laser defense.

    Good vs: Ground targets

    Special:
     - Provides stealth detection
     - Requires power to operate

naobel-name = Obelisk of Light
naobel-desc = High tech laser defense.

    Good vs: Ground targets

    Special:
     - Provides stealth detection
     - Requires power to operate

nasam-name = SAM Site
nasam-desc = Anti-Air base defense.

    Good vs: Aircraft

    Special:
     - Can attack Air
     - Requires power to operate

    Upgrades:
     - Tiberium Core Missiles

mubunkr-name = Bunker
mubunkr-desc = Fortified position where infantry can fire from within.

    Good vs: Depends what has been garrisoned

    Special:
     - Provides stealth detection when garrisoned
     - Infantry garrisoned inside gain extra range
     - Comes with a free marauder

mucannon-name = Guardian Cannon
mucannon-desc = Anti-Armor base defense.

    Good vs: Vehicles

    Special:
     - Provides stealth detection

muflak-name = Flak Station
muflak-desc = Anti-Air base defense.

    Good vs: Aircraft.

    Special:
     - Can attack Air

cabpit-name = Drone Pit
cabpit-desc = Drone Hangar that launches small bomber drones.

    Good vs: Infantry

    Special:
     - Provides stealth detection

cabblast-name = Blaster Turret
cabblast-desc = Artillery defense turret.

    Good vs: Ground targets

    Special:
     - Provides stealth detection
     - Has minimum range of fire
     - Requires power to operate

cabrail-name = Railgun Turret
cabrail-desc = Anti-Air base defense.

    Good vs: Aircraft

    Special
     - Can attack Air
     - Requires power to operate

scrneedler-name = Ichor Waste Turret
scrneedler-desc = Base defense that atacks with tiberium based chemicals.

    Good vs Ground targets

    Special:
     - Requires power to operate
     - Can mutate killed enemies
     - Attacks emit radiation that harm units in the area

    Upgrades:
     - Vinfera Catalysts

scrdrone-name = Pulsar Tower
scrdrone-desc = Advanced base defense.

    Good vs Ground targets

    Special:
     - Impulse deals damage over time and slows enemy units
     - Requires power to operate

scrtractor-name = Tractor Beam
scrtractor-desc = Anti-Air base defense.

    Good vs Aircraft

    Special:
     - Can attack air
     - Freezes the targetted aircraft momentarily
     - Requires power to operate

gafire-name = Firestorm Generator
gafire-desc = Generates a firestorm barrier in a circle around the building.

    Special:
     - When activated it increases the durability of friendly units inside the barrier by 30%
     - Kills enemy units that try to cross the firestorm

gaplug-name = Ion Cannon Control Center
gaplug-desc = Communication facility for the Ion Cannon Array.

    Special:
     - Provides access to the Ion Cannon Array
     - Requires power to operate

nastlh-name = Stealth Generator
nastlh-desc = Support structure that cloaks friendly units and buildings.

    Special:
     - Cloaks friendly assets around it
     - Generator itself is not cloaked
     - Requires power to operate

    Upgrades:
     - Improved Stealth Generator

natmpl-name = Temple of Nod
natmpl-desc = The religious center of the Brotherhood of Nod.

    Special:
     - Provides access to the Apocalypse Missile
     - Requires power to operate

muventi-name = Tunnel Network
muventi-desc = Allows mutants to quickly transport their units across the battlefield.

    Special:
     - Transports units between tunnels (except Epics and Demotrucks)
     - Cannot be repaired by engineers

    Upgrades:
     - Tunnel Repairs

muvent-name = Tunnel
muvent-desc = Allows mutants to quickly transport their units across the battlefield.

    Special:
     - Transports units between tunnels (except Epics and Demotrucks)
     - Cannot be repaired by engineers
     - Cannot be sold

    Upgrades:
     - Tunnel Repairs

mutsw1-name = Meditation Hall
mutsw1-desc = Generates powerful Ion Storms.

    Special:
     - Provides access to the Ion Storm
     - Requires power to operate

scrextractor-name = Shield Generator
scrextractor-desc = Scrin support structure shielding their troops.

    Special:
     - Generates shields around nearby friendly ground vehicles
     - Shields provide additional 150 health points
     - Does not protect against super weapons
     - Requires power to operate

scrsw1-name = Meteor Tractor
scrsw1-desc = Allows the Scrin to track a meteor shower.

    Special:
     - Provides access to the Wrath of the Creator
     - Requires power to operate

cabeye-name = Eye of C.A.B.A.L.
cabeye-desc = High tech detection system.

    Special:
     - Provides huge vision range
     - Shrouds the enemy vision range in a certain area
     - Requires power to operate

cabsw1-name = Iron Savior
cabsw1-desc = Energy cannon based off Scrin technology.

    Special:
     - Provides access to the Iron Savior
     - Requires power to operate

mumine-name = Mine

blackdefd-name = C.A.B.A.L. Defender
blackdefd-desc = C.A.B.A.L.'s ultimate weapon. Defense structure with
    long range plasma artillery. Can transform into
    the mobile Epic walker version.

    Good vs: Ground targets

    Special when deployed:
     - Armed with a plasma artillery launcher
     - Maximum Supply of 1

    Special when mobile:
     - Selfhealing up to 100%
     - Can shoot over walls
     - Can crush everything except other Epics
     - Immune to mind control
     - Build limit: 1

## Vehicle
smech-name = Wolverine
smech-desc = Small but fast anti-infantry walker.

    Good vs: Infantry

    Special:
     - Provides stealth detection
     - Can attack enemies ahead while moving

    Upgrades:
     - AP Ammunition

mmch-name = Titan
mmch-desc = GDI's main battle walker armed with an anti-tank cannon.

    Good vs: Vehicles

    Special:
     - Can shoot over walls
     - Can crush infantry
     - Can attack while moving

    Upgrades:
     - Railgun Barrels

apc-name = Amphibious APC
apc-desc = Armored infantry transport.

    Good vs: Infantry.

    Special:
     - Can transport 5 infantry units
     - Can travel over water
     - Can crush infantry\n - Can attack while moving

    Upgrades:
     - AP Ammunition

hvr-name = Hover MLRS
hvr-desc = Hovering vehicle, armed with long range missiles.

    Good vs: Vehicles, Aircraft

    Special:
     - Can attack Air
     - Can shoot over walls
     - Hovers (ignores terrain like Veins, Water and Radiations)
     - No longer hovers when disabled by EMP
     - Can attack while moving

jug-name = Juggernaut
jug-desc = Heavy mechanized artillery walker.

    Good vs: Infantry, Buildings

    Special:
     - Deals AoE damage
     - Minimum attack distance
     - Can shoot over walls
     - Can crush infantry

sonic-name = Disruptor
sonic-desc = Armored high-tech vehicle with a powerful harmonic resonance cannon.

    Good vs: Ground targets

    Special:
     - Deals heavy damage over time to any unit in the line of fire
     - Can crush infantry
     - Can attack while moving

g4tnk-name = Mammoth Tank
g4tnk-desc = Heavy assault tank.

    Good vs: Vehicles, Aircraft

    Special:
     - Selfheal to 50%
     - Can attack air
     - Only the Missiles can pass over walls
     - Can crush infantry
     - Can attack while moving

    Upgrades:
     - Railgun Barrels

hmec-name = Mammoth Mk. II
hmec-desc = GDI's super-heavy walker and the pride of their army.

    Good vs: Everything

    Special:
     - Build limit: 1
     - Selfhealing up to 100%
     - Can attack air
     - Immune to mind control
     - Can shoot over walls
     - Can crush everything except other Epics
     - Cannot be teleported by wormhole

    Upgrades:
     - AP Ammunition

nodmcv-name = Nod MCV

bggy-name = Raider Buggy
bggy-desc = Fast vehicle armed with a machine gun and
    has room for one passenger that modifies the weapon of the vehicle.

    Good vs: Changes depending on the passenger

    Special:
     - The weapon of the vehicle changes with the passenger
     - Provides stealth detection
     - Can attack while moving

    Upgrades:
     - Raider Passenger

attackbike-name = Attack Bike
attackbike-desc = Fast scout vehicle armed with rockets.

    Good vs: vs Vehicles, Aircraft

    Special:
     - Can attack air
     - Can attack enemies ahead while moving

    Upgrades:
     - Tiberium Core Missiles

ttnk-name = Tick Tank
ttnk-desc = Nod's main battle tank armed with an anti-tank cannon.

    Good vs: Vehicles

    Special:
     - Can deploy to gain extra protection
     - Can crush infantry
     - Can attack enemies ahead while moving

    Upgrades:
     - Laser Capacitors

tickhologram-name = Tick Tank Hologram

bike-name = Demo Bike
bike-desc = Fast moving suicide vehicle.

    Good vs: Ground targets

    Special:
     - Can deploy to explode

    Upgrades:
     - Deadly Mixtures

sapc-name = Subterranean APC
sapc-desc = Troop transport that can move underground.

    Special:
     - Can move underground
     - Can transport up to 5 infantry units
     - Can crush infantry
     - Cannot move or burrow back for 3 seconds after resufacing

subtnk-name = Devil's Tongue
subtnk-desc = Subterranean flame tank able to move underground.

    Good vs: Infantry, Buildings

    Special:
     - Can move underground
     - Can crush infantry
     - Cannot move or burrow back for 3 seconds after resufacing

    Upgrades:
     - Purifying Flame

howtlizer-name = Specter
howtlizer-desc = High tech artillery armed with a self-propelled howitzer and a cloaking device.

    Good vs: Ground targets

    Special:
     - Can shoot over walls
     - Can crush infantry
     - Cloaked
     - Cloaking malfunctions when at red hp

stnk-name = Stealth Tank
stnk-desc = Hit-and-run tank armed with twin dragon TOW missiles and a cloaking device.

    Good vs: Vehicles, Aircraft

    Special:
     - Stealthed
     - Can attack air
     - Can shoot over walls
     - Can crush infantry
     - Cloaking malfunctions when at red hp
     - Can attack while moving

    Upgrades:
     - Tiberium Core Missiles

scorpion-name = Lightbringer
scorpion-desc = Nod's super-heavy walker. One vision one purpose.

    Good vs: Ground

    Special:
     - Build limit: 1
     - Selfhealing up to 100%
     - Immune to mind control
     - Can shoot over walls
     - Can crush everything except other Epics
     - Deployability: Enemy units fire at each other for 5 seconds
     - Cannot be teleported by wormhole

    Upgrades:
     - Purifying Flame

mutmcv-name = Mutant MCV

mutquad-name = Quad Cannon
mutquad-desc = Armored truck armed with a quad cannon.

    Good vs: Infantry, Aircraft

    Special:
     - Provides stealth detection
     - Can crush infantry
     - Can attack while moving

lynx-name = Lynx Tank
lynx-desc = Light tank armed with an anti-tank cannon.

    Good vs: Vehicles

    Special:
     - Can crush infantry
     - Can attack while moving

    Upgrades:
     - Lynx Rockets

struck-name = Battle Bus
struck-desc = Combat transport.

    Good vs: Depending on occupants

    Special:
     - Cargo for 5 soldiers
     - Passengers can fire out of its windows
     - Basic infantry inside has increased weapon range
     - Can attack while moving

minelayer-name = Minelayer
minelayer-desc = Mines...mines everywhere!

    Special:
     - Provides stealth detection
     - Can remove enemy mines
     - Can place up to 5 mines on the battlefield
     - Gain 100% EXP from enemy killed by mines
     - Reloads on service depot
     - Mines are cloaked
     - Can crush Infantry

wolf-name = Carnotaurus
wolf-desc = Tiberium beast that uses tiberium acid to harm nearby units.

    Good vs: Ground targets

    Special:
     - Heals on Tiberium fields
     - Attacks reduce target's attack and movement speed by 35%
     - Can attack enemies ahead while moving
     - Can mutate killed Infantry
     - E.M.P. Immune
     - Wild mind: Mind only controllable by MasterMind

    Upgrades:
     - Stimulant Infusion

mrls-name = Blighter MLRS
mrls-desc = Light artillery armed with unguided rockets.

    Good vs: Infantry, Buildings.

    Special:
     - Deals AoE damage
     - Minimum attack distance
     - Can shoot over walls
     - Can crush infantry
     - Can attack while moving

    Upgrades:
     - Tiberium Gas Warhead

deathclaw-name = Ravager
deathclaw-desc = Melee beast from the outer redzones that brings death with its claws.

    Good vs: Ground targets

    Special:
     - Heals on Tiberium fields
     - E.M.P. Immune
     - Slows enemy units with it's attack
     - Wild mind: Mind only controllable by MasterMind

    Upgrades:
     - Stimulant Infusion

hvrtruk3-name = Demo Truck
hvrtruk3-desc = A hovering nuclear demolition truck with a oneway ticket.

    Good vs: Everything

    Special:
     - Demolishes almost everything nearby upon detonation
     - Hovers (ignores terrain like Veins, Water and Radiations)
     - No longer hovers when disabled by EMP
     - Cannot enter Tunnel Networks or transport, can be carried by carryall

weasau-name = Weasaurus Lord
weasau-desc = A long forgotten creature that has emerged again.

    Good vs: Ground

    Special:
     - Build limit: 1
     - Selfhealing up to 100%
     - Immune to mind control
     - Can shoot over walls
     - Heals on Tiberium fields
     - E.M.P. Immune
     - Deployability: Switch to melee attack gains +50 movement speed
       and Summons a brood of Weasaurus
     - Gains 100% EXP from enemies killed by the summoned brood
     - Can crush everything except other Epics
     - Cannot be teleported by wormhole

weasausmall-name = Small Weasaurus

cabmcv-name = C.A.B.A.L. MCV

centurion-name = Centurion
centurion-desc = Heavy walker equipped with chain guns.

    Good vs: Infantry

    Special:
     - Provides stealth detection
     - Can shoot over walls
     - Can crush infantry
     - Can attack while moving

    Upgrades:
     - Gatling Cannons

reapercab-name = Cyborg Reaper
reapercab-desc = Fast raiding walker armed with missiles and Web launchers.

    Good vs: Vehicles, Aircraft

    Special:
     - Can attack enemies ahead while moving
     - Can target air
     - Can ensnare infantry with Webs
         - For example: Infantry, smaller Beast and Cyborgs.
     - Can shoot over walls

    Upgrades:
     - Paralyzing Reaper Nets

limped-name = Limpet Drone
limped-desc = Small drone armed with explosives. Good vs: Infantry, Vehicles

    Special:
     - Needs to be deployed to be armed
     - Invisible when deployed
     - Launches explosives towards a target after small delay
     - Cannot be teleported by wormhole when deployed

    Upgrades:
     - Limpet AA Missile

repairvehicle-name = Mobile Repair Vehicle
repairvehicle-desc = Repairs nearby vehicles.

    Special:
     - Unarmed
     - Can repair vehicles ahead while moving
     - Can crush infantry

cabapc-name = Hover Transport
cabapc-desc = Light Armored transport.

    Special:
     - Can transport up to 12 infantry or 4 vehicles
     - Hovers (ignores terrain like Veins, Water and Radiations)
     - No longer hovers when disabled by EMP
     - Cannot enter Tunnel Networks or transport, can be carried by carryall

spiderarty-name = Drone Host 
spiderarty-desc = Heavy artillery that deploys drones at the targetted area.

    Good vs: Ground targets

    Special:
     - Minimum attack distance
     - Spawns drones at target
     - Gain 100% EXP from enemy killed by spawned drones
     - Converts killed Infantry/Beast into worker cyborgs
     - Can shoot over walls
     - Can crush infantry
     - Selfrepair

paladin-name = Minotaur
paladin-desc = Advanced walker armed with a twin laser cannon.

    Good vs: Ground targets

    Special:
     - Selfrepair
     - Can shoot over walls
     - Can crush infantry

corruptor-name = Corruptor
corruptor-desc = Anti-personnel walker.

    Good vs Infantry

    Special:
     - Can attack enemies ahead while moving
     - Provides stealth detection
     - Can mutate enemies killed
     - Absorbs essence from killed enemies to heal
     - Attacks emit radiation that harms ground units in the area
     - Immune to Tiberium radiation
     - Can crush infantry

    Upgrades:
     - Vinifera Catalysts

scrmbt-name = Hover Tank
scrmbt-desc = Advanced hover tank armed with a laser cannon.

    Good vs Ground targets

    Special:
     - Hovers (ignores terrain like Veins, Water and Radiations)
     - No longer hovers when disabled by EMP
     - Absorbs essence from killed units to heal
     - Cannot crush infantry
     - Can attack while moving

    Upgrades:
     - Tiberium Conversion

scrscorpion-name = Plague Walker
scrscorpion-desc = Long range assault walker.

    Good vs: Infantry, Vehicle

    Special:
     - Absorbs essence from killed enemies to heal
     - Fog inflicts blinding and poison effect in the area
     - Can crush infantry

    Upgrades:
     - Wasting Disease

scrrecharger-name = Guardian
scrrecharger-desc = Mobile shield generator.

    Special:
     - Generates shields around nearby friendly land vehicles
     - Shields provide additional 150 health points
     - Does not protect against super weapons
     - Absorbs essence from killed enemies to heal
     - Can crush infantry

scrmobmine-name = Subjugator
scrmobmine-desc = Mind Control unit.

    Special:
     - Can take over regular ground units
     - Can control up to three units
     - Gain 50% EXP from enemy killed by controlled unit.
     - Absorbs essence from killed units to heal
     - Cannot mind control heroics, flying units, beasts, structures
     - Can shoot over walls
     - Can mind control units while moving

tripod-name = Annihilator Tripod
tripod-desc = Advanced walker.

    Good vs Ground

    Special:
     - Absorbs essence from killed units to heal
     - Can shoot over walls
     - Can crush infantry

## Aircraft
orcaf-name = Orca Fighter
orcaf-desc = Fast assault gunship with dual missile launchers.

    Good vs: Vehicles, Aircraft

    Special:
     - Can attack air
     - Provides stealth detection when at air
     - Can attack enemies ahead while moving

    Upgrades:
     - Ceramic Plating

orcab-name = Orca Bomber
orcab-desc = Heavy carpet bomber.

    Good vs: Ground targets

    Special:
     - Can attack enemies ahead while moving

    Upgrades:
     - Ceramic Plating

dshp-name = Dropship
orcatran-name = Orca Transport

harpy-name = Harpy
harpy-desc = Scout Helicopter armed with chain guns.

    Good vs: Infantry, Aircraft

    Special:
     - Can attack air
     - Provides stealth detection when at air
     - Can attack enemies from all directions

banshee-name = Banshee Fighter
banshee-desc = Advanced fighter-bomber craft armed with twin plasma cannons.

    Good vs: Vehicles, Aircraft

    Special:
     - Can attack air
     - Can attack enemies ahead while moving

cerberus-name = Paladin Cruiser
cerberus-desc = High tech frigate armed with a mobile stealth generator.

    Good vs: Ground targets

    Special:
     - Cloaks friendly units next to it 
     - Stealth generator can't be used with weapon at the same time
     - Can attack enemies ahead while moving

mutheli-name = Gargoyle
mutheli-desc =  Heavy assault gunship with an auto-cannon.

    Good vs: Vehicles, Aircraft

    Special:
     - Can attack air
     - Provides stealth detection when at air
     - Can attack enemies from all directions

mutqueen-name = Queen
mutqueen-desc = Flying tiberium beast dealing high amount of damage and
    able to spread their brood accross the world.

    Good vs: Vehicles, Buildings

    Special:
     - Lays eggs via deploy which later hatch as crabs that are hostile to anyone
     - Gain 100% EXP from enemy killed by spawned crabs
     - Can attack enemies ahead while moving
     - Can mutate killed Infantry

    Upgrades:
     - Stimulant Infusion

falcon-name = Falcon
falcon-desc = Fast jet that fires 4 high-exlosive rockets.

    Good vs: Ground targets

    Special:
     - Moves at super-sonic speed able to dodge all attacks (except mind control)
     - Moves at half speed for 5 seconds after attacking

    Upgrades:
     - Tiberium Gas Warhead

stormrider-name = Stormrider
stormrider-desc = Scrin scout aircraft.

    Good vs: Vehicles, Aircraft

    Special:
     - Can absorb essence from killed units to heal
     - Can attack enemies from all directions
     - Can attack enemies while moving

drache-name = Scrin Host Station
drache-desc = Deploys into a Host Station.

    Special:
     - Provides build radius for structures when deployed
     - Has increased HP when deployed

glider-name = Glider
glider-desc = Anti-air hover vehicle.

    Good vs Aircraft

    Special:
     - Can attack enemies ahead while moving
     - Can be upgraded to transform into an aerial version that can only engage ground units
     - Aerial Version weapon performs good vs infantry
     - Absorbs essence from killed units to heal
     - Hovers (ignores terrain like Veins, Water and Radiations)
     - No longer hovers when disabled by EMP

    Upgrades:
     - Aerial Gliders

scrdestroyer-name = Destroyer
scrdestroyer-desc = Scrin light frigate designed for long range combat.

    Good vs Ground targets

    Special:
     - Can absorb essence from killed units to heal
     - Can attack enemies ahead while moving

    Upgrades:
     - Disc Barrage
     - Hyper-Flight Rotors

scrtrans-name = Scrin Transport
scrtrans-desc = Scrin transport.

    Special:
     - Can carry up to 3 vehicles or ten soldiers (no epics or nuke truck)

    Upgrades:
     - Hyper-Flight Rotors

scrcarrier-name = Assault Carrier
scrcarrier-desc = Scrin frigate.

    Strong vs Vehicles, Aircraft

    Special:
     - Sends fighters at enemies 
     - Gain 100% EXP from enemy killed by spawned fighters
     - Can attack Air
     - Provides stealth detection when at air
     - Can absorb essence from killed enemies to heal
     - Can attack enemies while moving

    Upgrades:
     - Hyper-Flight Rotors

scrbattleship-name = Battlecruiser
scrbattleship-desc = The main battleship of Scrin and the pride of their fleet.

    Strong vs Ground targets

    Special:
     - Build limit: 1
     - Auto repairs
     - Can absorb essence from killed enemies to heal
     - Can attack enemies from all directions
     - Immune to mind control

wasp-name = Wasp
wasp-desc = Aerial drone armed with dual railgun cannons.

    Good vs: Vehicles, Aircraft

    Special:
     - Can attack air
     - Provides stealth detection when at air
     - Can attack enemies ahead while moving

basilisk-name = Basilisk
basilisk-desc = Light frigate armed with firestorm rockets.

    Good vs: Infantry

    Special:
     - Deals AoE damage
     - Can attack enemies ahead while moving

devourer-name = Devourer
devourer-desc = Heavy and slow siege frigate.

    Good vs: Buildings

    Special:
     - Selfrepairs
     - Slows enemy units with it's attack

wyvern-name = Wyvern
repairwyvern-name = Repair Drone?
cabdronejet-name = Imp
repairdrone-name = Repair Drone

## Campaign Unit
buggy-e1-name = Anti Infantry Buggy
buggy-rocket-name = Rocket Buggy
buggy-laser-name = Laser Buggy
buggy-flame-name = Flame Buggy
struckfull-name = Filled Battle Bus
nukecarryall-desc = AI Nuke Transport
sonicarryall-desc = AI Sonic Transport
apcarryall-desc = AI APC Transport
mubunkrfull-desc = Filled Bunker

ghost-name = Gil the Ghost Stalker
ghost-desc =  Gil is the elite commando of Mutants, if not GDI.
    He is armed with a ion railgun and C4.

    Good vs Ground

    Special:
     - Maximum Supply of 1
     - Demolishes structures with C4
     - Heals on Tiberium fields
     - Immune to mindcontrol
     - Crush class: crushable

umagon-name = Umagon
umagon-desc = Umagon was one of the GDI's "good girls", she sure was. 
    She is armed with a sniper with silencer and C4.

    Good vs Infantry

    Special:
     - Maximum Supply of 1
     - Demolishes structures with C4
     - Heals on Tiberium fields
     - Immune to mindcontrol
     - Crush class: crushable

mujeep-name = Technical
mujeep-desc = Infantry transport.

    Good vs: Infantry.

    Special:
     - Can transport 3 infantry units
     - Can attack while moving

avatar-name = Avatar
avatar-desc = Heavy Walker armed with a strong laser and enhances nearby Units.

    Good vs: Ground

    Special:
     - Can crush infantry
     - Can shoot over wall

weasau3-name = Green Weasaurus
weasau3-desc = Some long forgotten creature trained by mutant.

    Good vs: Ground

    Special:
     - Selfhealing up to 100%
     - Wild mind: Mind only controllable by MasterMind
     - Can shoot over walls
     - Can crush Infantry 
     - Heals on Tiberium 
     - E.M.P. Immune
     - Deployability: use melee attack for a short period

lpst-name = Sensor Array
lpst-desc = Support unit for large stealth detection.

    Special:
     - Deploy to reveal stealth unit in a large area

sgen-name = Mobile Stealth Generator
sgen-desc = Support unit that projects a cloaking field.

    Special:
     - Cloak nearby unit when deploy

icbm-name = Ballistic Missile Launcher
icbm-desc = Even ICBM is deployed as the tension increased.

    Special:
     - Deploy to gain ICBM strike abiility

mwar-name = Mobile War Factory
mwar-desc = Support unit can deploy into warfactory.

    Special:
     - Can deploy into a factory to produce vehicle.
     - Needs Nod's War Factory to unlock basic vehicle.
     - Can crush infantry

memp-name = Mobile EMP
memp-desc = Support unit can generate emp pulse.

    Special:
     - Deploy to generate emp pulse to disable all ground machines in the area
         - For example: Buildings, Defences, Vehicles and Cyborgs
     - Immumes to EMP
     - Can crush Infantry

cabecm-name = Hacker Drone
cabecm-desc = Support unit can recon and infiltrate.

    Special:
     - Force fire any location: Send the Quadrotor to recon
     - Send the Quadrotor to hack hackable.
     - Capture building without consumed.
     - Can crush Infantry
qdrone-name = Quadrotor

mutmound-name = Suspicious Mound
mutambush-name = Suspicious Mound (hide a tunnel under it, and keep unit in tunnel living)
mutambushvent-name = Tunnel (generated by Suspicious Mound)

gbeacon-name = DropShip Beacon
gaiontur-name = Ion Turret
kodk-name = Kodiak Command Ship
cabgrinder-name = Stasis Chamber
cabgrinder-desc = Allows to process units into credits.

fabricator-name = Civilian Fabricator

containmentunit-name = Containment Unit

coredefender-name = Core Defender
coredefender-desc = C.A.B.A.L.'s final creation armed with crazy weapon.

    Good vs: Everything

    Special:
     - Needs preparing before rising.
     - Selfrepair
     - Can shoot over walls
     - Can crush vehicle
     - Immune to mindcontrol
     - Cannot be teleported by wormhole

hmectest-name = Mammoth Mk.III
hmectest-desc =  GDI's final creation for the war when everything is doomed.

    Good vs: Everything

    Special:
     - Build limit: 1
     - Selfhealing up to 100%
     - Can attack air
     - Immune to mind control
     - Can shoot over walls
     - Can crush everything except other Epics
     - It is deployed via dropship at the Dropship Bay
     - Can attack while moving
     - Can spawn 2 gun drones
     - Gain 100% EXP from enemy killed by spawned gun drones.
     - Cannot be teleported by wormhole

mkiiidrone-name = Minigun Drone

scrincouncil-name = Scrin Council
scrincouncil-desc = A council debates on the world's corpse.

    Good vs: Everything

    Special:
     - Build limit: 1
     - Selfhealing up to 100%
     - Can attack air
     - Immune to mind control
     - Can shoot over walls
     - Can attack while moving

councilor-name = Councilor

## Tech Building
cahosp-name = Civilian Hospital
cahosp-desc = Provides global healing for infantry.

well-name = Tiberium Extractor
well-desc = Provides additional funds.

well-mw-name = Megawealth Only Tiberium Extractor
well-nomw-name = Non-Megawealth Only Tiberium Extractor

machineshop-name = Machine Shop
machineshop-desc = Provides global repairs for vehicles.

bloodderrick-name = Cravicus Containment
bloodderrick-desc = Spawns hostile crabs when destroyed.

neutralradar-name = Scout Array
neutralradar-desc = Provides vision.

neutralpowerplant-name = Nuclear Power Plant
neutralpowerplant-desc = Provides power & explodes violently when destroyed.

scrinreinfpad-name = Scrin Replicator
scrinreinfpad-desc = Builds a main battle tank periodically.

neutralsonictur-name = Shockwave Turret
neutralsonictur-desc = Damages units in a straight line.

cabcannon-name = Cannon Turret
repairtur-name = Maintenance Turret
bluetibbarrel-name = Blue Tiberium Barrel

fueltower-name = Fuel Tower
fueltower-desc = Causes huge explosion when destroyed.

civheli-name = Civilian Helipad
civheli-desc = Rearms aircraft.

caarmr-name = Civilian Armory
caarmr-desc = Increases global experience gain.

gaspot-name = Light Tower

flagdom-name = Domination Flag
flagdom-desc = Capture for Domination point.
flagdom-name2 = Domination Mode Only Flag

ntpyra-koth-name = Nod Pyramid (Contains Tacitus)
ntpyra-koth-desc = Capture and hold for 6 minutes to win.
ntpyra-koth-name2 = KOTH Mode Only Pyramid

tacnst-name = Tech Construction Yard
tacnst-desc = Builds tech structures.

## Civilian
weedguy-name = Chem Spray Infantry
weedeater-name = Weed Eater
chamspy-name = Chameleon Spy
mutantguy1-name = Mutant Soldier
mutantguy2-name = Mutant Sergeant
tratos-name = Tratos
oxanna-name = Oxanna
slavik-name = Slavik
c4tnk-name = Civilian Tank
truck-name = Truck
bus-name = Bus
car2-name = Automobile
carbus1-name = School Bus
pickup-name = Pickup
excavator-name = Excavator
wini-name = Recreational Vehicle
van-name = Van
ftruck-name = Fuel Truck
aban01-name = WS Logging Company
aban02-name = Panullo Haciendas
aban03-name = Abandoned Factory
aban04-name = City Hall
aban05-name = Hunting Lodge
aban06-name = Local Inn & Lodging
aban07-name = Church
aban08-name = Abandoned Warehouse
aban09-name = Tall's Residence
aban10-name = Denzil's Last Chance Motel
aban11-name = Miele Manor
aban12-name = Kettler's Places
aban13-name = Long's Home
aban14-name = Local Store
aban15-name = Adam's House
aban16-name = Gas Station
aban17-name = Gas Pumps
aban18-name = Gas Station Sign
fountain-name = Fountain
ammocrat-name = Ammo Crates
bboard01-name = Eat at Rade's Roadhouse
bboard02-name = Drink YEO-CA Cola!
bboard03-name = Hamburgers $.99
bboard04-name = Visit Scenic Las Vegas
bboard05-name = Rooms $29 a nite
bboard06-name = Kaspm's Tiberium Warhouse
bboard07-name = Alkaline's Battery Superstore
bboard08-name = Alex-gators petshop just ahead!
bboard09-name = TacticX games rock!
bboard10-name = WW Surf and Turf hits the spot!
bboard11-name = Only 11 miles to Zydeko's cafe!
bboard12-name = No escape from Archer's Asylum!
bboard13-name = Stop in at Hewitt's hair salon
bboard14-name = Billy Bob's Harvester school
bboard15-name = Pannullo's hacienda es bueno
bboard16-name = Join GDI: We save lives.
bboard17-name = TibeRUM, Mutants love it!!
bboard18-name = Drinkable Water!
bboard19-name = Get Tibbux: Charm Girls
ca0001-name = Rade's Roadhouse
ca0002-name = Sandberg and Son's
ca0003-name = Temp Housing
ca0004-name = Waystation
ca0005-name = Ferbie's 4 Sale
ca0006-name = Deluxe Accomodations
ca0007-name = Field Generator
ca0008-name = Subterranean Dwelling
ca0009-name = Subterranean Dwelling
ca0010-name = Leary Traveller Inn
ca0011-name = Water Tank
ca0012-name = Greenhouse
ca0013-name = Water Purifier
ca0014-name = Observation Tower
ca0015-name = Port-A-Shack
ca0016-name = Port-A-Shack Deluxe
ca0017-name = Energy Transformer
ca0018-name = Solar Panel
caaray-name = Civilian Array
flag-name = Flag
capyr01-name = Pyramid
city01-name = Connelly Court Apts.
city02-name = Lightner's Luxury Suites
city03-name = Office Building
city04-name = Westwood Stock Exchange
city05-name = Daily Sun Times
city06-name = YEO-CA Cola Corp.
city07-name = Urban Housing
city08-name = Yee's Discount Liquor
city09-name = Abandoned Warehouse
city10-name = Urban Storefront
city11-name = Ambrose Lounge
city12-name = Bostic Tower
city13-name = Hewitt Hair Salon
city14-name = Business Offices
city15-name = 2nd National Bank
city16-name = Highrise Hotel
city17-name = The Projects
city18-name = Archer Asylum
city19-name = Fill'er Up-Pump'N'Go
city20-name = Gas Pump
city21-name = Gas Station Sign
city22-name = Church
cpwrp-name = Power Station
city35-name = Apartment Complex
carpub-name = Pub
apartment-name = Apartment
watertower-name = Water Tower
gaoldcc1-name = Old Construction Yard
gaoldcc2-name = Old Temple
gaoldcc3-name = Old Weapons Factory
gaoldcc4-name = Old Refinery
gaoldcc5-name = Old Advanced Power Plant
gaoldcc6-name = Old Silos

gasand-name = Sandbags
gasand-desc = Stops infantry and light vehicles.
    Can be crushed by tanks.

radiotower-name = Radio Tower
signaltower-name = Communications Tower
container-name = Container
tent-name = Military Tent
civdock-name = Civilian Dock
cargoship01-name = Cargo Ship
fishingship01-name = Fishing Ship
gdihyperion-name = Hyperion Patrol Ship
ctdam-name = Hydroelectric Dam
ctvega-name = Vega's Pyramid
namntk-name = Nod Montauk
ntpyra-name = Nod Pyramid
ufo-name = Scrin Ship
c_kodiak-name = Kodiak Crash
mustable-name = Training Grounds
tdadvgtwr-name = Advanced Guard Tower
advhq-name = Advanced Communications Center
tdadvpp-name = Advanced Power Plant
tdairstrip-name = Airstrip
tdbarr-name = Barracks
tdgtwr-name = Guard Tower
tdnhand-name = Hand of Nod
tdntpl-name = Temple of Nod
tdhelipad-name = Helipad
tdntur-name = Gun Turret
tdobelisk-name = Obelisk of Light
tdrefinery-name = Refinery
tdrepd-name = Service Depot
tdsamsite-name = SAM Site
tdpp-name = Power Plant
tdsilo-name = Silo
tdwf-name = Weapons Factory

## Creep
doggie-name = Tiberian Fiend
visc_sml-name = Baby Visceroid
visc_lrg-name = Adult Visceroid
zombie-name = Haunt
berserker-name = Berserker
eggs-name = Cravicus Eggs
crab-name = Tiberian Cravicus
pcrab-name = Tiberian Cravicus
jfish-name = Floater
jfish-desc  = Hovering amphibious lifeform attacks with electrical tentacles.

    Good vs Ground targets

    Special:
     - Hovers (ignores terrain like Veins, Water and Radiations)
     - Heals on Tiberium fields and Green Tiberium Radiation
     - Can't hover when Attack.
     - Attack with EMP : Disable ground machines for a short time
         - For example: Buildings, Defences, Vehicles and Cyborgs
     - Wild mind: Mind only controllable by MasterMind

minivein-name = Baby Veinhole
veinhole-name = Veinhole Monster

## Decoration
rocks-name = Rock set (Supports facings)
pallet-name = Pallet
pallet-name2 = Pallet (Supports facings)
boxes-name = Boxes
boxes-name2 = Boxes (Supports facings)
grave-name = Grave
grave-name2 = Grave (Supports facings)
pipes-name = Pipes
pipes-name2 = Pipes (Supports facings)
streetsign-name = Street Sign
streetsign-name2 = Street Sign (Supports facings)
trafficlights-name = Traffic Light
trafficlights-name2 = Traffic Light (Supports facings)
bench-name = Bench
bench-name2 = Bench (Supports facings)
busstop-name = Bus Stop
busstop-name2 = Bus Stop (Supports facings)
scrap-name = Scrap
scrap-name2 = Scrap (Supports facings)

tankbarrier-name = Tank Barrier
concretebarrier-name = Civilian Barrier
fence-name = Fence
powerline-name = Power Line
trashdump-name = Trash Dump
bigblue-name = Vinifera Monolith
biggreen-name = Riparius Monolith
fona-name = Fona
box-name = Boxe
drum-name = Drum
lightpost-name = Light Post
constructionbarrier-name = Construction Barrier
campfire-name = Campfire
cacrsh01-name = Crash Sites
geye-name = Curiosity
weye-name2 = ...(Normal terrain water only)
snoweye-name2 = ...(Snow terrain water only)
fairy-name = Fairy

## Infantry
gdie1-name = Marine
gdie1-desc = Basic combat infantry.

    Good vs: Infantry

    Special:
     - Crush class: crushable

    Upgrades:
     - AP Ammunition
     - Nanofiber Vests

grenadier-name = Phalanx
grenadier-desc = Basic anti armor infantry.

    Good vs: Vehicles, Aircraft

    Special:
     - Can attack Air
     - Can shoot over walls
     - Crush class: crushable

    Upgrades:
     - Nanofiber Vests

medic-name = Medic
medic-desc = Support infantry capable of healing other infantry.

    Special:
     - Unarmed
     - Crush class: crushable

e2-name = Disc Thrower
e2-desc = Infantry armed with special explosive discs.

    Good vs: Infantry, Structures.

    Special:
     - Can throw grenades over walls
     - Crush class: crushable

    Upgrades:
     - Nanofiber Vests

jumpjet-name = Jumpjet Infantry
jumpjet-desc = Soldiers with jetpacks armed with a grenade launcher.

    Good vs: Infantry, Structures.

    Special:
     - Can fly
     - Will land when ordered to deploy
     - Can shoot over walls
     - Crush class: crushable

eagleguard-name = Eagle Guard
eagleguard-desc = Elite Soldier armed with an EMP rifle.

    Good vs: Vehicles

    Special:
     - Immune to Tiberium fields
     - Attack with EMP : Disable ground machines for a short time
         - For example: Buildings, Defences, Vehicles and Cyborgs
     - Crush class: uncrushable

jjcomm-name = Jumpjet Commando
jjcomm-desc = GDI's commando unit equipped with a jetpack.
    Armed with a Personal Ion Cannon and orbital bombardment beacon.

    Good vs: Ground

    Special:
     - Will fly when ordered to deploy
     - Immune to mind control
     - Build limit: 1
     - Crush class: crushable

altnode1-name = Militant
altnode1-desc = Nod's light infantry variant.

    Good vs: Infantry

    Special:
     - Does not go prone after taking fire
     - Crush class: crushable

crusader-name = Crusader
crusader-desc = Anti-Armor infantry.

    Good vs: Vehicles, Aircraft

    Special:
     - Can attack Air
     - Can shoot while moving
     - Does not go prone after taking fire
     - Can shoot over walls
     - Crush class: crushable

templar-name = Templar
templar-desc = Warrior resurrected regenesis prototype armed with a flamethrower.

    Good vs: Infantry, Buildings

    Special:
     - Does not go prone after taking fire
     - Immune to Tiberium fields
     - Crush class: crushable

    Upgrades:
     - Purifying Flame

nconf-name = Black Hand Trooper
nconf-desc = Nod's Special Forces armed with firefly laser rifles and a personal cloaking device.

    Good vs: Ground targets

    Special:
     - Spawns holograms when ordered to deploy
     - Cloaked
     - Crush class: crushable

bhs-name = Toxin Commando
bhs-desc = Kane��s Elite Commando armed with a toxin sniper rifle
    and a personal cloaking device.

    Good vs: Infantry, Buildings.

    Special:
     - Build limit: 1
     - Immune to Tiberium fields
     - Demolishes structures with C4
     - Bullets release toxins which slow down groups of enemies
     - Cloaks when stationary and not firing
     - Immune to mind control
     - Crush class: crushable

flamehologram-name2 = Militant Hollogram

marauder-name = Marauder
marauder-desc = Wasteland soldiers armed with shotguns.

    Good vs: Infantry

    Special:
     - Heals on Tiberium fields
     - Can deploy to increase health and attackrange
     - Crush class: crushable

    Upgrades:
     - Fortified Barricades

mutfiend-name = Tiberian Fiend
mutfiend-desc = Tiberium beasts trained to take out enemies with tiberium shards.

    Good vs: Vehicles, Aircraft

    Special:
     - Heals on Tiberium fields
     - Can attack Air
     - Crush class: crushable
     - Wild mind: Mind only controllable by MasterMind

    Upgrades:
     - Blue Shards
     - Stimulant Infusion

cutman-name = Mutant Engineer
cutman-desc =  Support infantry.

    Good vs: Buildings

    Special:
     - Can capture neutral and enemy buildings
     - Can repair buildings and bridges
     - Crush class: crushable
     - Heals on Tiberium fields

e3-name = Skirmisher
e3-desc =  Siege militia armed with molotov mortars.

    Good vs: Infantry, Buildings

    Special:
     - Heals on Tiberium fields
     - High attack range
     - Has a minimum attack distance
     - Can shoot over walls
     - Crush class: crushable

    Upgrades:
     - Tiberium Gas Warhead

seer-name = Tyrant
seer-desc = Mutant infantry capable of channeling psychic energy waves and
    increasing the efficiency of nearby friendly units.

    Good vs: Vehicles, Aircraft

    Special:
     - Heals on Tiberium fields
     - Can persuade friendly units to fight harder when deployed
     - Affected units will gain 20% extra attack and movement speed but receive 20% more damage
     - Effect lasts 15 seconds
     - Can attack air
     - Can shoot over walls
     - Crush class: crushable

psyker-name = Lyra the Storm Caller
psyker-desc = Forgotten Commando born with high level psychic powers.
    She is capable of manipulating the presence of tiberium based gas
    in the atmosphere triggering an Ion Storm.

    Good vs: Ground targets

    Special:
     - Build limit: 1
     - Heals on Tiberium fields
     - Long Range and Strong AoE
     - Can shoot over walls
     - Immune to mind control 
     - Crush class: crushable

shark-name = Razorshark
shark-desc = Melee creature that shreds enemies.

    Good vs Infantry

    Special:
     - Absorbs essence from killed units to heal
     - Crush class: crushable
     - Does not receive damage from tiberium fields

legion-name = Legionnaire
legion-desc = Frontline anti-vehicle trooper.

    Good vs Vehicles, Buildings, Aircraft

    Special:
     - Absorbs essence from killed units to heal
     - Can shoot over walls
     - Crush class: crushable
     - Does not receive damage from tiberium fields

shapeshifter-name = Shapeshifter
shapeshifter-desc = Support infantry.

    Good vs: Buildings

    Special:
     - Can capture neutral and enemy structures
     - Can repair structures and bridges
     - Crush class: crushable
     - Does not receive damage from tiberium fields

float-name = Essence Collector
float-desc = Support unit that steals essence from enemy units.

    Special:
     - Absorbs essence from killed enemies to heal
     - Hovers (ignores terrain like Veins, Water and Radiations)
     - No longer hovers when disabled by Web
     - Attacks steal essence to heal friendly units but deal no damage
     - Crush class: uncrushable

bug-name = Plague Trooper
bug-desc =  Alien breed that poisons enemies.

    Good vs Ground targets

    Special:
     - Units hit will become poisoned
     - Poisoned units take damage over time
     - Projectile bouncea a second time
     - Can shoot over walls
     - Crush class: uncrushable
     - Does not receive damage from tiberium fields
     - Absorbs essence from killed enemies to heal

    Upgrades:
     - Wasting Disease

colossus-name = Colossus
colossus-desc = Heavy duty alien.

    Good vs Infantry

    Special:
     - Can irradiate its surroundings when deployed
     - Radiation harms all units regardless of owner
     - Can absorb essence from killed units to heal
     - Can shoot over walls
     - Crush class: uncrushable
     - Does not receive damage from tiberium fields and all radiations

mastermind-name = Mastermind
mastermind-desc = The commando unit of Scrin born with the ability
    to mind control everyone and everything.

    Special:
     - Can mind control everything but Commandos and Epics
     - Gain 50% EXP from enemy killed by controlled unit/defence
     - Takes some time to mind control Structures
     - Absorbs essence from killed units to heal
     - Can teleport over a short distance
     - Can mind control over walls
     - Immune to mind control
     - Does not receive damage from tiberium fields
     - Build limit: 1
     - Crush class: uncrushable

cabsentry-name = Sentry Drone

cyborg-name = Cyborg Infantry
cyborg-desc = Durable cyborg construct.

    Good vs: Infantry

    Special:
     - Can be deactivated with E.M.P.
     - Does not receive damage from tiberium fields
     - Loses its legs on critical health
     - Crush class: uncrushable

    Upgrades:
     - Cybernetic Leg Enhancements
     - Regenerative Materials

cborg-name = Missile Cyborg
cborg-desc = Anti-Armor cyborg.

    Good vs: Vehicles, Aircraft

    Special:
     - Can be deactivated with E.M.P.
     - Can attack Air
     - Does not receive damage from tiberium fields
     - Loses its legs on critical health
     - Can shoot over walls
     - Crush class: uncrushable

    Upgrades:
     - Cybernetic Leg Enhancements
     - Regenerative Materials

swarmling-name = Swarmling
swarmling-desc = Support infantry.

    Good vs: Buildings

    Special:
     - Can capture neutral and enemy structures
     - Can repair structures and bridges
     - Crush class: crushable
     - Does not receive damage from tiberium fields

    Upgrades:
     - Regenerative Materials

pdrone-name = Reclaimer
pdrone-desc = Hovered melee drone programmed to kill enemy infantry.

    Special:
     - Can only attack infantry
     - Turns killed infantry into worker cyborgs
     - Hovers (ignores terrain like Veins, Water and Radiations)
     - No longer hovers when disabled by EMP or Web
     - Crush class: uncrushable

    Upgrades:
     - Reclaim and Recycle
     - Regenerative Materials

glad-name = Gladiator
glad-desc = Advanced cyborg construct, extremely resilient.

    Good vs: Vehicles

    Special:
     - Can be deactivated with E.M.P.
     - Does not receive damage from tiberium fields
     - Loses its legs on critical health
     - Crush class: uncrushable

    Upgrades:
     - Cybernetic Leg Enhancements
     - Regenerative Materials

moth-name = Abductor
moth-desc = High tech ambusher drone.

    Good vs: Infantry, Buildings

    Special:
     - Needs to Deploy to attack
     - Cloaked when deployed
     - Turns enemy units into worker cyborgs
     - Hovers (ignores terrain like Veins, Water and Radiations)
     - No longer hovers when disabled by EMP or Web
     - Attacks travel beneath walls
     - Crush class: uncrushable
     - Cannot be teleported by wormhole when deployed

    Upgrades:
     - Regenerative Materials

cyc-name = Cyborg Commando
cyc-desc =  Elite cyborg armed with a plasma cannon.

    Good vs: Ground targets

    Special:
     - Can be deactivated with E.M.P.
     - Does not receive damage from tiberium fields
     - Loses its legs when at critical health
     - Build limit: 1
     - Crush class: uncrushable
     - Immune to mind control

    Upgrades:
     - Cybernetic Leg Enhancements
     - Regenerative Materials

worker-name = Worker Unit
cybdog-name = Cyberdog
chusk-name = Husk
workermech-name = Worker Drone
nanos-name = Small Nano Swarm

## Upgrades
ap_ammunition-name = AP Ammunition
ap_ammunition-desc = Increases the damage of the following assets by 50%:
    - Vulcan Tower
    - Marine
    - Wolverine
    - Amphibious APC
    Note: Garrisoned infantry needs to redeploy to recieve this upgrade!

nanofiber_vests-name = Nanofiber Vests
nanofiber_vests-desc = Increases the durability of following units by 20%:
    - Marine
    - Phalanx
    - Disc Thrower

power_turbines-name = Power Turbines
power_turbines-desc = Doubles the power output of all GDI Power Plants.

sonic_emitter_protocol-name = Sonic Emitter Protocols
sonic_emitter_protocol-desc = Enables construction of Sonic Emitter defence structure.

railguns-name = Railgun Barrels
railguns-desc = Provides Titans and Mammoth Tanks with railguns increasing their damage by 50%.

ceramic_plating-name = Ceramic Plating
ceramic_plating-desc = Increases the durability of GDI Aircraft by 20%.

purifying_flame-name = Purifying Flame
purifying_flame-desc = Increases the damage of flamethrower weapons by 50%.
    Note: Garrisoned infantry needs to redeploy to recieve this upgrade!

raider_passenger-name = Raider Passenger
raider_passenger-desc = Raider Buggies start with a Militant in them.
    Existing empty Raider Buggies gain a Militant.

explosive_mixtures-name = Deadly Mixtures
explosive_mixtures-desc = Increases the damage of Demo Bikes by 50%.

improved_stealth_generator-name = Improved Stealth Generator
improved_stealth_generator-desc = Enables Stealth Generators to cloak themselves.

tib_core_missiles-name = Tiberium Core Missiles
tib_core_missiles-desc = Increases damage and projectile speed of the following assets by 50%:
    - Attack Bike
    - SAM Site
    - Stealth Tank

laser_capacitors-name = Laser Capacitors
laser_capacitors-desc = Provides Tick Tanks with a Laser Cannon that deals full damage against Infantry and hits targets instantly.

fortified_upg-name = Fortified Barricades
fortified_upg-desc = Depoloyed Marauders become uncrushable by anything.

blue_shards-name = Blue Shards
blue_shards-desc = Increases damage of Mutant Fiends by 25%.
    Note: Garrisoned infantry needs to redeploy to recieve this upgrade!

lynx_rockets-name = Lynx Rockets
lynx_rockets-desc = Provides Lynx Tanks with a secondary rocket launcher for additional firepower.

tunnel_repairs-name = Tunnel Repairs
tunnel_repairs-desc = Enables Tunnel Networks to heal infantry and repair vehicles inside them.

tiberium_infusion-name = Stimulant Infusion
tiberium_infusion-desc = Increases the durability and speed of following units by 20% and 15 respectively and gives them self-healing:
    - Tiberium Fiend
    - Carnotaurus
    - Queen
    - Ravager
    Note: Garrisoned infantry needs to redeploy to recieve this upgrade!

tiberium_gas_warheads-name = Tiberium Gas Warhead
tiberium_gas_warheads-desc = The weapons of following units generate radiation and generate large explosion upon several hits at the same place in a short time:
    - Skirmisher (5 to ensure explosion)
    - Blighter MLRS (2 to ensure explosion)
    - Falcon (1 can ensure explosion)
    Note: Garrisoned infantry needs to redeploy to recieve this upgrade!

cybernetic_leg_enhancements-name = Cybernetic Leg Enhancements
cybernetic_leg_enhancements-desc = Increases the speed of following units by 30:
    - Cyborg
    - Missile Cyborg
    - Gladiator
    - Cyborg Commando
    Note: Speed increase does not apply when the units are on critical health.

improved_reaper_nets-name = Paralyzing Reaper Nets
improved_reaper_nets-desc = Cyborg Reaper nets last 50% longer against Infantry.

limpet_aa_targeting-name = Limpet AA Missile
limpet_aa_targeting-desc = Enables Limpet Drones to target air units.

reclaim_and_recycle-name = Reclaim and Recycle
reclaim_and_recycle-desc = Infantry killed by Reclaimers turn into Cyborgs with 15% health.
    Increases durability of Reclaimers by 50%.

regenerative_materials-name = Regenerative Materials
regenerative_materials-desc = Enables all C.A.B.A.L Infantry units to repair themselves, even during combat.
    Note: Garrisoned infantry needs to redeploy to recieve this upgrade!

gatling_cannons-name = Gatling Cannons
gatling_cannons-desc = Centurions fire faster as they continue firing.

vinifera_catalysts-name = Vinifera Catalysts
vinifera_catalysts-desc = Increates range of Ichor Waste Turrets and Corruptors by 1 cell.
    Doubles the damage of the tib-radiation left by those assets.

tiberium_conversion-name = Tiberium Conversion
tiberium_conversion-desc = Enables Hover Tanks to absorb Tiberium to increase their damage for the next 3 attacks by 50%.
    - If not MegaWealth: Deploy the Hover Tank to absorb nearby Tiberium.
    - If MegaWealth: Target Tiberium Extractors with Hover Tanks to gain the effect, Extractors will shutdown for a short time.
    
aerial_gliders-name = Aerial Gliders
aerial_gliders-desc = Enables Gliders to be deployed to take flight.
    While in air they are able to attack ground targets.

improved_plague_gas-name = Wasting Disease
improved_plague_gas-desc = Enables Plague Troopers and Plague Walkers to slow down affected enemy units by 50%.

disc_barrage-name = Disc Barrage
disc_barrage-desc = Changes the weapon of the Destroyer to fire a burst of 4 smaller projectiles effectively doubling its damage output.

hyper_flight_rotors-name = Hyper-Flight Rotors
hyper_flight_rotors-desc = Increases the speed of the following units by 30:
    - Destroyer
    - Scrin Transport
    - Assault Carrier

## Misc
mpspawn-name2 = (multiplayer player starting point)
waypoint-name2 = (waypoint for scripted behavior)
minelaypoint-name2 = (Used for initialize MinelayerBotModule, when you need minelayer to lay mine more precisely)
flarenotifier-name2 = Red Flare
tibambientsounddummy-name2 = (waypoint for scripted behavior)
cashnotifier-name2 = (waypoint for scripted behavior)
camera-name2 = (reveals area to owner)
scamera-name2 = (reveals area to owner and detect stealth unit)
ingalite-name2 = (Invisible Light Post)
inpurplamp-name2 = (Invisible Purple Light Post)
neglamp-name2 = (Invisible Negative Light Post)
inoranlamp-name2 = (Invisible Orange Light Post)
ingrnlmp-name2 = (Invisible Green Light Post)
indgrnlmp-name2 = (Invisible deep Green Light Post)
inredlmp-name2 = (Invisible Red Light Post)
inblulmp-name2 = (Invisible Blue Light Post)
inyelwlamp-name2 = (Invisible Yellow Light Post)
colorpicker-name2 = Color picker unit (Do not place in editor)
moneycrate-name = Cash Crate (Provides $2000)
veterancycrate-name = Veterancy Crate (Increase a unit's rank by 1)
healcrate-name = Supplies Of Healing (Heals all units)
buffcrate-name = Upgrade Kit (Give a random buff to a unit)
unitcrate-name = Unit Crate (Give a unit)
galite-name = Light Post
redlamp-name = Red Light Post
negred-name = Negative Red Light Post
grenlamp-name = Green Light Post
bluelamp-name = Blue Light Post
yelwlamp-name = Yellow Light Post
purplamp-name = Purple Light Post

## Ency
blossomtree-ency = Blossom trees were one of the earliest mutated spieces by Tiberium. Normal tree is mutated and develop a fleshly stalk, with a pulsating bulb periodically releasing Tiberium spores to the surrounding area. When these spores land on the ground, a new Tiberium field will begin forming.
    Blossom trees could spread Riparius Tiberium (green Tiberium) at an alarming rate, which is important for those who harvest Tiberium.
bigblue-ency = Vinifera Monolith is a cystal collection consist of many fast growing long crystals, which will fracture periodically. When they fracture, the cystal shards will seed into soil and a new Tiberium field will begin forming.
    Vinifera Monolith could spread Vinifera Tiberium (blue Tiberium) at an alarming rate, which is important for those who harvest Tiberium.
biggreen-ency = Just like Vinifera Monolith, but they spread and made from long crystals of Riparius Tiberium (green Tiberium).
fona01-ency = Mutated from some kinds of cactus, glows with blue light.
fona07-ency = Mutated from some kinds of lichen, glows with green light.
fona11-ency = Mutated from some kinds of palm trees, glows with green light.
fona17-ency = Mutated from some kinds of fern, glows with white light.
yellowfona01-ency = Mutated from Amorphophallus titanum, glows with yellow light.
geye-ency = Unknown lifeform. Maybe it is a creature looks like an eyeball, or just an eye from a greater being.
fairy-ency = Those harmless and glowing creatures feed on the Tiberian particulates in the air, and will flee to the sky above if being threated and frightened. They will try to return to their position after a while, just like some species of jellyfish before Tiberium came to earth.
doggie-ency = Tiberian Fiends are large, doglike animals that stand about the size of a large horse or a small elephant, and have blackish-brown or red skin, although that is usually completely covered by Tiberium crystals growing on its back.
visc_sml-ency = Visceroids were disturbing creatures that sometimes form when organic life dies to tiberium exposure. As flesh breaks down after death, it may mutate into a Baby Visceroid, a blob-like masses of flesh.
visc_lrg-ency = When Baby Visceroid survive enough, it will grow into a Adult Visceroid.
    When Adult Visceroid is dead, it splits into 2 Baby Visceroids.
zombie-ency = Haunt were disturbing creatures that sometimes form when organic life dies to tiberium exposure. As corpse being taken over by cystals after death, it may mutate into a wandering, half-corrupted humanoid.
berserker-ency = If a Haunt survives long enough, it will grow into a strong and durable creature -- Berserker.
eggs-ency = Egg of the Tiberian Cravicus, with durable shell. It will hatch into Tiberian Cravicus within hours.
crab-ency = Tiberian Cravicus is a kind of mutated arthropod, has a strong adaptability. It can be found everywhere, even in Veinhole.
jfish-ency = Floater is a mysterious lifeform, it is capable of generating intense electrical charges and toxic Tiberium gas, while using its tentacles tears body and vehicle armors apart.
    Research suggests that it is unlikely mutated from known spieces.
veinhole-ency = At the center of the Veins, a "mouth" like structure can react to basic stimulus. There have been many reports of vehicles being torn apart by the veins, and moved to the central "mouth" and consumed.
bloodderrick-ency = Containment of Tiberian Cravicus, designed by Scrin.
