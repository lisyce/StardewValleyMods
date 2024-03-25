using HarmonyLib;
using StardewValley;
using StardewModdingAPI;
using StardewValley.Buffs;
using StardewValley.GameData.Buffs;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Utilities;

namespace BZP_Allergies.HarmonyPatches
{
    [HarmonyPatch(typeof(Farmer), nameof(Farmer.doneEating))]
    internal class PatchFarmerAllergies : Initializable
    {
        [HarmonyPrefix]
        static bool DoneEating_Prefix(ref Farmer __instance, out int __state)
        {
            try
            {

                StardewValley.Object? itemToEat = __instance.itemToEat as StardewValley.Object;
                __state = itemToEat == null ? int.MinValue : itemToEat.Edibility;

                if (itemToEat != null && AllergenManager.FarmerIsAllergic(itemToEat, Config, GameContent))
                {
                    // change edibility
                    itemToEat.Edibility = -20;

                    // add the allergic reaction buff
                    BuffAttributesData buffAttributesData = new()
                    {
                        Speed = -2,
                        Defense = -1,
                        Attack = -1,
                    };

                    BuffEffects effects = new(buffAttributesData);
                    string iconPath = PathUtilities.NormalizeAssetName(@"TileSheets/BuffsIcons");
                    Buff reactionBuff = new(AllergenManager.ALLERIC_REACTION_DEBUFF, "food", itemToEat.DisplayName,
                        120000, GameContent.Load<Texture2D>(iconPath), 6, effects,
                        true, "Allergic Reaction", "Probably shouldn't have eaten that...");
                    reactionBuff.glow = Microsoft.Xna.Framework.Color.Green;

                    __instance.applyBuff(reactionBuff);

                    // randomly apply nausea
                    if (new Random().NextDouble() < 0.50)
                    {
                        __instance.applyBuff(Buff.nauseous);
                    }
                }
                else if (itemToEat != null && itemToEat.QualifiedItemId.Equals("(O)BzpAllergies_AllergyMedicine"))
                {
                    // nausea is automatically removed. remove the reaction as well
                    __instance.buffs.Remove(AllergenManager.ALLERIC_REACTION_DEBUFF);
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
                if (itemToEat != null && AllergenManager.FarmerIsAllergic(itemToEat, Config, GameContent) && __state != int.MinValue)
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
