using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;
using static MushroomLogFramework.MushroomLogData;

namespace MushroomLogFramework;

public class HarmonyPatches
{
    private const string FallbackProduce = "(O)404";
    
    public static void Patch(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.OutputMushroomLog)),
            transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(NewTranspilerHehe)));
    }

    private static IEnumerable<CodeInstruction> NewTranspilerHehe(IEnumerable<CodeInstruction> instructions,
        ILGenerator gen)
    {
        var matcher = new CodeMatcher(instructions, gen);
        
        // enter the method
        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Ldarg_S),
            new CodeMatch(OpCodes.Initobj));
        
        // init the list of terrain features to hold all nearby trees and fruit trees
        var treeListLocal = gen.DeclareLocal(typeof(List<TerrainFeature>));
        var terrainFeatureListCtor = AccessTools.Constructor(typeof(List<TerrainFeature>), Array.Empty<Type>());
        matcher.Insert(
            new CodeInstruction(OpCodes.Newobj, terrainFeatureListCtor),
            new CodeInstruction(OpCodes.Stloc, treeListLocal.LocalIndex));
        
        // init the list of Items that could be potential outputs
        var outputListLocal = gen.DeclareLocal(typeof(List<(Item, TreeOutputItem?)>));
        var itemListCtor = AccessTools.Constructor(typeof(List<(Item, TreeOutputItem?)>), Array.Empty<Type>());
        matcher.Insert(
            new CodeInstruction(OpCodes.Newobj, itemListCtor),
            new CodeInstruction(OpCodes.Stloc, outputListLocal.LocalIndex));
        
        // add all nearby trees and fruit trees to the list
        var tryAddTree = AccessTools.Method(typeof(HarmonyPatches), nameof(TryAddTreeToList));
        matcher.MatchStartForward(
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Isinst),
                new CodeMatch(OpCodes.Stloc_S))
            .Advance(1)
            .Insert(
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Ldloc, treeListLocal.LocalIndex),
                new CodeInstruction(OpCodes.Call, tryAddTree)
            );
        
        // replace the treeCount local with the appropriate value from our new local list
        var getTreeCount = AccessTools.PropertyGetter(typeof(List<Tree>), nameof(List<Tree>.Count));
        var getTerrainFeatureCount = AccessTools.PropertyGetter(typeof(List<TerrainFeature>), nameof(List<TerrainFeature>.Count));

        matcher.MatchStartForward(
                new CodeMatch(OpCodes.Callvirt, getTreeCount))
            .Advance(1)
            .Insert(
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldloc, treeListLocal.LocalIndex),
                new CodeInstruction(OpCodes.Callvirt, getTerrainFeatureCount));
        
        // skip the original tree loop; we don't need it
        // we can just pop the result of the enumerator MoveNext call and load false onto the stack so the loop never runs
        var moveNext = AccessTools.Method(typeof(List<Tree>.Enumerator), nameof(List<Tree>.Enumerator.MoveNext));
        matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldloca_S),
                new CodeMatch(OpCodes.Call, moveNext))
            .Advance(1)
            .Insert(
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldc_I4_0));
        
        // loop through the list of terrain features, roll their contribution, and add it to the output list
        // then, store the output in the mossyCount local (local 3)
        var populateOutputs = AccessTools.Method(typeof(HarmonyPatches), nameof(PopulatePotentialOutputs));
        matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Stloc_S))
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc, outputListLocal.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc, treeListLocal.LocalIndex),
                new CodeInstruction(OpCodes.Call, populateOutputs),
                new CodeInstruction(OpCodes.Stloc, 3));
        
        // roll the default outputs and add those Items to the list
        var rollDefault = AccessTools.Method(typeof(HarmonyPatches), nameof(RollDefaultProduce));
        var shouldRollDefault = AccessTools.Method(typeof(HarmonyPatches), nameof(ShouldRollDefaultProduce));
        var outputListAdd = AccessTools.Method(typeof(List<(Item, TreeOutputItem?)>), nameof(List<(Item, TreeOutputItem?)>.Add));
        matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldstr, "(O)404"),
                new CodeMatch(OpCodes.Br_S),
                new CodeMatch(OpCodes.Ldstr, "(O)420"),
                new CodeMatch(OpCodes.Br_S),
                new CodeMatch(OpCodes.Ldstr, "(O)422"),
                new CodeMatch(OpCodes.Callvirt))
            .Advance(1)
            .Insert(
                new CodeInstruction(OpCodes.Ldloc, outputListLocal.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, rollDefault),
                new CodeInstruction(OpCodes.Callvirt, outputListAdd))
            .Advance(4)
            .CreateLabel(out Label rollDefaultLabel)
            .Advance(-4)
            .Insert(
                new CodeInstruction(OpCodes.Ldloc, outputListLocal.LocalIndex),
                new CodeInstruction(OpCodes.Call, shouldRollDefault),
                new CodeInstruction(OpCodes.Brfalse, rollDefaultLabel));
        
        // at method end, instead of the original ItemRegistry::Create call, choose at random from the list and
        // set the stack and quality to the amounts calculated by the method already
        var chooseFrom = AccessTools.Method(typeof(HarmonyPatches), nameof(ChooseFrom));
        var game1Rand = AccessTools.Field(typeof(Game1), nameof(Game1.random));
        
        matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldsfld, game1Rand),
                new CodeMatch(OpCodes.Ldloc_2),
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Call))
            .Advance(1)
            .Insert(
                new CodeInstruction(OpCodes.Pop),  // we don't need the item vanilla chose
                new CodeInstruction(OpCodes.Ldloc, outputListLocal.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc, 4),
                new CodeInstruction(OpCodes.Ldloc, 5),
                new CodeInstruction(OpCodes.Call, chooseFrom));

        return matcher.InstructionEnumeration();
    }

    private static Item ChooseFrom(List<(Item, TreeOutputItem?)> items, int amount, int quality)
    {
        var (item, treeOutputItem) = Game1.random.ChooseFrom(items);
        if (treeOutputItem?.AllowQualityModifications ?? true)
        {
            item.Quality = quality;
        }

        if (treeOutputItem?.AllowQuantityModifications ?? true)
        {
            item.Stack = amount;
        }
        return item;
    }

    // returns "mossyCount", aka number of trees with moss or fruit
    private static int PopulatePotentialOutputs(StardewValley.Object machine, List<(Item, TreeOutputItem?)> outputs,
        List<TerrainFeature> nearbyTrees)
    {
        var mossyCount = 0;
        
        foreach (var feature in nearbyTrees)
        {
            if (feature is Tree wildTree && wildTree.growthStage.Value >= 5)
            {
                outputs.Add(RollTreeProduce(machine, feature));
                if (wildTree.hasMoss.Value) mossyCount++;
            }
            else if (feature is FruitTree fruitTree && fruitTree.growthStage.Value >= 4 && !fruitTree.stump.Value)
            {
                outputs.Add(RollTreeProduce(machine, feature));
                if (fruitTree.fruit.Any()) mossyCount++;
            }
        }

        return mossyCount;
    }

    private static void TryAddTreeToList(TerrainFeature feature, List<TerrainFeature> list)
    {
        if (feature is Tree or FruitTree) list.Add(feature);
    }

    private static (Item, TreeOutputItem?) RollTreeProduce(StardewValley.Object machine, TerrainFeature t)
    {
        // if the machine has no data in the asset, default to (BC)MushroomLog for the data
        if (!ModEntry.ProduceRules.TryGetValue(machine.QualifiedItemId, out var data) ||
            !ModEntry.ProduceRules.TryGetValue("(BC)MushroomLog", out data)) return (ItemRegistry.Create(FallbackProduce), null);
        
        // get all possible outputs for this tree from all the entries
        List<TreeOutputItem> possibleOutputs = new();
        foreach (var produce in data.SpecificTreeOutputs)
        {
            if (produce.Type == TreeType.Wild && t is Tree wildTree && wildTree.treeType.Value == produce.TreeId &&
                GameStateQuery.CheckConditions(produce.Condition, wildTree.Location))
            {
                possibleOutputs.AddRange(produce.Outputs);
            }
            else if (produce.Type == TreeType.Fruit && t is FruitTree fruitTree && fruitTree.treeId.Value == produce.TreeId &&
                     GameStateQuery.CheckConditions(produce.Condition, fruitTree.Location))
            {
                possibleOutputs.AddRange(produce.Outputs);
            }
        }

        return Util.SelectTreeContribution(possibleOutputs, RollDefaultProduce(machine).Item1);
    }

    private static bool ShouldRollDefaultProduce(List<(Item, TreeOutputItem?)> outputs)
    {
        return !outputs.Any(pair => pair.Item2?.DisableDefaultOutputPossibilities ?? false);
    }

    private static (Item, TreeOutputItem?) RollDefaultProduce(StardewValley.Object obj)
    {
        // common mushroom fallback
        if (!ModEntry.ProduceRules.TryGetValue(obj.QualifiedItemId, out var data)) return (ItemRegistry.Create(FallbackProduce), null);
        return Util.SelectTreeContribution(data.DefaultTreeOutputs, ItemRegistry.Create(FallbackProduce));
    }
}