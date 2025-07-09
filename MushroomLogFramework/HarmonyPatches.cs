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
            transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(OutputMushroomLogTranspiler)));
    }

    private static IEnumerable<CodeInstruction> OutputMushroomLogTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
    {
        var matcher = new CodeMatcher(instructions, gen);

        var hasMoss = AccessTools.Field(typeof(Tree), nameof(Tree.hasMoss));
        var hasData = AccessTools.Method(typeof(HarmonyPatches), nameof(ObjectHasData));
        var rollTree = AccessTools.Method(typeof(HarmonyPatches), nameof(RollTreeProduce));
        var rollDefault = AccessTools.Method(typeof(HarmonyPatches), nameof(RollDefaultProduce));
        var create = AccessTools.GetDeclaredMethods(typeof(ItemRegistry))
            .FirstOrDefault(m =>
                m.Name == nameof(ItemRegistry.Create) && !m.IsGenericMethod);
        
        // step one: replace the per-tree mushroom
        matcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldloc_2),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Callvirt), // list add
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldfld, hasMoss))
            .ThrowIfNotMatch("Could not find entry point for Object::OutputMushroomLog transpiler")
            .Advance(2)
            .CreateLabel(out Label lbl)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, hasData),
                new CodeInstruction(OpCodes.Brfalse, lbl),

                // the object had data
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc, 12), // the tree
                new CodeInstruction(OpCodes.Call, rollTree))
            .MatchEndForward(
                new CodeMatch(OpCodes.Ldstr, "(O)422"),
                new CodeMatch(OpCodes.Callvirt))
            .ThrowIfNotMatch("Could not find second entry point for Object::OutputMushroomLog transpiler")
            .CreateLabel(out Label lbl2)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, hasData),
                new CodeInstruction(OpCodes.Brfalse, lbl2),

                // the object had data
                new CodeInstruction(OpCodes.Pop), // pop "(O)422" off the stack since we will replace it
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, rollDefault));

        return matcher.InstructionEnumeration();
    }
    
    private static bool ObjectHasData(StardewValley.Object obj)
    {
        return ModEntry.ProduceRules.ContainsKey(obj.QualifiedItemId);
    }
    
    // TODO: make this return an Item and pick from the list of items rather than item ids
    // TODO: need to deal with disabling vanilla affecting quality and quantity
    private static string RollTreeProduce(StardewValley.Object obj, TerrainFeature t)
    {
        // common mushroom fallback
        if (!ModEntry.ProduceRules.TryGetValue(obj.QualifiedItemId, out var data)) return FallbackProduce;
        
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

        var (item, outputRule) = Util.SelectTreeContribution(possibleOutputs, FallbackProduce);
        return item.QualifiedItemId;
    }

    private static string RollDefaultProduce(StardewValley.Object obj)
    {
        // common mushroom fallback
        if (!ModEntry.ProduceRules.TryGetValue(obj.QualifiedItemId, out var data)) return FallbackProduce;
        var (item, outputRule) = Util.SelectTreeContribution(data.DefaultTreeOutputs, FallbackProduce);
        return item.QualifiedItemId;
    }
}