using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewValley.Menus;
using Microsoft.Xna.Framework;

namespace BZP_Allergies.HarmonyPatches
{
    [HarmonyPatch(typeof(StardewValley.Object), "getDescription")]
    internal class PatchTooltip : Initializable
    {
        [HarmonyPostfix]
        static void GetDescription_Postfix(StardewValley.Object __instance, ref string __result)
        {
            // note: this method gets called A LOT. This prefix needs to be efficient
            // 25 chars max a line
            try
            {
                ISet<string> allergens = AllergenManager.GetAllergensInObject(__instance);
                if (allergens.Count == 0) return;

                StringBuilder allergenText = new("\nAllergens: ");
                int currLineLen = allergenText.Length;

                int i = 0;
                foreach (string a in allergens)
                {
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
                    i++;
                }
                __result += allergenText.ToString();
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GetDescription_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
    }

    [HarmonyPatch(typeof(Item), "canStackWith")]
    internal class PatchCanStack : Initializable
    {
        [HarmonyPostfix]
        static void CanStackWith_Postfix(ISalable other, StardewValley.Item __instance, ref bool __result)
        {
            try
            {
                // don't do any work if we know they can't stack already
                if (!__result) return;

                ISet<string> instanceAllergens = AllergenManager.GetAllergensInObject(__instance as StardewValley.Object);
                ISet<string> otherAllergens = AllergenManager.GetAllergensInObject(other as StardewValley.Object);

                if (instanceAllergens.Count != otherAllergens.Count)
                {
                    // different allergen count, so different allergens. can't stack
                    __result = false;
                    return;
                }

                // do they contain the same allergens?
                foreach (string a in instanceAllergens)
                {
                    if (!otherAllergens.Contains(a))
                    {
                        __result = false;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(CanStackWith_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
    }

    [HarmonyPatch(typeof(SkillsPage), "draw")]
    internal class PatchSkillsPageDraw : Initializable
    {
        public static ClickableTextureComponent? AllergyTab = null;  // TODO move this to the actual menu draw class later as a static constant

        [HarmonyPrefix]
        static void Draw_Prefix(SkillsPage __instance, SpriteBatch b)
        {
            try
            {
                ClickableTextureComponent allergyTab = new(
                    "BarleyZP.BzpAllergies",
                    new Rectangle(__instance.xPositionOnScreen - 48, __instance.yPositionOnScreen + 64 * 2, 64, 64),
                    "",
                    "Allergies",
                    Game1.mouseCursors,
                    new Rectangle(640, 80, 16, 16),
                    4f
                 );

                allergyTab.draw(b);
                AllergyTab = allergyTab;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Draw_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }
    }

    [HarmonyPatch(typeof(SkillsPage), "performHoverAction")]
    internal class PatchSkillsPageHover : Initializable
    {
        [HarmonyPostfix]
        static void PerformHoverAction_Postfix(SkillsPage __instance, int x, int y)
        {
            try
            {
                if (PatchSkillsPageDraw.AllergyTab == null) return;

                if (PatchSkillsPageDraw.AllergyTab.containsPoint(x, y))
                {
                    Traverse.Create(__instance).Field("hoverText").SetValue(PatchSkillsPageDraw.AllergyTab.hoverText);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(PerformHoverAction_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}
