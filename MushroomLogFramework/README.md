# Mushroom Log Framework

Mushroom Log Framework allows content pack authors to edit some of the hardcoded mushroom log behavior.

## Background

Please familiarize yourself with the [mushrrom log wiki page](https://stardewvalleywiki.com/Mushroom_Log) so
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

| field                 | description                                                                                                                                                                                                           |
|-----------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `DefaultTreeWeights`  | A string to float dictionary of qualified output item Ids to the weight of that output item. These are used as the basic distribution of outputs, including when there is no specific distribution for a nearby tree. |
| `SpecificTreeWeights` | A string to model dictionary of wild tree Ids to a mapping of outputs in the same format as `DefaultTreeWeights`.                                                                                                     |

Because this mod normalizes the probability of each output item being produced, the weights do not need to sum to 1.

> You may use the `mushroom_log_summary` console command to see the normalized probabilities for each distribution.

### Example

This example Content Patcher pack adds two new possible outputs to the oak tree's distribution:

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
    }
  ]
}
```