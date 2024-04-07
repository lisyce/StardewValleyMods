using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BZP_Allergies.HarmonyPatches
{
    [HarmonyPatch(typeof(IClickableMenu), "drawToolTip")]
    internal class PatchTooltip : Initializable
    {
        public static StardewValley.Object? craftedObj = null;

        [HarmonyPrefix]
        static void DrawToolTip_Prefix(ref string hoverText, ref Item hoveredItem)
        {
            // note: this method gets called A LOT. This prefix needs to be efficient
            // 25 chars max a line
            try
            {
                List<string> allergens = AllergenManager.GetAllergensInObject(hoveredItem as StardewValley.Object);
                if (allergens.Count == 0) return;

                StringBuilder allergenText = new("\nAllergens: ");
                int currLineLen = allergenText.Length;

                for (int i=0; i < allergens.Count; i++)
                {
                    string a = allergens[i];

                    int len = a.Length;
                    if (currLineLen + len > 25)
                    {
                        allergenText.Append('\n');
                        currLineLen = 0;
                    }

                    allergenText.Append(AllergenManager.GetAllergenDisplayName(a));
                    currLineLen += len;

                    if (i < allergens.Count - 1)
                    {
                        allergenText.Append(", ");
                        currLineLen += 2;
                    }
                }
                hoverText += allergenText.ToString();
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(DrawToolTip_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}
