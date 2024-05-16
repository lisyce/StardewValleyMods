
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using System;
using System.Text.RegularExpressions;
using Object = StardewValley.Object;

namespace WikiLinks
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.pet))]
        public class FarmAnimal_pet_Patch
        {
            public static bool Prefix(ref FarmAnimal __instance)
            {
                if (!Config.EnableMod || !SHelper.Input.IsDown(Config.LinkModButton)) return true;

                OpenPage(GetWikiPageForFarmAnimal(__instance));
                return false;
            }
        }

        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.rightClick))]
        public class InventoryMenu_rightClick_Patch
        {
            public static bool Prefix(InventoryMenu __instance)
            {
                if (!Config.EnableMod || !SHelper.Input.IsDown(Config.LinkModButton))
                    return true;
                foreach(var i in __instance.inventory)
                {
                    if (i.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        int slotNumber = Convert.ToInt32(i.name);
                        if (slotNumber < __instance.actualInventory.Count)
                        {
                            if (__instance.actualInventory[slotNumber] != null)
                            {
                                OpenPage(GetWikiPageForItem(__instance.actualInventory[slotNumber], SHelper.Translation));
                            }
                        }
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, ref bool __result)
            {
                if (!Config.EnableMod || !SHelper.Input.IsDown(Config.LinkModButton))
                    return true;
                if (__instance.objects.TryGetValue(Game1.currentCursorTile, out Object obj))
                {
                    OpenPage(GetWikiPageForItem(obj, SHelper.Translation));
                    __result = true;
                    return false;
                }
                else if (__instance.objects.TryGetValue(Game1.currentCursorTile + new Vector2(0, 1), out Object obj2) && obj2.bigCraftable.Value)
                {
                    OpenPage(obj2.DisplayName);
                    __result = true;
                    return false;
                }
                foreach (var f in __instance.furniture)
                {
                    if (f.GetBoundingBox().Contains(Game1.currentCursorTile * 64))
                    {
                        OpenPage(f.DisplayName);
                        __result = true;
                        return false;
                    }
                }
                if (__instance.terrainFeatures.TryGetValue(Game1.currentCursorTile, out TerrainFeature feature))
                {
                    if (feature is HoeDirt && (feature as HoeDirt).crop != null)
                    {
                        OpenPage(new Object((feature as HoeDirt).crop.indexOfHarvest.Value, 1).DisplayName);
                        __result = true;
                        return false;
                    }
                }
                foreach (var c in __instance.characters)
                {
                    if ((c.IsVillager && c.Tile + new Vector2(0, - 1) == Game1.currentCursorTile ) || c.Tile == Game1.currentCursorTile)
                    {
                        if(c.IsVillager || c is Monster)
                            OpenPage(c.displayName);
                        else if(c is Pet)
                            OpenPage(GetWikiPageForPet(SHelper.Translation));
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
        }
    }
}