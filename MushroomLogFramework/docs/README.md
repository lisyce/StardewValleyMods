# Mushroom Log Framework

Mushroom Log Framework allows content pack authors to edit some of the hardcoded mushroom log behavior.

## Background

Please familiarize yourself with the [mushroom log wiki page](https://stardewvalleywiki.com/Mushroom_Log) so
that you better understand how Vanilla handles mushroom log outputs. This mod does not change the general
algorithm used to decide the output. Rather, it simply allows for adding to the list of potential outputs
based on the nearby trees. See `VanillaProduceRules.json` in the `assets` folder for the Vanilla distributions
of output items.

## Editing the Data

You can change the produce of mushroom log-type machines (any machine that calls the Vanilla `Object::OutputMushroomLog`
method) by editing the `BarleyZP.MushroomLogFramework/ProduceRules` asset. This asset is a string to model
lookup.

- The string key is the qualified Id of the mushroom log machine. For example, Vanilla's `(BC)MushroomLog`.
- The value is a model with the following fields:

| field                       | description                                                                                       
| - | - |
| `DefaultTreeWeights`        | A string to float dictionary of qualified output item Ids to the weight of that output item. If one output has a higher weight than another, it is more likely to get randomly chosen as the mushroom log's output. These are used as the basic distribution of outputs, including when there is no specific distribution for a nearby tree. |
| `SpecificTreeWeights`       | A string to model dictionary of wild tree Ids to a mapping of outputs in the same format as `DefaultTreeWeights`. These are used when adding potential outputs to the mushroom log based on nearby wild trees. |
| `DisableQualityModifiersOn` | A list of qualified item Ids that quality modifiers should not be applied to. Optional. |

Because this mod normalizes the probability of each output item being produced, *the weights do not need to sum to 1*.

> You may use the `mushroom_log_summary` console command to see the percent chance of each output item for each tree, etc. (These are the normalized probabilities of each output item based on the weights).

### Example

This example Content Patcher pack edits the Vanilla Mushroom Log's potential outputs based on nearby oak trees.
In addition to the Morel, it adds Moss and Mystery Boxes as potential output items. Moss and Morels will both
be chosen as potential outputs with a 40% chance and Mystery Boxes will be chosen with a 20% chance. This example also disables changing the quality of the Moss and Mystery Box outputs since we always want them to be normal quality.

```json
{
  "Format": "2.7.0",
  "Changes": [
    {
      "Action": "EditData",
      "Target": "BarleyZP.MushroomLogFramework/ProduceRules",
      "TargetField": [ "(BC)MushroomLog", "SpecificTreeWeights", "1"],  // target the oak tree
      "Entries": {
        "(O)Moss": 1,
        "(O)MysteryBox": 0.5
      }
    },
    {
        "Action": "EditData",
        "Target": "BarleyZP.MushroomLogFramework/ProduceRules",
        "TargetField": [ "(BC)MushroomLog", "DisableQualityModifiersOn" ],
        "Entries": {
          "#-1": "(O)Moss",
          "#-2": "(O)MysteryBox"
        }
    }
  ]
}
```

Explanation: When the mushroom log code adds potential output items based on nearby oak trees, it has 3 options: Morel (already in Vanilla, so not pictured here), Moss, and Mystery Boxes. In Vanilla, Morel has a weight of 1. We added Moss with a weight of 1 and Mystery Boxes with a weight of 0.5. So, it's more likely that the oak tree will contribute a Morel or Moss than a Mystery Box. The total sum of weights is 2.5. This makes it a 1/2.5 = 0.4 = 40% chance that a Morel is picked, a 40% chance that Moss is picked, and a 0.5/2.5 = 0.2 = 20% chance that Mystery Boxes are picked.
