# Author Guide

This guide describes how to use Conversation Topic Utilities (CTU). In this guide, "Conversation Topic" is abbreviated as "CT".

## Trigger Actions

CTU provides the following new [Trigger Actions](https://stardewvalleywiki.com/Modding:Trigger_actions):

| Action                                               | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
|------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `CTU.MarkCtRepeatable <topic Id> [include memories]` | Immediately marks the provided topic as repeatable by clearing the associated dialogue mail flags. If the memories argument is `true` (default `false`), it also marks all memories for that topic as repeatable. Note that you will again need to mark this topic (and optionally the memories) as repeatable every time you want them repeated. To do this automatically, see the `RepeatableOnExpire` and `MemoriesRepeatableOnExpire` fields in the asset later in this document. |

## Console Commands

CTU provides the following utility console commands:

| Command          | Description                                                                            |
|------------------|----------------------------------------------------------------------------------------|
| `CTU_ListActive` | Lists all of the current player's active CTs and the number of days until they expire. |

## Conversation Topic Rules (Default Dialogue, Repeatable Dialogue, etc.)

Other mods can interface with CTU by editing the asset it exposes (normally, this would be done through Content Patcher
or by using the C# asset editing events). This asset has the name `BarleyZP.CTU/TopicRules`. It's a list of entries, the fields of which are described below. See the [Content Patcher List Editing Docs](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/action-editdata.md#edit-a-list).

Note that the *first* matching topic rule is used for a CT, so consider placing more specific rules, like ones where `IdIsPrefix` is true, higher in the list than more general rules. 

### Fields

| Field                        | Description                                                                                                                                                                                                                                                                                                                                                                                         | Default Value                     |
|------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------|
| `Id`                         | The CT itself. For example, `"dating_Abigail"`. Must be unique among all entries in the asset.                                                                                                                                                                                                                                                                                                      | Required field; no default value. |
| `IdIsPrefix`                 | Whether the Id for this entry is a prefix that may match many CTs. For example, "somePrefix_" is a prefix that matches the CTs "somePrefix_" and "somePrefix_Abigail" but *not* "Abigail_somePrefix_". Note that a prefix also does not match memory keys unless it's an exact match. For example, "myOtherPrefix_" is a prefix for "myOtherPrefix_Sam" but not "myOtherPrefix_Sam_memory_oneweek". | `false`                           |
| `RepeatableOnExpire`         | Whether to immediately mark all matching CTs as repeatable (by clearing the associated mail flags) on the night they expire.                                                                                                                                                                                                                                                                        | `false`                           |
| `MemoriesRepeatableOnExpire` | Whether to immediately mark all matching CTs' *memories* as repeatable on the night they expire. (e.g. "someKey_memory_oneweek" is a memory for the key "someKey").                                                                                                                                                                                                                                 | `false`                           |
| `DefaultDialogueRules`       | A list of entries, describing default dialogue lines for matching CTs and the conditions under which they appear. More details can be found below.                                                                                                                                                                                                                                                  | `[]`                              |

### Default Dialogue Rules

> Default dialogue rules are only used if the NPC does not already have a line in their dialogue file for the CT. The game will first check if they have a dialogue line in their respective dialogue file. If not, only then will the default dialogue rules for the CT be checked.
>
> This feature is not intended to be a replacement for the existing method of specifying CT keys in an NPC's dialogue file.

The `DefaultDialogueRules` field of a Topic Rule allows you to specify a default dialogue line, the NPCs that should say it, and under what conditions they should say it. For example, maybe all NPCs have a default dialogue line for a CT, but only if it's not raining. This field is a list of entries. The first default dialogue rule where all `Rules` are true will be used when talking to an NPC.

Note that *all* rules must evaluate to true for an NPC to say a default dialogue line for a CT. An empty list of rules can be used for a default dialogue line that all NPCs will use in all circumstances for a CT.

#### Fields

| Field      | Description                                                                                                                                                                          | Default Value                     |
|------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------|
| `Id`       | Unique identifier among all default dialogue rules for a topic rule.                                                                                                                 | Required field; no default value. |
| `Dialogue` | The default dialogue line itself.                                                                                                                                                    | Required field; no default value. |
| `Rules`    | A list of string rules in the form `"RuleType: Arguments"` that specify when a default dialogue line applies. All rules must apply for a default dialogue line to be said by an NPC. | `[]`                              |

#### Available Rules

| Rule Type       | Arguments                                                                      | Description                                                                                                                                     | Example                                     |
|-----------------|--------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------|
| `ForNPC`        | The NPCs this dialogue is for. Either `ANY` or a list of internal NPC names.   | Allows a default dialogue line to only be applied when speaking to specific NPCs.                                                               | `"ForNPC: Abigail, Emily"`, `"ForNPC: ANY"` |
| `GSQ`           | A [Game State Query](https://stardewvalleywiki.com/Modding:Game_state_queries) | Allows a default dialogue to only be applied when a given GSQ is true.                                                                          | `"GSQ: WEATHER Here Rain"`                  |
| `TopicContains` | A string to match. This is an exact match.                                     | Allows a default dialogue to only be applied when the CT contains the argument string. Useful in conjunction with the `IdIsPrefix` field above. | `"TopicContains: someText"`                 |
| `CurrentNpcManner` | The [manner](https://stardewvalleywiki.com/Modding:NPC_data#Basic_info) to match. | Allows a default dialogue to apply only when the NPC the player is speaking to has a specific manner field. | `"CurrentNpcManner: Rude"` |

### The %CurrentNPC% Token

`%CurrentNPC%` is a special token that is populated with the internal name of the NPC the player is currently talking to.
As of now, it can only be used in default dialogue rules: `"GSQ: PLAYER_HEARTS Current %CurrentNPC% 2"`, `"TopicContains: %CurrentNPC%"`

```json
{
  "Id": "myConversationTopic_",
  "IdIsPrefix": true,
  "DefaultDialogueRules": [
    {
      "Id": "SomeUniqueId",
      "Dialogue": "I only say this line if my name is in the CT key!",
      "Rules": [ "TopicContains: _%CurrentNPC%" ]      
    }      
  ]
}
```

In the above example, if the player has an active CT "{{ModId}}_myConversationTopic_Sam", then only Sam will use the default dialogue rule.
This is because the rule evaluates to `"TopicContains: _Sam"` when the player talks to Sam and the topic contains "Sam". 
Note that Sam would also reply to a CT with key "{{ModID}}_myConversationTopic_Sameday" (this name has no significance) because this key matches the topic rule
and the default dialogue rule is still true. You should think carefully about how you name your conversation topics if you
wish to use any string-matching features like the `TopicContains` rule.

### Examples

The below are example Content Patcher snippets to accomplish common tasks with CTU.

#### Making Topics Repeatable

This example would make the "{{ModId}}_myConversationTopic" CT and its memories (e.g. "{{ModId}}_myConversationTopic_memory_oneweek") instantly able to be repeated when the CT expires for a player. Note that this does not reactivate the CT. Consider using trigger actions, etc. for this purpose.

```json
{
  "Format": "2.6.0",
  "Changes": [
    {
      "Action": "EditData",
      "Target": "BarleyZP.CTU/TopicRules",
      "Entries": {
        "{{ModId}}_myConversationTopic": {
          "Id": "{{ModId}}_myConversationTopic",
          "RepeatableOnExpire": true,
          "MemoriesRepeatableOnExpire": true
        }
      }
    }
  ]
}
```

#### Matching Many CTs with one Topic Rule

This example makes all of the vanilla "dating_{{NPC Name}}" conversation topics (and their memories) repeatable.

```json
{
  "Format": "2.6.0",
  "Changes": [
    {
      "Action": "EditData",
      "Target": "BarleyZP.CTU/TopicRules",
      "Entries": {
        "dating_": {
          "Id": "dating_",
          "IdIsPrefix": true,
          "RepeatableOnExpire": true,
          "MemoriesRepeatableOnExpire": true
        }
      }
    }
  ]
}
```

#### Default Dialogue

This example provides default dialogue lines for the vanilla CT that activates when the Community Center is completed. If it's raining, Sam will say "Hey! I'm Sam, it's raining, and I think it's cool you completed the CC!$h". All other NPCs will say "Hey, it's raining and you completed the CC!" if it's raining when you speak to them. Otherwise, the NPC will say "Hey, it's cool you completed the CC!".

Notice that the order of the default dialogue rules from specific to general allows for this behavior.

```json
{
  "Format": "2.6.0",
  "Changes": [
    {
      "Action": "EditData",
      "Target": "BarleyZP.CTU/TopicRules",
      "Entries": {
        "cc_Complete": {
          "Id": "cc_Complete",
          "DefaultDialogueRules": [
            {
              "Id": "{{ModId}}_SamRaining",
              "Dialogue": "Hey! I'm Sam, it's raining, and I think it's cool you completed the CC!$h",
              "Rules": [ "ForNPC: Sam", "GSQ: WEATHER Here Rain" ]
            },
            {
              "Id": "{{ModId}}_raining",
              "Dialogue": "Hey, it's raining and you completed the CC!",
              "Rules": [ "GSQ: WEATHER Here Rain" ]
            },
            {
              "Id": "{{ModId}}_fallback",
              "Dialogue": "Hey, it's cool you completed the CC!"
            }
          ]
        }
      }
    }
  ]
}
```
