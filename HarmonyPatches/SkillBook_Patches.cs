﻿using HarmonyLib;
using StardewValley.Menus;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BZP_Allergies.HarmonyPatches
{
    internal class SkillBook_Patches
    {
        public static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), "readBook"),
                postfix: new HarmonyMethod(typeof(SkillBook_Patches), nameof(ReadBook_Postfix))
            );
        }

        public static void ReadBook_Postfix(ref StardewValley.Object __instance)
        {
            if (!AllergenManager.ModDataGet(Game1.player, Constants.ModDataRandom, out string val) || val == "false")
            {
                return;  // we don't discover allergies
            }

            string bookId = Traverse.Create(__instance).Property("ItemId").GetValue<string>();
            if (bookId == Constants.AllergyTeachBookId)
            {
                ISet<string> playerHas = AllergenManager.ModDataSetGet(Game1.player, Constants.ModDataHas);
                ISet<string> playerDiscovered = AllergenManager.ModDataSetGet(Game1.player, Constants.ModDataDiscovered);

                if (playerDiscovered.Count == playerHas.Count) return;

                // pick a random one to discover
                List<string> diff = playerHas.Except(playerDiscovered).ToList();
                int discoverIdx = new Random().Next(0, diff.Count);
                AllergenManager.DiscoverPlayerAllergy(diff[discoverIdx]);

                if (!Game1.player.mailReceived.Contains("read_a_book"))
                {
                    Game1.player.mailReceived.Add("read_a_book");
                }

                Game1.player.stats.Increment(bookId);
                Game1.showGlobalMessage("You've learned more about your dietary restrictions.");
            }
        }
    }
}