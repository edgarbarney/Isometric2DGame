# Isometric 2D Game

This is an unnamed, open-source, 2D, Isometric game "base" project that utilises Unity 6 and C#. It is planned to be a rouge-like (or rouge-lite) system that will be the heart of a story-driven game.

## Features

Here is a summary of implemented features.

- Implemented Player Controller 
  - You can move the player using the default controls, WASD and the arrow keys.
  - You can tweak the player in the inspector to use a smoother movement or instant acceleration.
- Implemented Isometric Camera
  - Isometric player camera is implemented using the Cinemachine package.
  - Camera follows player around smoothly, thanks to the systems provided by Cinemachine.
- Implemented Enemy AI
  - Enemy abilities are implemented using a modular state machine.
  - Enemy abilities can be toggled (Attack, patrol, follow) on and off.
  - If patrol ability is enabled, the enemy will patrol between random given patrol points.
  - If follow ability is enabled, the enemy will chase possible targets within the follow range.
  - If attack ability is enabled, the enemy will attack the target that it was following if the target is within the attack range.
  - If followed target leaves the follow range, the enemy will return to patrolling.
  - Enemies change colour according to their current state.
- Implemented Player Inventory
  - You can use it using the default "I" and "Tab" keys.
  - You can resize player inventory slot count using the Resize() method, and the inventory slots UI will resize accordingly.
    - You can also resize using the inspector, but you should restart the game.
  - Items will be stacked according to the maxStack property of Item classes.
  - Items in the selected slot can be used or dropped to the ground using the buttons in the inventory UI.
  - Items can be inspected to know the name and 
- Implemented Item System
  - The player can pick up items using the default E key. A UI prompt will be shown on the item to indicate that.
  - Items can be picked up by all characters, including players. Player will add the items into their inventory upon pickup, while characters will use them directly upon touch.
  - Item classes can be derived from BaseItem to promote custom behaviour on use.
  - Item types can be directly created as assets using Create > Inventory > Item.
- Implemented Sprite Managers for the Characters
  - Every character, including the player, now turns around to a total of 8 cardinal and ordinal directions. Directions are estimated using movement vectors of characters and input vectors for the player.