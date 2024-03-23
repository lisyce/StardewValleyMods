using HarmonyLib;
using StardewValley;
using StardewModdingAPI;
using BZP_Allergies.Config;
using System.Numerics;

namespace BZP_Allergies
{
    [HarmonyPatch(typeof(Farmer), nameof(Farmer.doneEating))]
    internal class PatchFarmerAllergies
    {
        private static IMonitor Monitor;
        private static Random Rand;
        private static ModConfig Config;
        private static IModHelper ModHelper;


        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper modHelper)
        {
            Monitor = monitor;
            Rand = new Random();
            Config = config;
            ModHelper = modHelper;
        }

        [HarmonyPrefix]
        static bool DoneEating_Prefix(ref Farmer __instance, out int __state)
        {
            try
            {
                StardewValley.Object? itemToEat = __instance.itemToEat as StardewValley.Object;
                __state = itemToEat == null ? int.MinValue : itemToEat.Edibility;

                if (itemToEat != null && AllergenManager.FarmerIsAllergic(itemToEat, Config, ModHelper.GameContent))
                {
                    // change edibility
                    itemToEat.Edibility = -20;

                    // add the allergic reaction buff
                    __instance.applyBuff(AllergenManager.ALLERIC_REACTION_DEBUFF);
                    
                    // randomly apply nausea
                    if (Rand.NextDouble() < 0.50)
                    {
                        __instance.applyBuff(Buff.nauseous);
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(DoneEating_Prefix)}:\n{ex}", LogLevel.Error);
                __state = int.MinValue;  // error value
            }
            return true; // run original logic
        }

        [HarmonyPostfix]
        static void DoneEating_Postfix(ref Farmer __instance, int __state)
        {
            try
            {
                StardewValley.Object? itemToEat = __instance.itemToEat as StardewValley.Object;
                if (itemToEat != null && AllergenManager.FarmerIsAllergic(itemToEat, Config, ModHelper.GameContent) && __state != int.MinValue)
                {
                    // change edibility
                    itemToEat.Edibility = __state;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(DoneEating_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}
