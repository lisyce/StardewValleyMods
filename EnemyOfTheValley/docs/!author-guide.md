# Author Guide

These docs outline how to add your mod's NPCs, etc. to work with Enemy of the Valley. (Or, to expand upon the storylines of the vanilla NPCs!)

Table of Contents:

- [Adding negative heart dialogue for an NPC](dialogue.md)
- [Adding negative heart events](#events)
- [Unlocking recipes and perfection](#unlocking-recipes-and-perfection)
- [New Game State Queries](#game-state-queries)

## Events

This mod introduces a new event precondition: `NegativeFriendship`. It is used identically to the vanilla [Friendship Precondition](https://stardewvalleywiki.com/Modding:Event_data#Current_player): `NegativeFriendship <name> <number>+`.
The precondition is satisfied if the current player has at least as many *negative* friendship points with all of the given NPCs (can specify multiple name/number pairs). So, for example, `NegativeFriendship Sam -500` is satisfied if the
current player has -2 hearts (or -3, or -8, etc.) with Sam.

## Game State Queries

| Condition                      | Returns                                                                                                                                                    |
|--------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `EOTV_PLAYER_NPC_RELATIONSHIP` | Identical behavior to the vanilla `PLAYER_NPC_RELATIONSHIP` GSQ, but also adds support for the `Enemy`, `Archenemy`, and `ExArchenemy` relationship types. |

This mod also sets the following mail flags, which can be used in the `PLAYER_HAS_MAIL` GSQ or anywhere else mail flags are used.
All mail flags listed use the NPC's *internal* name.

- `BarleyZP.EnemyOfTheValley.BeenEnemies_{NPC}`: The player has been enemies with the NPC named `{NPC}` at any point in time.
- `BarleyZP.EnemyOfTheValley.BeenArchenemies_{NPC}`: The player has been archenemies with the NPC named `{NPC}` at any point in time.
- `BarleyZP.EnemyOfTheValley.BeenExArchenemies_{NPC}`: The player has been ex-archenemies with the NPC named `{NPC}` at any point in time.

## Unlocking Recipes and Perfection

If your mod has recipes that can only be unlocked upon reaching a high enough friendship level with an NPC, then
players will not be able to unlock that recipe if they decide to become enemies with them. You may want to consider
alternative methods of unlocking recipes. This mod makes it so that all vanilla recipes unlocked only through
NPC friendship become purchaseable at the Saloon upon reaching the corresponding *negative* heart level with that NPC.
For example, the Salad recipe is usually learned from Emily at 3 hearts. Players can instead buy it from the Saloon if
they have -3 (or -4, or -5, etc.) hearts with Emily.