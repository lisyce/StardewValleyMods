# Mod Author Guide

## Conversation Topic Determination

On day's end, Failed Quests Lose Friendship checks for both "Help Wanted" quests and Special Orders that have expired
without completion. For "Help Wanted" quests, it gets the `Requester` (an internal, not display name, for an NPC).
This is the "target" of the quest (`.target.Value` in C#) for all but the Socialize Quest types.

For Special Orders, the mod uses the *key* of the Special Order in `Data/SpecialOrders` (see [Special Orders on the wiki](https://stardewvalleywiki.com/Modding:Special_orders)).
In the table below (and in C#), this is the `questKey`.

The mod sets a 1-day [Conversation Topic](https://stardewvalleywiki.com/Modding:Dialogue#Conversation_topics)
denoting what quest was failed and which NPC requested it.
Every conversation topic set by the mod is of the format `BarleyZP.FailedQuestsLoseFriendship_questFailed_{topic}`. The following
table explains how topics are determined per-quest:

| Quest Type                              | Relevant C# Class                              | Topic            |
|-----------------------------------------|------------------------------------------------|------------------|
| "Help Wanted" Resource Collection Quest | `StardewValley.Quests.ResourceCollectionQuest` | `rc_{Requester}` |
| "Help Wanted" Slay Monsters Quest       | `StardewValley.Quests.SlayMonsterQuest`        | `sm_{Requester}` |
| "Help Wanted" Fishing Quest             | `StardewValley.Quests.FishingQuest`            | `f_{Requester}`  |
| "Help Wanted" Item Delivery Quest       | `StardewValley.Quests.ItemDeliveryQuest`       | `id_{Requester}` |
| "Help Wanted" Socialize Quest           | `StardewValley.Quests.SocializeQuest`          | `socialize`      |
| Special Orders                          | `StardewValley.SpecialOrders.SpecialOrder`     | `so_{questKey}`  |

Every night, the relevant mail flags are removed from the player so that these conversation topics can be repeated.
This is different from normal behavior where conversation topics are not repeatable.

### Example Content Patcher Patches

> These patches would need to go inside a `Changes` block and would not work as a standalone file. Please see the [Content Patcher Docs](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide.md)
> for more information about using Content Patcher.

The player failed a Pam's special order for "potato juice" (which has `questKey` "Pam"), and Penny is going to comment on it:

```json
{
  "Action": "EditData",
  "Target": "Characters/Dialogue/Penny",
  "Entries": {
    "BarleyZP.FailedQuestsLoseFriendship_questFailed_so_Pam": "Dialogue Here"
  }
}
```

The player failed an Item Delivery "Help Wanted" quest for Harvey, and Maru is going to comment on it:

```json
{
  "Action": "EditData",
  "Target": "Characters/Dialogue/Maru",
  "Entries": {
    "BarleyZP.FailedQuestsLoseFriendship_questFailed_id_Harvey": "Dialogue Here"
  }
}
```

The player failed a Slay Monsters "Help Wanted" quest for Clint, and Clint himself is going to comment on it:

```json
{
  "Action": "EditData",
  "Target": "Characters/Dialogue/Clint",
  "Entries": {
    "BarleyZP.FailedQuestsLoseFriendship_questFailed_sm_Clint": "Dialogue Here"
  }
}
```

## Compat With Other Mods

While this mod does not add dialogue for custom NPCs or Quests/Special Orders added by other mods,
it should be compatible with them. Please see the documentation above to add dialogue for custom quests!
