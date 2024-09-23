using HarmonyLib;
using StardewValley;
using System.Reflection;
using System.Reflection.Emit;
using StardewValley.Menus;

namespace EnemyOfTheValley.Patches
{
    internal class SocialPagePatches
    {
        public static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(SocialPage), "drawNPCSlotHeart"),
                transpiler: new HarmonyMethod(typeof(SocialPagePatches), nameof(drawNPCSlotHeart_Prefix))
                );
        }

        public static void drawNPCSlotHeart_Prefix(SocialPage.SocialEntry entry)
        {
            if
        }
    }
}
