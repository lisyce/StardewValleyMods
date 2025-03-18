# Author Guide

These docs outline how to add your mod's NPCs, etc. to work with Enemy of the Valley. (Or, to expand upon the storylines of the vanilla NPCs!)

Table of Contents:

- [Adding negative heart dialogue for an NPC](dialogue.md)
- [Adding negative heart events](events.md)
- [Unlocking recipes and perfection](#unlocking-recipes-and-perfection)
- [New Game State Queries](#game-state-queries)

## Game State Queries

TODO

## Unlocking Recipes and Perfection

If your mod has recipes that can only be unlocked upon reaching a high enough friendship level with an NPC, then
players will not be able to unlock that recipe if they decide to become enemies with them. You may want to consider
alternative methods of unlocking recipes. This mod makes it so that all vanilla recipes unlocked only through
NPC friendship become purchaseable at the Saloon upon reaching the corresponding *negative* heart level with that NPC.
For example, the Salad recipe is usually learned from Emily at 3 hearts. Players can instead buy it from the Saloon if
they have -3 (or -4, or -5, etc.) hearts with Emily.