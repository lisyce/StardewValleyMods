using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using HarmonyLib;

namespace YourProjectName
{
    internal sealed class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            Harmony harmony = new(ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.setRunning)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(setRunning_Postfix))
                );
        }
        public static void setRunning_Postfix(ref Farmer __instance)
        {
            if (__instance.bathingClothes.Value)
            {
                __instance.speed = 5;
            }
        }
    }    
}