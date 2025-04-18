using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using SObject = StardewValley.Object;

namespace Functional_Wishing_Well;

public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.CanRefillWateringCanOnTile)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(CanRefillWateringCanOnTile_Postfix)));
    }

    private static void CanRefillWateringCanOnTile_Postfix(GameLocation __instance, int tileX, int tileY, ref bool __result)
    {
        if (__result) return;
        
        SObject? obj = __instance.getObjectAtTile(tileX, tileY);
        __result = obj is { QualifiedItemId: "(BC)BarleyZP.FunctionalWishingWell_WishingWell", isTemporarilyInvisible: false };
    }
}