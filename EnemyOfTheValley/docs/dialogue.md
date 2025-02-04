# New Dialogue Keys For All NPCs

If a specific dialogue is not set for an NPC, defaults will be used. Dialogue in a table is shown in order of precedence.

## Negative Heart Dialogue

TODO

## Becoming Enemies

Shown when an NPC accepts the "A-void Me" Cake and becomes enemies with the player:

| Key | Description |
| --- | ----------- |
| `AcceptEnemyCake` | Default dialogue for becoming enemies |

Shown when an NPC rejects the "A-void Me" Cake:

| Key | Description |
| --- | ----------- |
| `RejectEnemyCake_Archenemies` | The NPC and player are already archenemies |
| `RejectEnemyCake_ExArchenemies` | The NPC and player were ex-archenemies |
| `RejectEnemyCake_NoNegativeHearts` | Player has > -1 hearts with the NPC |
| `RejectEnemyCake_VeryLowNegativeHearts` | Player has > -4 hearts with the NPC |
| `RejectEnemyCake_LowNegativeHearts` | Player has > -8 hearts with the NPC |

## Becoming Archenemies

Shown when an NPC accepts a Shattered Amulet and becomes archenemies with the player:

| Key | Description |
| --- | ----------- |
| `AcceptShatteredAmulet` | Default dialogue for becoming enemies for the first time |

Shown when an NPC rejects a Shattered Amulet:

| Key | Description |
| --- | ----------- |
| `RejectShatteredAmulet_AlreadyArchenemies` | The NPC and player are already archenemies |
| `RejectShatteredAmulet_ExArchenemies` | The NPC and player were ex-archenemies |
| `RejectShatteredAmulet_NoNegativeHearts` | Player has > -1 hearts with the NPC |
| `RejectShatteredAmulet_VeryLowNegativeHearts` | Player has > -8 hearts with the NPC |
| `RejectShatteredAmulet_LowNegativeHearts` | Player has > -10 hearts with the NPC |

## Misc

| Key | Description |
| --- | ----------- |
| `RejectMermaidPendant_NegativeHearts` | The player offered an NPC a Mermaid's Pendant, but they have negative friendship with that NPC |
| `RejectBouquet_NegativeHearts` | The player offered an NPC a Bouquet, but they have negative friendship with that NPC |