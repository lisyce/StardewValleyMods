# Mushroom Log Framework

Mushroom Log Framework allows content pack authors to edit some of the hardcoded mushroom log behavior.

## Overview

The [Vanilla algorithm](https://stardewvalleywiki.com/Mushroom_Log) for determining the output of a mushroom log is a bit complicated, but here's the gist:
1. An empty list of possible outputs is created.
2. Each fully-grown nearby tree contributes one possible output to the list, depending on the type of tree it is.
3. A number of possible "default" outputs are added to the list based on the number of nearby trees, fully-grown or not.
4. A random output is selected from the list, and the quality/stack size are set based on various factors.

This mod keeps the same general idea, but provides mod authors with more control over the output.

## Features

- Add or change the mushroom log's potential outputs based on nearby trees
- Add or change the "default" potential outputs
- Add potential outputs based on nearby fruit trees, not just wild trees
- Set the stack size, quality, and more with [item spawn fields](https://stardewvalleywiki.com/Modding:Item_queries#Item_spawn_fields)
- Set different rules for different machines that use the mushroom log behavior
  - If a machine has an `OutputItem`'s `OutputMethod` set to `StardewValley.Object, Stardew Valley: OutputMushroomLog`, then this mod can be used to set the outputs for it.

## Data format

You can edit mushroom logs by editing the `BarleyZP.MushroomLogFramework/ProduceRules` asset. See [assets/VanillaProduceRules.json](../assets/VanillaProduceRules.json) for the Vanilla values.
This consists of a string → model lookup, where...

- The key is the qualified item Id of the machine (e.g. `(BC)MushroomLog`)
- The value is a model with the fields listed below.

|field| description                                                                                                                                                           |
|-|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
|DefaultTreeOutputs| The "default" possible outputs (see algorithm above). The types of nearby trees do not affect these outputs. This consists of a list of [Tree Outputs](#Tree-Outputs). |
|SpecificTreeOutputs| The possible outputs based on the type of nearby trees (see algorithm above). This consists of a list of [Tree Rules](#Tree-Rules).                                   |


### Tree Rules

The fields for a tree rule are described below.

|field|description|
|-|-|
|Id|The [unique string Id](https://stardewvalleywiki.com/Modding:Common_data_field_types#Unique_string_ID) for this entry.|
|Type|*(Optional)* The type of tree. One of either `Wild` or `Fruit`. Default `Wild`.|
|TreeId|The unique Id of the tree in its corresponding data asset (`Data/WildTrees` or `Data/FruitTrees`).|
|Outputs|The possible outputs for this entry. This consists of a list of [Tree Outputs](#Tree-Outputs).|
|Condition| A [game state query](https://stardewvalleywiki.com/Modding:Game_state_queries) condition to determine whether any of the entries in `Outputs` can be considered for this tree.|

When choosing a possible output based on a specific tree, the mod will collect all possible tree rules that apply and then select an output from the combined set of possible Outputs.

#### Tree Outputs

The fields for a possible tree output item are described below.

| field              |description|
|--------------------|-|
| *common fields*    |See [item spawn fields](https://stardewvalleywiki.com/Modding:Item_queries#Item_spawn_fields) for the generic item fields supported. If set to an [item query](https://stardewvalleywiki.com/Modding:Item_queries) which returns multiple items, one of them will be selected at random.|
| Precedence         |*(Optional)* The order in which this entry should be checked, where lower values are checked first. Outputs with the same precedence are shuffled randomly. Default 0.|
| Chance             |*(Optional)* The probability that this output will be chosen if checked, as a decimal between 0 and 1. Default 1.|
| ChanceModifiers    |*(Optional)* [Quantity modifiers](https://stardewvalleywiki.com/Modding:Common_data_field_types#Quantity_modifiers) applied to the `Chance` value. Default none.|
| ChanceModifierMode |*(Optional)* [Quantity modifier modes](https://stardewvalleywiki.com/Modding:Common_data_field_types#Quantity_modifiers) which indicate what to do if multiple modifiers in the `ChanceModifiers` field apply at once. Default `Stack`.|
|AllowQualityModifications|*(Optional)* Whether to allow mossy wild trees and fruit trees with produce to affect the quality of the output item. It is recommended to disable this if you set the quality using item spawn fields. Default true.|
|AllowQuantityModifications|*(Optional)* Whether to allow the number of nearby trees to affect the stack size of the output item. It is recommended to disable this if you set the stack size using item spawn fields. Default true.|
|DisableDefaultOutputPossibilities|*(Optional)* If this output is selected as a possible output, whether to block the "default" output possibilities from being considered. Default false.|

## Examples and Snippets

### Adding a new possible output from the oak tree

This Content Patcher example adds mystery boxes as a potential output from the oak tree.

```json
{
  "Format": "2.7.0",
  "Changes": [
    {
      "Action": "EditData",
      "Target": "BarleyZP.MushroomLogFramework/ProduceRules",
      "TargetField": [ "(BC)MushroomLog", "SpecificTreeOutputs" ],
      "Entries": {
        "{{ModId}}_MysteryBoxOak": {
          "Id": "{{ModId}}_MysteryBoxOak",
          "TreeId": "1",
          "Outputs": [
            {
              "ItemId": "(O)MysteryBox",
              "Precedence": -1,  // this entry is checked before the Vanilla entries with Precedence of 0
              "Chance": 0.3,  // 30% chance
              "MinStack": 1,
              "MaxStack": 5,
              "AllowQualityModifications": false,  // mystery boxes should only be normal quality
              "AllowQuantityModifications": false  // we set the stack above, so we shouldn't allow further modifications
            }
          ]
        }
      }
    }
  ]
}
```

### Adding possible outputs based on fruit trees

This Content Patcher example adds a random fruit as an output when a cherry tree is nearby. If only cherry trees are nearby
and no other rules interfere, then this will guarantee that a cherry or apple is outputted from the mushroom log.

```json
{
  "Format": "2.7.0",
  "Changes": [
    {
      "Action": "EditData",
      "Target": "BarleyZP.MushroomLogFramework/ProduceRules",
      "TargetField": [ "(BC)MushroomLog", "SpecificTreeOutputs" ],
      "Entries": {
        "RandomFruitCherryTree": {
          "Id": "RandomFruitCherryTree",
          "TreeId": "628",
          "Type": "Fruit",
          "Outputs": [
            {
              "ID": "CherryOrApple",
              "RandomItemId": ["(O)638", "(O)613"],
              "Chance": 1,  // 100% chance
              "DisableDefaultOutputPossibilities": true  // we won't get any "default" possibilties
            }
          ]
        }
      }
    }
  ]
}
```
