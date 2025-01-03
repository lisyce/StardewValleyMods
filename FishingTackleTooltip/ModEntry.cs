﻿using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;
using System.Reflection;
using System.Reflection.Emit;

namespace FishingTackleTooltip
{
    internal sealed class ModEntry : Mod
    {
        public static ITranslationHelper Translation;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Translation = helper.Translation;

            Harmony harmony = new(ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(FishingRod), "doDoneFishing"),
                transpiler: new HarmonyMethod(typeof(ModEntry), nameof(doDoneFishing_Transpiler))
            );

            // helper.Events.GameLoop.DayStarted += OnDayStarted;

            helper.ConsoleCommands.Add("tackle_tooltip", "Get the bait/tackle you recently ran out of from the fishing rod you're holding.", GetLastUsed);
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            StardewValley.Object brokenTrapBobber = ItemRegistry.Create<StardewValley.Object>("(O)694");
            brokenTrapBobber.uses.Value = FishingRod.maxTackleUses - 1;
            Game1.player.addItemToInventory(brokenTrapBobber);

            StardewValley.Object treasure = ItemRegistry.Create<StardewValley.Object>("(O)693");
            treasure.uses.Value = FishingRod.maxTackleUses - 1;
            Game1.player.addItemToInventory(treasure);
        }

        public static IEnumerable<CodeInstruction> doDoneFishing_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> inputInstrList = new(instructions);

            string baitStr = "Strings\\StringsFromCSFiles:FishingRod.cs.14085";

            string tackleStr = "Strings\\StringsFromCSFiles:FishingRod.cs.14086";

            CodeMatcher matcher = new(inputInstrList);

            MethodInfo baitRunOut = AccessTools.Method(typeof(ModEntry), nameof(ShowBaitRunOutMessage));
            MethodInfo tackleRunOut = AccessTools.Method(typeof(ModEntry), nameof(ShowTackleRunOutMessage));

            matcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldstr, baitStr)
                )
                .ThrowIfNotMatch("could not find bait string to replace")
                .Advance(-1)
                .RemoveInstructions(4)
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, baitRunOut)
                )
            .MatchStartForward(
                new CodeMatch(OpCodes.Ldstr, tackleStr)
             )
            .ThrowIfNotMatch("could not find tackle string to replace")
            .Advance(-1)
            .RemoveInstructions(4)
            .Insert(
                new CodeInstruction(OpCodes.Ldloc_S, 5),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Call, tackleRunOut)
            );

            return matcher.InstructionEnumeration();
        }

        public static void ShowBaitRunOutMessage(StardewValley.Object bait, FishingRod rod)
        {
            // bait is attachment slot 0
            rod.modData["BarleyZP.FishingTackleTooltip.0"] = bait.DisplayName;

            if (bait.QualifiedItemId == "(O)703")
            {
                Game1.showGlobalMessage(Translation.Get("baitGoneMagnet"));
            }
            else
            {
                Game1.showGlobalMessage(Translation.Get("baitGone", new { baitName = bait.DisplayName }));
            }
        }

        public static void ShowTackleRunOutMessage(StardewValley.Object tackle, FishingRod rod, int i)
        {
            rod.modData["BarleyZP.FishingTackleTooltip." + i] = tackle.DisplayName;
            Game1.showGlobalMessage(Translation.Get("tackleGone", new { tackleName = tackle.DisplayName }));
        }

        private void GetLastUsed(string command, string[] args)
        {
            Item? item = Game1.player.CurrentItem;
            if (item is not null && item is FishingRod rod)
            {
                bool found = false;
                foreach (var pair in rod.modData.Pairs)
                {
                    if (pair.Key.StartsWith("BarleyZP.FishingTackleTooltip.")) {
                        string bait = pair.Value;
                        Monitor.Log(bait, LogLevel.Info);
                        found = true;
                    }
                }

                if (!found)
                {
                    Monitor.Log("No known last-used bait.", LogLevel.Debug);
                }
                
            }
            else
            {
                Monitor.Log("You need to be holding a fishing rod.", LogLevel.Debug);
            }
        }
    }
}
