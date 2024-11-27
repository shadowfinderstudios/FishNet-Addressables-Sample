# FishNet-Addressables-Showcase
A W.I.P. 2D URP showcase for FishNet in Unity created by Shadowfinder Studios.
With 2D navmesh agent support using NavMeshPlus.

This sample showcases different topics, among them:
* Addressables
* Scene loading; global, with addressable loading
* Rideable vehicles; rowboat.
* Mineable resources: rocks.
* Network switchable Character
* Proximity radio using an observable distance condition and trigger
* Shooting arrows
* Taking damage zones (campfire), healing zones, stat hud
* Health, mana and stamina loss/gain and regen
* Death implemented

The sample is set up to work with Unity Multiplayer Play Mode for quick testing.

Instructions to play:

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


TODO: More showcases.

Art and Sound Credits:

Licenses:
https://creativecommons.org/licenses/by/4.0/
https://creativecommons.org/licenses/by-sa/4.0/
https://creativecommons.org/licenses/by-sa/3.0/
https://creativecommons.org/publicdomain/zero/1.0/

LPC Tree Recolors
CCBY By William.Thompsonj and C.Nilsson
https://opengameart.org/content/lpc-tree-recolors

LPC All Seasons Tree
CCBYSA By Death's Darling
https://opengameart.org/content/lpc-all-seasons-apple-tree

Grass Tiles
Public Domain by Invincible
https://opengameart.org/content/grass-tiles-0

LPC Medieval Fantasy Character Sprites
CCBYSA By Wulax
https://opengameart.org/content/lpc-medieval-fantasy-character-sprites

Cosmic Time - Magic Effect
Public Domain By Cethiel
https://opengameart.org/content/cosmic-time-magic-effect

Magic Spell
CC0 By Bastianhallo
https://freesound.org/people/Bastianhallo/sounds/682635/

Awaking of Magic
CCBY3 By Tausdei
https://opengameart.org/content/awaking-of-magic

Knight's Forest Footsteps
Public Domain By Ali_6868 
https://freesound.org/people/Ali_6868/

Pickaxe sounds
CCBY4 By TechspiredMinds
https://freesound.org/people/TechspiredMinds/sounds/728756/

Rocks breaking
CC0 By SoundCollectah
https://freesound.org/people/SoundCollectah/sounds/109360/

RPG Tiles: Cobble Stone Paths & Town Objects
CCBYSA By daneeklu, Jetrel, Hyptosis, Redshrike, Bertram
https://opengameart.org/content/rpg-tiles-cobble-stone-paths-town-objects

Ships with Ripple Effect
CCBY By chabull
https://opengameart.org/content/ships-with-ripple-effect

surfaces_tileset
CC0 By Shadowfinder Studios
https://github.com/shadowfinderstudios/FishNet-Addressables-Showcase

North-South Rowboat modifications
CCBYSA By Shadowfinder Studios
https://opengameart.org/content/lpc-rowboat-topdown-4-directional-recolor-for-rpg

LPC Tree Buttons
CCBY,CCBYSA By Shadowfinder Studios, based on LPC Tree Recolors and LPC All Seasons Tree
https://github.com/shadowfinderstudios/FishNet-Addressables-Showcase
https://opengameart.org/content/lpc-tree-recolors
https://opengameart.org/content/lpc-all-seasons-apple-tree

LPC Pickaxe
CCBYSA Based on art originally by Tuomo Untinen. Adapted by Darkwall LKE.
http://darkwalllke.com/

LPC Rocks
https://opengameart.org/content/lpc-rocks
CCBYSA By bluecarrot16, Johann Charlot, Yar, Hyptosis, Evert, Lanea Zimmerman (Sharm), Guillaume Lecollinet, Richard Kettering (Jetrel), Zachariah Husiar (Zabin), Jetrel, Hyptosis, Redshrike, Rayane Félix (RayaneFLX), Michele Bucelli (Buch)

Retro Stereo
CC0 By inog
https://opengameart.org/content/retro-stereo-sprite

Sword Dialog Box
CCBYSA By Angelee https://artsyangelee.deviantart.com
https://opengameart.org/content/sword-dialog-box

Good Neighbors Pixel Font
CC0 by Clint Bellanger, RamifyArt (otf vector version)
https://opengameart.org/content/good-neighbors-pixel-font

Fight Icons Spritesheet
CC0 By ninja_6734_
https://opengameart.org/content/fight-icons-spritesheet

Jingling Heal
CCBYSA By Zoltan Mihalyi based on Spell Sounds Starter Pack by p0ss https://opengameart.org/content/spell-sounds-starter-pack

Fireplace Sound Loop
CC0 By PagDev
https://opengameart.org/content/fireplace-sound-loop

Fire Animation For RPGs (Finished)
CCBYSA By Zabin and Jetrel
https://opengameart.org/content/camp-fire-animation-for-rpgs-finished

Explosion effects and more
CC0 By Soluna Software
https://opengameart.org/content/explosion-effects-and-more

Fantasy MMORPG HUD & Mobile Controllers modifications (Cleaned up fill image)
CCBY By Shadowfinder Studios, based on AliHamieh's mmorpg hud
https://github.com/shadowfinderstudios/FishNet-Addressables-Showcase
https://opengameart.org/content/fantasy-mmorpg-hud-mobile-controllers

