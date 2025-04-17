# Author Guide

These docs outline how to add your mod's NPCs, etc. to work with Enemy of the Valley. (Or, to expand upon the storylines of the vanilla NPCs!)

Table of Contents:

- [Opting in an NPC](#opting-in-an-npc)
- [Adding negative heart dialogue for an NPC](dialogue.md)
- [Adding negative heart events](#events)
- [New Game State Queries](#game-state-queries)
- [Console Commands](#console-commands)
- [Unlocking recipes and perfection](#unlocking-recipes-and-perfection)

## Opting In an NPC

**All NPCs are opted OUT of the negative friendship system by default to respect the wishes of mod authors who may have very specific visions for how their characters should interact with the player.**

If you want to opt a character *in* to the system, you can do so as a [Custom Fields](https://stardewvalleywiki.com/Modding:Common_data_field_types#Custom_fields) entry in the NPC's data. If you are publishing a mod that opts someone else's NPC into the system, please ensure you have permission to do so.

| Field                                                 | Description                                                                                                                                                                                                                                                                          |
|-------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `BarleyZP.EnemyOfTheValley.CanHaveNegativeFriendship` | Opts an NPC into the negative friendship system. The value can be any string. Simply including this field opts the NPC in.                                                                                                                                                           |
| `BarleyZP.EnemyOfTheValley.CanBecomeEnemies`          | Opts an NPC into being able to become enemies, archenemies, and ex-archenemies with the farmer. The value can be any string. Simply including this field opts the NPC in. You should also set the `BarleyZP.EnemyOfTheValley.CanHaveNegativeFriendship` field if you set this field. |

## Events

This mod introduces a new event precondition: `NegativeFriendship`. It is used identically to the vanilla [Friendship Precondition](https://stardewvalleywiki.com/Modding:Event_data#Current_player): `NegativeFriendship <name> <number>+`.
The precondition is satisfied if the current player has at least as many *negative* friendship points with all of the given NPCs (can specify multiple name/number pairs). So, for example, `NegativeFriendship Sam -500` is satisfied if the
current player has -2 hearts (or -3, or -8, etc.) with Sam.

## Game State Queries

| Condition                      | Description                                                                                                                                                |
|--------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `EOTV_PLAYER_NPC_RELATIONSHIP` | Identical behavior to the vanilla `PLAYER_NPC_RELATIONSHIP` GSQ, but also adds support for the `Enemy`, `Archenemy`, and `ExArchenemy` relationship types. |

This mod also sets the following mail flags, which can be used in the `PLAYER_HAS_MAIL` GSQ or anywhere else mail flags are used.
All mail flags listed use the NPC's *internal* name.

- `BarleyZP.EnemyOfTheValley.BeenEnemies_{NPC}`: The player has been enemies with the NPC named `{NPC}` at any point in time.
- `BarleyZP.EnemyOfTheValley.BeenArchenemies_{NPC}`: The player has been archenemies with the NPC named `{NPC}` at any point in time.
- `BarleyZP.EnemyOfTheValley.BeenExArchenemies_{NPC}`: The player has been ex-archenemies with the NPC named `{NPC}` at any point in time.

## Console Commands

| Command                                     | Description                                                                                       |
|---------------------------------------------|---------------------------------------------------------------------------------------------------|
| `EOTV_friendly <NPC name>`                  | Sets the specified NPC to have the 'friendly' relationship with the player (no relationship)      |
| `EOTV_enemy <NPC name>`                     | Sets the specified NPC to be the player's enemy                                                   |
| `EOTV_archenemy <NPC name>`                 | Sets the specified NPC to be the player's archenemy                                               |
| `EOTV_exarchenemy <NPC name>`               | Sets the specified NPC to be the player's ex-archenemy                                            |
| `EOTV_changefriendship <NPC name> <amount>` | Changes the friendship of the provided NPC by the given amount and prints the before/after values |
| `EOTV_maxedfriends`                         | Outputs `Utility::getMaxedFriendshipPercent`                                                      |

## Unlocking Recipes and Perfection

If your mod has recipes that can only be unlocked upon reaching a high enough friendship level with an NPC, then
players will not be able to unlock that recipe if they decide to become enemies with them. You may want to consider
alternative methods of unlocking recipes. This mod makes it so that all vanilla recipes unlocked only through
NPC friendship become purchaseable at the Saloon upon reaching the corresponding *negative* heart level with that NPC.
For example, the Salad recipe is usually learned from Emily at 3 hearts. Players can instead buy it from the Saloon if
they have -3 (or -4, or -5, etc.) hearts with Emily.