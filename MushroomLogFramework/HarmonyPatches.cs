using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;

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
            .ThrowIfNotMatch("Could not find second entry point for Object::OutputMushroomLog transpielr")
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

    private static void NormalizeDistribution(Dictionary<string, int> distribution)
    {
        var total = distribution.Values.Sum();
        foreach (var key in distribution.Keys)
        {
            var res = ((float)distribution[key] / total) * 100;
            distribution[key] = (int) res;  // we want int from 0-100, not float from 0 to 1
        }
    }

    private static bool ObjectHasData(StardewValley.Object obj)
    {
        ModEntry.ModMonitor.Log(ModEntry.ProduceRules.ContainsKey(obj.QualifiedItemId).ToString(), LogLevel.Debug);
        return ModEntry.ProduceRules.ContainsKey(obj.QualifiedItemId);
    }
    
    private static string RollTreeProduce(StardewValley.Object obj, Tree t)
    {
        ModEntry.ModMonitor.Log("rolltree", LogLevel.Debug);
        // common mushroom fallback
        if (!ModEntry.ProduceRules.TryGetValue(obj.QualifiedItemId, out var data)) return FallbackProduce;

        // get the tree data
        if (data.SpecificTreeProbabilities.TryGetValue(t.treeType.Value, out var produceRule))
        {
            return DrawFromDistribution(produceRule);
        }
        else
        {
            // use defaults
            ModEntry.ModMonitor.Log("defaults", LogLevel.Debug);
            return DrawFromDistribution(data.DefaultTreeProbabilities);
        }
    }

    private static string RollDefaultProduce(StardewValley.Object obj)
    {
        // common mushroom fallback
        if (!ModEntry.ProduceRules.TryGetValue(obj.QualifiedItemId, out var data)) return FallbackProduce;
        return DrawFromDistribution(data.DefaultTreeProbabilities);
    }

    private static string DrawFromDistribution(Dictionary<string, int> distribution)
    {
        if (!distribution.Any()) return FallbackProduce;
        
        NormalizeDistribution(distribution);

        var choices = new List<string>();
        foreach (var pair in distribution)
        {
            choices.AddRange(Enumerable.Repeat(pair.Key, pair.Value));   
        }

        var res = Game1.random.ChooseFrom(choices);
        ModEntry.ModMonitor.Log(res, LogLevel.Debug);
        return res;
    }
}