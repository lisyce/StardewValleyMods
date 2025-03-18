# New Dialogue Options

## TL;DR

- You can now define location-specific and daily dialogue keys with heart levels -2, -4, -6, -8, and -10 (for example, `Mon-2`, `fall_Wed-6`, and `HaleyHouse-10`). Define them in an asset of the format `BarleyZP.EnemyOfTheValley/NegativeHeartDialogue/{NPC}` where `{NPC}` is the NPC's *internal* name.
- If the current player has at least 1 full negative heart with an NPC, then the mod will look for dialogue keys in the custom asset `BarleyZP.EnemyOfTheValley/NegativeHeartDialogue/{NPC}`. If not found, it will default to searching the vanilla `Characters/Dialogue/{NPC}` asset.
- This mod adds additional dialogue keys for things like rejecting mermaid's pendants due to being enemies, apology conversation topics, etc. See below for details.

## Overview

This mod loads dialogue from both the vanilla `Characters/Dialogue/{NPC}` assets AND from special assets of the form
`BarleyZP.EnemyOfTheValley/NegativeHeartDialogue/{NPC}`. When the game tries to load a dialogue key for an NPC, the following
occurs:

1. If the current player does not have at least 1 full negative heart (-250 points) with the given NPC, then vanilla behavior applies.
2. Otherwise, the mod checks for the presence of the relevant key in the appropriate `BarleyZP.EnemyOfTheValley/NegativeHeartDialogue/{NPC}` asset for that NPC. If not found, it searches in the usual `Characters/Dialogue/{NPC}` asset.

If a specific dialogue is not set for an NPC, defaults will be used. Dialogue in a table is shown in order of precedence.
Note that while some keys may work regardless of which asset you define them in, each section has a recommended location you should
use for best practices. As a shorthand, `Vanilla` means the original dialogue asset for an NPC (`Characters/Dialogue/{NPC}`) and
`EOTV` means the Enemy of the Valley negative heart dialogue asset for an NPC (`BarleyZP.EnemyOfTheValley/NegativeHeartDialogue/{NPC}`).

Since this mod uses this scheme for vanilla NPCs, you can check out the content pack folder (\[CP\] EnemyOfTheValley) for examples.

## Location-Specific Dialogue

> Recommended Asset: EOTV

[Location dialogue](https://stardewvalleywiki.com/Modding:Dialogue#Location_dialogue) keys are almost identical to the vanilla ones.
With this mod, the additional heart values of -2, -4, -6, -8, and -10 are checked for keys in the form `<location><hearts>`.

For example, if the player has -5 hearts with Abigail and the game has options of `Saloon8`, `Saloon-2`, and `Saloon-4`, 
it will select `Saloon-4` as the dialogue key to display since the player has at least 4 *negative* hearts with Abigail.
(`Saloon-2` is not chosen since the negative heart levels are checked starting from -10 and ending at -2, so `Saloon-4`
satisfies the conditions first).

## Daily Dialogue

> Recommended Asset: EOTV

[Generic dialogue](https://stardewvalleywiki.com/Modding:Dialogue#Generic_dialogue) or "daily dialogue" keys also function similarly to vanilla ones.
The mod will try to load seasonal dialogue first, followed by non-seasonal. This means you can fall back on the day-of-week keys just like in vanilla.
If you define the keys `Sun-2` through `Sat-2` (for each day of the week), then, regardless of season, this character will have new lines to say if the player
has negative hearts with them without having to fall back on vanilla dialogue.

You may use the additional negative heart levels anywhere they apply. See [Location-Specific Dialogue](#location-specific-dialogue) for further details.

## Becoming Enemies

Shown when an NPC accepts the "A-void Me" Cake and becomes enemies with the player:

| Key               | Description                           | Recommended Asset |
|-------------------|---------------------------------------|-------------------|
| `AcceptEnemyCake` | Default dialogue for becoming enemies | Vanilla           |

Shown when an NPC rejects the "A-void Me" Cake:

| Key                                     | Description                                | Recommended Asset |
|-----------------------------------------|--------------------------------------------|-------------------|
| `RejectEnemyCake_Archenemies`           | The NPC and player are already archenemies | Vanilla           |
| `RejectEnemyCake_ExArchenemies`         | The NPC and player were ex-archenemies     | Vanilla           |
| `RejectEnemyCake_NoNegativeHearts`      | Player has > -1 hearts with the NPC        | Vanilla           |
| `RejectEnemyCake_VeryLowNegativeHearts` | Player has > -4 hearts with the NPC        | Vanilla           |
| `RejectEnemyCake_LowNegativeHearts`     | Player has > -8 hearts with the NPC        | Vanilla           |

## Becoming Archenemies

Shown when an NPC accepts a Shattered Amulet and becomes archenemies with the player:

| Key                     | Description                                              | Recommended Asset |
|-------------------------|----------------------------------------------------------|-------------------|
| `AcceptShatteredAmulet` | Default dialogue for becoming enemies for the first time | Vanilla           |

Shown when an NPC rejects a Shattered Amulet:

| Key                                           | Description                                | Recommended Asset |
|-----------------------------------------------|--------------------------------------------|-------------------|
| `RejectShatteredAmulet_AlreadyArchenemies`    | The NPC and player are already archenemies | Vanilla           |
| `RejectShatteredAmulet_ExArchenemies`         | The NPC and player were ex-archenemies     | Vanilla           |
| `RejectShatteredAmulet_NoNegativeHearts`      | Player has > -1 hearts with the NPC        | Vanilla           |
| `RejectShatteredAmulet_VeryLowNegativeHearts` | Player has > -8 hearts with the NPC        | Vanilla           |
| `RejectShatteredAmulet_LowNegativeHearts`     | Player has > -10 hearts with the NPC       | Vanilla           |

## Misc

| Key                                          | Description                                                                                       | Recommended Asset |
|----------------------------------------------|---------------------------------------------------------------------------------------------------|-------------------|
| `RejectMermaidPendant_NegativeHearts`        | The player offered an NPC a Mermaid's Pendant, but they have negative friendship with that NPC    | Vanilla           |
| `RejectBouquet_NegativeHearts`               | The player offered an NPC a Bouquet, but they have negative friendship with that NPC              | Vanilla           |
| `BarleyZP.EnemyOfTheValley.apologized_{NPC}` | A conversation topic set the morning after the player posts an apology letter to the provided NPC | Vanilla           |