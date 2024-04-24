﻿using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using static BZP_Allergies.AllergenManager;

namespace BZP_Allergies.HarmonyPatches
{
    [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.createQuestionDialogue), new Type[] { typeof(string), typeof(Response[]), typeof(string) })]
    internal class PatchEatQuestionPopup : Initializable
    {
        [HarmonyPrefix]
        static void CreateQuestionDialogue_Prefix(ref string question)
        {
            try
            {
                bool randomAllergies = ModDataGet(Game1.player, "BarleyZP.BzpAllergies_Random", out string val) && val == "true";

                if (!ModEntry.Config.HintBeforeEating || Game1.player.ActiveObject == null)
                {
                    return;
                }

                // is this the "Eat {0}?" or "Drink {0}?" popup?
                IDictionary<string, string> stringsData = GameContent.Load<Dictionary<string, string>>("Strings/StringsFromCSFiles");
                
                string activeObjectName = Game1.player.ActiveObject.DisplayName;
                string eatQuestion = string.Format(stringsData["Game1.cs.3160"], activeObjectName);
                string drinkQuestion = string.Format(stringsData["Game1.cs.3159"], activeObjectName);

                if (question.Equals(eatQuestion) || question.Equals(drinkQuestion))
                {
                    bool hasDiscoveredAtLeastOneAllergy = false;
                    ISet<string> allergens = GetAllergensInObject(Game1.player.ActiveObject);
                    foreach (string a in allergens)
                    {
                        if (PlayerHasDiscoveredAllergy(a))
                        {
                            hasDiscoveredAtLeastOneAllergy = true;
                            break;
                        }
                    }

                    if (FarmerIsAllergic(Game1.player.ActiveObject) && (!randomAllergies || (randomAllergies && hasDiscoveredAtLeastOneAllergy)))
                    {
                        question += " You are allergic to it!";
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(CreateQuestionDialogue_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }
    }

}
