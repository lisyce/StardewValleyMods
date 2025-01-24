# New Dialogue Keys For All NPCs

If a specific dialogue is not set for an NPC, defaults will be used. Dialogue in a table is shown in order of precedence.

## Negative Heart Dialogue

TODO

## Becoming Enemies

Shown when an NPC accepts the "A-void Me" Cake and becomes enemies with the player:

| Key | Description |
| --- | ----------- |
| `AcceptEnemyCake_ExArchenemies` | The NPC and player were ex-archenemies |
| `AcceptEnemyCake` | Default dialogue for becoming enemies for the first time |

Shown when an NPC rejects the "A-void Me" Cake:

| Key | Description |
| --- | ----------- |
| `RejectEnemyCake_Archenemies` | The NPC and player are already archenemies |
| `RejectEnemyCake_NoNegativeHearts` | Player has > -1 hearts with the NPC |
| `RejectEnemyCake_VeryLowNegativeHearts` | Player has > -4 hearts with the NPC |
| `RejectEnemyCake_LowNegativeHearts` | Player has > -8 hearts with the NPC |

## Misc

| Key | Description |
| --- | ----------- |
| `RejectMermaidPendant_NegativeHearts` | The player offered an NPC a Mermaid's Pendant, but they have negative friendship with that NPC |
| `RejectBouquet_NegativeHearts` | The player offered an NPC a Bouquet, but they have negative friendship with that NPC |