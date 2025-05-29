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
        var create = AccessTools.GetDeclaredMethods(typeof(ItemRegistry))
            .FirstOrDefault(m =>
                m.Name == nameof(ItemRegistry.Create) && !m.IsGenericMethod);
        var changeQuality = AccessTools.Method(typeof(HarmonyPatches), nameof(ChangeOutputQualityIfNeeded));
        
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
                new CodeInstruction(OpCodes.Call, rollDefault))
            .MatchStartForward(
                new CodeMatch(OpCodes.Call, create))
            .ThrowIfNotMatch("Could not find third entry point for Object::OutputMushroomLog transpiler")
            .Advance(1)
            .Insert(
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, changeQuality));

        return matcher.InstructionEnumeration();
    }
    
    private static bool ObjectHasData(StardewValley.Object obj)
    {
        return ModEntry.ProduceRules.ContainsKey(obj.QualifiedItemId);
    }
    
    private static string RollTreeProduce(StardewValley.Object obj, Tree t)
    {
        // common mushroom fallback
        if (!ModEntry.ProduceRules.TryGetValue(obj.QualifiedItemId, out var data)) return FallbackProduce;

        // get the tree data
        if (data.SpecificTreeWeights.TryGetValue(t.treeType.Value, out var produceRule))
        {
            return Util.DrawFromDistribution(produceRule, FallbackProduce);
        }
        else
        {
            // use defaults
            return Util.DrawFromDistribution(data.DefaultTreeWeights, FallbackProduce);
        }
    }

    private static string RollDefaultProduce(StardewValley.Object obj)
    {
        // common mushroom fallback
        if (!ModEntry.ProduceRules.TryGetValue(obj.QualifiedItemId, out var data)) return FallbackProduce;
        return Util.DrawFromDistribution(data.DefaultTreeWeights, FallbackProduce);
    }

    private static void ChangeOutputQualityIfNeeded(Item output, StardewValley.Object machine)
    {
        if (ModEntry.ProduceRules.TryGetValue(machine.QualifiedItemId, out var data) &&
            data.DisableQualityModifiersOn.Contains(output.QualifiedItemId))
        {
            output.Quality = 0;
        }
    }
}