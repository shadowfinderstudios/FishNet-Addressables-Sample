# FishNet-Addressables-Showcase

A W.I.P. 2D URP showcase for FishNet in Unity created by Shadowfinder Studios. With 2D navmesh agent support using NavMeshPlus. The sample is set up to work with Unity Multiplayer Play Mode for quick testing.

## Features

This sample showcases different topics, among them:
* Addressables.
* Scene loading; global, with addressable loading.
* Rideable vehicles; rowboat.
* Mineable resources: rocks.
* Network switchable character outfits.
* Proximity radio using an observable distance condition and trigger.
* Shooting arrows.
* Taking damage zones (campfire), healing zones, and stat HUD.
* Health, mana and stamina loss/gain and regen.
* Death implemented.
* A wandering NPC was added and GOAP support for it added

## Showcases

Showcase 1 - Scene loading and addressables

1. Walk to the right to trip the loadable area.

2. When the next scene loads, you can press Spacebar to spawn trees.

3. Trees cannot be spawned in the main scene until you walk into the next scene because that's when the addressables load.

4. Press 1 or 2 or click the tree icons at the bottom of the screen to switch between tree types.

Showcase 2 - Rideables

1. Press E near the boat to ride it and E again to dismount.

2. Move around after using the boat to ride it around.

3. The boat cannot pass thru the ground, but can pass through the water due to tilemap colliders.

Showcase 3 - Mineable Rocks

1. Walk up to a rock.

2. Press E near the rock to swing the pickaxe at it.

3. Keep hitting the rock until it turns to rubble.

4. ???

Showcase 4 - Switchable Character

1. Press the C key to switch the character.

2. Character can now be switched using a SpriteLibrary over the network.

Showcase 5 - Proximity radio

1. The music won't play until you're within distance of the northern radio.

2. If you walk outside the range of the radio it will pause.

3. This is network aware and an observer is used to manage visibility as well.

Showcase 6 - Shooting arrows

1. Press Q to ready your bow.

2. Aim in the direction you want to aim.

3. Point your mouse somewhere on the screen and left click and hold.

4. When ready, release the mouse button to shoot the arrow.

5. Press Q again to holster your bow.

Showcase 7 - Campfire damage zone, healing aura zone

1. Walk into the campfire and take damage, watch health go down

2. Walk into the healing aura and heal, watch health go up faster

3. Stand still outside of either zone and watch health regen slowly

4. Stand in campfire damage zone until health hits zero, watch Death

5. Attempt to move around after death, input is locked

Showcase 8 - Swing pickaxe, shoot arrows, or cast summon tree to affect stats

1. Cast summon tree, or shoot an arrow, or swing your pickaxe

2. Watch stamina go down.

3. Wait and watch stamina regen

Showcase 9 - NPC Wanderer (Now with GOAP support)

1. Watch the wandering NPC run around aimlessly.

2. ???


TODO: More showcases.

Showcase ? - Emotes and animations.
Showcase ? - Interact with chests and generate loot randomly.
Showcase ? - Synchronize a shop and inventory system.
Showcase ? - Item pickup and drop.
Showcase ? - Set up quests.
Showcase ? - Synchronize AI (goap, behavior trees, etc?).
Showcase ? - Pet follower.
Showcase ? - Interact with levers, buttons, switches, pressure plates
Showcase ? - Interact with doors.
Showcase ? - Puzzles (e.g. two players have to use levers or pressure plates, puzzle blocks, etc)
Showcase ? - Area of effect
Showcase ? - Breakable objects (pots, crates, barrels, etc)
Showcase ? - Stealth mechanics (noise detection, player visibility)
Showcase ? - Crafting
Showcase ? - Base building
Showcase ? - Chat (global, guild, local, private) + Chat history
Showcase ? - Friend lists
Showcase ? - Spectating
Showcase ? - PVE Combat
Showcase ? - PVP Combat
Showcase ? - Leaderboards
Showcase ? - Matchmaking
Showcase ? - Character wearables
Showcase ? - Character customization - customizing looks
Showcase ? - Synchronized dynamic weather; rain, snow, lightning, autumn leaves, etc.
Showcase ? - Farming (plowing, seeding, growth, harvesting, carrying, stashing, buying, selling)
Showcase ? - Day / night cycles
Showcase ? - Water sources, filling containers
Showcase ? - Chopping for wood, lighting fires, cooking food
Showcase ? - Reading / writing notes, scrolls, books and tablets
Showcase ? - Anvil and workbench for blacksmith and other crafting
Showcase ? - Guild systems
Showcase ? - Trade systems - auction house
Showcase ? - Economy systems
Showcase ? - Player housing ownership
Showcase ? - Dynamic world events
Showcase ? - NPC Faction system
Showcase ? - War and siege battles
Showcase ? - Alchemy
Showcase ? - Tailoring
Showcase ? - Fishing

## Art and Sound Credits

Please refer to [Art and Sound Credits](CREDITS.md) for more details.

## License

This code is MIT
