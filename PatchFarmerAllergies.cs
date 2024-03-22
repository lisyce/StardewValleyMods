using HarmonyLib;
using StardewValley;
using StardewModdingAPI;

namespace BZP_Allergies
{
    [HarmonyPatch(typeof(Farmer), nameof(Farmer.doneEating))]
    internal class PatchFarmerAllergies
    {
        private static IMonitor Monitor;
        private static Random rnd;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
            rnd = new Random();
        }

        [HarmonyPrefix]
        static bool DoneEating_Prefix(ref Farmer __instance)
        {
            try
            {
                StardewValley.Object? itemToEat = __instance.itemToEat as StardewValley.Object;
                if (itemToEat != null)
                {
                    
                    // add the allergic reaction buff
                    __instance.applyBuff("bzp_allergies_1");
                    
                    // randomly apply nausea
                    if (rnd.NextDouble() < 0.75)
                    {
                        __instance.applyBuff(Buff.nauseous);
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(DoneEating_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true; // run original logic
        }
    }
}
