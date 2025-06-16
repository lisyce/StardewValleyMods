using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace MultiplayerPerfectionCollections;
public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.Constructor(typeof(CollectionsPage)),
            transpiler: new HarmonyMethod(typeof(ModEntry), nameof(CtorTranspiler)));
    }

    private static IEnumerable<CodeInstruction> CtorTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var getPlayer = AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.player));
        var basicShipped = AccessTools.Field(typeof(Farmer), nameof(Farmer.basicShipped));
        var isShipped = AccessTools.Method(typeof(Util), nameof(Util.ShouldDrawInShippingCollection));
        
        var matcher = new CodeMatcher(instructions);
        matcher.MatchEndForward(
                new CodeMatch(OpCodes.Call, getPlayer),
                new CodeMatch(OpCodes.Ldfld, basicShipped),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Callvirt))
            .ThrowIfNotMatch($"Could not find entrypoint for {nameof(CtorTranspiler)}")
            .Set(OpCodes.Call, isShipped);  // replace the containsKey call with our helper

        return matcher.InstructionEnumeration();
    }
}
