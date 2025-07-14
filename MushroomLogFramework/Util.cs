using StardewValley;
using StardewValley.Extensions;
using StardewValley.Internal;
using static MushroomLogFramework.MushroomLogData;
namespace MushroomLogFramework;

public class Util
{
    public static (Item, TreeOutput?) SelectTreeContribution(List<TreeOutput> outputs, Item fallback)
    {
        IEnumerable<TreeOutput> possibleOutputs = outputs;
        possibleOutputs = possibleOutputs.OrderBy(t => t.Precedence)
            .ThenBy(t => Game1.random.Next());

        ItemQueryContext ctx = new();
        for (var i = 0; i < 2; i++)
        {
            foreach (var output in possibleOutputs)
            {
                if (!GameStateQuery.CheckConditions(output.Condition)) continue;
                
                // TODO debug logging
                var item = ItemQueryResolver.TryResolveRandomItem(output, ctx, logError: (query, message) => { });
                if (item == null) continue;

                var chance = Utility.ApplyQuantityModifiers(output.Chance, output.ChanceModifiers, output.ChanceModifierMode);
                if (Game1.random.NextBool(chance)) return (item, output);
            }
        }

        return (fallback, null);
    }
}