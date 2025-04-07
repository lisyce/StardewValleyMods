# Author Guide

This guide describes how to use Conversation Topic Utilities (CTU). In this guide, "Conversation Topic" is abbreviated as "CT".

## The Asset

TODO make it a list not a dict! so it's ordered!

Other mods can interface with CTU by editing the asset it exposes (normally, this would be done through Content Patcher
or by using the C# asset editing events). This asset has the name `BarleyZP.CTU/TopicRules`. It's a mapping of string keys
to object values, the fields of which are described below. Keys should be the CT the rule is for.

Note that the *first* matching topic rule is used for a CT, so consider placing more specific rules like ones where `KeyIsPrefix` is true higher in the list than more general rules.

### Fields

| Field | Description | Default Value |
| ----- | ----------- | ------------- |
| `KeyIsPrefix` | Whether the key for this value is a prefix that may match many CTs. For example, "somePrefixKey_" is a prefix that matches the CTs "somePrefixKey_" and "somePrefixKey_Abigail" but *not* "Abigail_somePrefixKey_". | `false` |
| `RepeatableOnExpire` | Whether to immediately mark all matching CTs as repeatable (by clearing the associated mail flags) on the night they expire. | `false` |
| `MemoriesRepeatableOnExpire` | Whether to immediately mark all matching CTs' *memories* as repeatable on the night they expire. (e.g. "someKey_memory_oneweek" is a memory for the key "someKey"). | `false` |
| `DefaultDialogueRules` | A mapping of string keys to lists of string values, describing default dialogue lines for matching CTs and the conditions under which they appear. More details can be found in below. | `{}` |

### Default Dialogue Rules

The `DefaultDialogueRules` field of a Topic Rule allows you to specify a default dialogue line, the NPCs that should say it, and under what conditions they should say it. For example, maybe all NPCs have a default dialogue line for a CT that changes depending on whether it's raining or not. This field is a map. Keys should be the dialogue line (e.g. "Wow! This is my default dialogue line!$h"). The values are a list of rules, which are of the format `"RuleType: Arguments"`. Possible rules are described below.

| Rule Type | Arguments | Description | Example |
| - | - | - | - |
| `ForNPC` | The NPCs this dialogue is for. Either `Any` or a list of internal NPC names. | Allows a default dialogue line to only be applied when speaking to specific NPCs. | `"ForNPC: Abigail, Emily"`, `"ForNPC: Any"` |
| `GSQ` | A Game State Query | Allows a default dialogue to only be applied when a given GSQ is true. | `"GSQ: WEATHER Here Rain"` |
| `TopicContains` | A string to match. This is an exact match. | Allows a default dialogue to only be applied when the CT contains the argument string. Useful in conjunction with the `KeyIsPrefix` field above. | `"TopicContains: someText"` |

Note that *all* rules must evaluate to true for an NPC to say a default dialogue line for a CT. An empty list of rules (e.g. `"Default dialogue here!": []`) can be used for a default dialogue line that all NPCs will use in all circumstances for a CT.

## Examples

The below are example Content Patcher snippets to accomplish common tasks with CTU.

### Making Topics Repeatable

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
          "RepeatableOnExpire": true,
          "MemoriesRepeatableOnExpire": true
        }
      }
    }
  ]
}
```

### Matching Many CTs with one Topic Rule

This example makes all of the vanilla "dating_{{NPC Name}}" conversation topics (and their memories) repeatable (perhaps if you break up with Sebastian but get back together, you still want people to say something about it the second time!)

```json
{
  "Format": "2.6.0",
  "Changes": [
    {
      "Action": "EditData",
      "Target": "BarleyZP.CTU/TopicRules",
      "Entries": {
        "dating_": {
          "KeyIsPrefix": true,
          "RepeatableOnExpire": true,
          "MemoriesRepeatableOnExpire": true
        }
      }
    }
  ]
}
```

### Default Dialogue

This example gives every NPC 
