using HarmonyLib;
using StardewValley;
using StardewModdingAPI;
using StardewValley.Buffs;
using StardewValley.GameData.Buffs;
using Microsoft.Xna.Framework.Graphics;

using static BZP_Allergies.AllergenManager;

namespace BZP_Allergies.HarmonyPatches
{
    internal class PatchFarmerDoneEating
    {
        public static void DoneEating_Prefix(ref Farmer __instance, out int __state)
        {
            try
            {
                StardewValley.Object? itemToEat = __instance.itemToEat as StardewValley.Object;
                __state = itemToEat == null ? int.MinValue : itemToEat.Edibility;
                if (itemToEat == null || !__instance.IsLocalPlayer)
                {
                    return;
                }

                Texture2D sprites = Game1.content.Load<Texture2D>("BarleyZP.BzpAllergies/Sprites");

                if (FarmerIsAllergic(itemToEat) && !__instance.hasBuff(Buff.squidInkRavioli))
                {
                    ISet<string> itemToEatAllergens = GetAllergensInObject(itemToEat);

                    // is it dairy and do we have the buff?
                    if (itemToEatAllergens.Contains("dairy") && __instance.hasBuff(Constants.LactaseBuff))
                    {
                        HUDMessage lactaseProtectionMessage = new("Good thing you took your lactase!");
                        lactaseProtectionMessage.messageSubject = itemToEat;
                        Game1.addHUDMessage(lactaseProtectionMessage);
                        Game1.playSound("jingle1");
                        return;
                    }

                    // change edibility
                    itemToEat.Edibility = -20;

                    // clear any existing allergy buffs
                    __instance.buffs.Remove(Constants.ReactionDebuff);

                    // add the allergic reaction buff
                    __instance.applyBuff(AllergenManager.GetAllergicReactionBuff(itemToEat.DisplayName, "consume", 120000));
                    
                    // randomly apply nausea
                    if (new Random().NextDouble() < 0.50)
                    {
                        __instance.applyBuff(Buff.nauseous);
                    }

                    // TODO: abstract this out so that when holding or cooking a food you can get this mail and discover allergies
                    // send mail
                    if (!__instance.mailReceived.Contains(ModEntry.MOD_ID + "_harvey_ad"))
                    {
                        Game1.addMailForTomorrow(ModEntry.MOD_ID + "_harvey_ad");
                    }

                    // discover allergies
                    foreach (string allergen in itemToEatAllergens)
                    {
                        if (FarmerIsAllergic(allergen) && DiscoverPlayerAllergy(allergen))
                        {
                            Game1.showGlobalMessage("You've learned more about your dietary restrictions.");
                            Game1.playSound("newArtifact");
                            break;
                        }
                    }
                }
                else if (itemToEat.QualifiedItemId.Equals("(O)" + Constants.AllergyReliefId))
                {
                    // nausea is automatically removed. remove the reaction as well
                    __instance.buffs.Remove(Constants.ReactionDebuff);
                    
                }
                else if (itemToEat.QualifiedItemId.Equals("(O)" + Constants.LactasePillsId))
                {
                    // get that dairy immunity
                    Buff immuneBuff = new(Constants.LactaseBuff, "food", itemToEat.DisplayName,
                        120000, sprites, 3, null,
                        false, "Dairy Immunity", "Quick, eat the cheese!");

                    __instance.applyBuff(immuneBuff);
                }
            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.Log($"Failed in {nameof(DoneEating_Prefix)}:\n{ex}", LogLevel.Error);
                __state = int.MinValue;  // error value
            }
        }
        public static void DoneEating_Postfix(ref Farmer __instance, int __state)
        {
            try
            {
                StardewValley.Object? itemToEat = __instance.itemToEat as StardewValley.Object;
                if (itemToEat != null && __state != int.MinValue)
                {
                    // change edibility back to original value
                    itemToEat.Edibility = __state;
                }                
            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.Log($"Failed in {nameof(DoneEating_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}
