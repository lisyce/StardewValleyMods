using HarmonyLib;
using StardewValley;
using System.Reflection.Emit;
using System.Reflection;
using Microsoft.Xna.Framework.Content;
using Sickhead.Engine.Util;
using StardewModdingAPI;


namespace EnemyOfTheValley.Patches
{
    internal class NPCDialoguePatches
    {
        public static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.checkForNewCurrentDialogue)),
                transpiler: new HarmonyMethod(typeof(NPCDialoguePatches), nameof(checkForNewCurrentDialogue_Transpiler))
                );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.grantConversationFriendship)),
                transpiler: new HarmonyMethod(typeof(NPCDialoguePatches), nameof(grantConversationFriendship_Transpiler))
                );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), "loadCurrentDialogue"),
                transpiler: new HarmonyMethod(typeof(NPCDialoguePatches), nameof(loadCurrentDialogue_Transpiler))
                );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.TryGetDialogue), new [] {typeof(string)}),
                prefix: new HarmonyMethod(typeof(NPCDialoguePatches), nameof(TryGetDialogue_Prefix_V1))
                );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.TryGetDialogue), new [] {typeof(string), typeof(object[])}),
                prefix: new HarmonyMethod(typeof(NPCDialoguePatches), nameof(TryGetDialogue_Prefix_V2))
                );
            harmony.Patch(
                original: AccessTools.Method(typeof(Dialogue), nameof(Dialogue.TryGetDialogue)),
                prefix: new HarmonyMethod(typeof(NPCDialoguePatches), nameof(StaticTryGetDialogue_Prefix)));
        }

        public static Dictionary<string, string> NegativeDialogueLoader(NPC npc)
        {
            try
            {
                var dialogueFile = "BarleyZP.EnemyOfTheValley\\NegativeHeartDialogue\\" + npc.GetDialogueSheetName();
                return Game1.content
                    .Load<Dictionary<string, string>>(dialogueFile)
                    .Select(delegate(KeyValuePair<string, string> pair)
                    {
                        string key = pair.Key;
                        string value2 = Dialogue.applyGenderSwitch(str: pair.Value, gender: Game1.player.Gender,
                            altTokenOnly: true);
                        return new KeyValuePair<string, string>(key, value2);
                    }).ToDictionary((p) => p.Key, (p) => p.Value);
            }
            catch (ContentLoadException)
            {
                return new Dictionary<string, string>();
            }
        }
        
        public static void StaticTryGetDialogue_Prefix(NPC speaker, ref string translationKey)
        {
            if (translationKey.Contains("rainy") &&
                Game1.player.friendshipData.TryGetValue(speaker.Name, out var friendship) && friendship.Points <= -250)
            {
                translationKey = "BarleyZP.EnemyOfTheValley\\NegativeHeartDialogue\\Rainy:" + speaker.GetDialogueSheetName();
            }
        }

        public static bool TryGetDialogue_Prefix_V1(ref NPC __instance, ref Dialogue __result, ref string key)
        {
            if (!Game1.player.friendshipData.TryGetValue(__instance.Name, out var friendship) ||
                friendship.Points > -250)  // only go to negative dialogue asset if we have at least 1 negative heart
            {
                return true;  // run original
            }
            
            // try to get negative dialogue first
            var dialogue = NegativeDialogueLoader(__instance);
            if (dialogue.TryGetValue(key, out var text))
            {   
                __result = new Dialogue(__instance, "BarleyZP.EnemyOfTheValley\\NegativeHeartDialogue\\" + __instance.GetDialogueSheetName() + ":" + key, text);
                return false;  // skip original because we found something loaded on the negative sheet
            }
            
            return true;  // continue to the original method
        }
        
        public static bool TryGetDialogue_Prefix_V2(ref NPC __instance, ref Dialogue __result, ref string key, params object[] substitutions)
        {
            if (!Game1.player.friendshipData.TryGetValue(__instance.Name, out var friendship) ||
                friendship.Points > -250)  // only go to negative dialogue asset if we have at least 1 negative heart
            {
                return true;  // run original
            }
            
            // try to get negative dialogue first
            var dialogue = NegativeDialogueLoader(__instance);
            if (dialogue.TryGetValue(key, out var text))
            {
                __result = new Dialogue(__instance, "BarleyZP.EnemyOfTheValley\\NegativeHeartDialogue\\" + __instance.GetDialogueSheetName() + ":" + key, string.Format(text, substitutions));
                return false;  // skip original because we found something loaded on the negative sheet
            }
            
            return true;  // continue to the original method
        }

        public static IEnumerable<CodeInstruction> grantConversationFriendship_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new(instructions);
            MethodInfo changeAmt = AccessTools.Method(typeof(NPCDialoguePatches), nameof(ChangeConversationFriendshipAmount));

            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldc_I4_S),
                new CodeMatch(OpCodes.Starg_S),
                new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldarg_2))
                .ThrowIfNotMatch("could not find place to insert method")
                .Advance(1)
                .Insert(
                    // we keep the ldarg.2 instruction to load the amount arg and instead feed it into our method
                    new(OpCodes.Ldarg_1),
                    new(OpCodes.Ldloc_0),
                    new(OpCodes.Call, changeAmt));  // return val goes onto stack for changeFriendship to use

            return matcher.InstructionEnumeration();
        }
        
        public static IEnumerable<CodeInstruction> checkForNewCurrentDialogue_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo negativeLocDialogue = AccessTools.Method(typeof(NPCDialoguePatches), nameof(NegativeLocationDialogue));
            CodeMatcher matcher = new(instructions);

            // we want to insert under the while loop
            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_2),
                new CodeMatch(OpCodes.Sub),
                new CodeMatch(OpCodes.Stloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Brtrue_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_2),
                new CodeMatch(OpCodes.Bge_S)
                )
                .ThrowIfNotMatch("could not find the end of the while loop to insert after")
                .Advance(1)
                .Insert(
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldloc_S, (byte)7),
                    new(OpCodes.Ldarg_1),
                    new(OpCodes.Ldloc_S, (byte)6),
                    new(OpCodes.Call, negativeLocDialogue),
                    new(OpCodes.Stloc_S, (byte)7)
                );

            return matcher.InstructionEnumeration();
        }

        public static IEnumerable<CodeInstruction> loadCurrentDialogue_Transpiler(
            IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);
            var helper = AccessTools.Method(typeof(NPCDialoguePatches),
                nameof(LoadCurrentDialogueTranspilerHelper));
            var tryToRetrieveDialogue = AccessTools.Method(typeof(NPC), nameof(NPC.tryToRetrieveDialogue));
            var getSeason = AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.currentSeason));
            var extraDialogueThisMorning = AccessTools.Field(typeof(NPC), "extraDialogueMessageToAddThisMorning");

            matcher.MatchStartForward(
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, extraDialogueThisMorning))
                .ThrowIfNotMatch($"Cannot find forward match entrypoint for {nameof(loadCurrentDialogue_Transpiler)}")
                .CreateLabel(out var label);
            
            var labels = matcher.MatchStartBackwards(
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Call, getSeason),
                    new CodeMatch(OpCodes.Ldstr, "_"),
                    new CodeMatch(OpCodes.Call), // string concat
                    new CodeMatch(OpCodes.Ldloc_2),
                    new CodeMatch(OpCodes.Ldstr, ""),
                    new CodeMatch(OpCodes.Call, tryToRetrieveDialogue))
                .ThrowIfNotMatch($"Cannot find backwards match entrypoint for {nameof(loadCurrentDialogue_Transpiler)}")
                .Labels;
                
                
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call, helper),
                    new CodeInstruction(OpCodes.Brtrue, label))
                    .AddLabels(labels);

            return matcher.InstructionEnumeration();
        }
        
        public static int ChangeConversationFriendshipAmount(int amount, Farmer who, Friendship friendship)
        {
            if (who.hasBuff("statue_of_blessings_4")) return amount;
            return friendship.Points <= -250 ? -amount : amount;
        }

        public static Dialogue? NegativeLocationDialogue(NPC npc, Dialogue? currDialogue, int heartLevel, string preface)
        {
            if (currDialogue is not null) return currDialogue;

            int hearts = -10;
            while (currDialogue is null && hearts <= -2 && heartLevel <= hearts)
            {
                currDialogue = npc.TryGetDialogue(preface + Game1.currentLocation.Name + hearts);
                hearts += 2;
            }

            return currDialogue;
        }

        public static bool LoadCurrentDialogueTranspilerHelper(NPC npc, int heartLevel, Stack<Dialogue> currentDialogue)
        {
            var seasonal = TryToRetrieveNegativeDailyDialogue(npc, Game1.currentSeason + "_", heartLevel);
            if (seasonal != null && seasonal.TranslationKey.Contains("NegativeHeartDialogue"))
            {
                currentDialogue.Push(seasonal);
                return true;
            }
            
            var nonSeasonal = TryToRetrieveNegativeDailyDialogue(npc, "", heartLevel);
            if (nonSeasonal != null && nonSeasonal.TranslationKey.Contains("NegativeHeartDialogue"))
            {
                currentDialogue.Push(nonSeasonal);
                return true;
            }

            return false;
        }
        
        public static Dialogue? TryToRetrieveNegativeDailyDialogue(NPC npc, string preface, int heartLevel, string appendToEnd = "")
        {
            if (!string.IsNullOrEmpty(Game1.player.spouse) && appendToEnd.Equals(""))
            {
                if (Game1.player.hasCurrentOrPendingRoommate())
                {
                    var roommateDialogue = TryToRetrieveNegativeDailyDialogue(npc, preface, heartLevel, "_roommate_" + Game1.player.spouse);
                    if (roommateDialogue != null)
                    {
                        return roommateDialogue;
                    }
                }
                else
                {
                    var inlawDialogue = TryToRetrieveNegativeDailyDialogue(npc, preface, heartLevel, "_inlaw_" + Game1.player.spouse);
                    if (inlawDialogue != null)
                    {
                        return inlawDialogue;
                    }
                }
            }
            
            var dayName = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
            var year = Game1.year;
            if (Game1.year > 2)
            {
                year = 2;
            }
            
            if (year == 1)
            {
                var yearOneDayOfMonthDialogue = npc.TryGetDialogue(preface + Game1.dayOfMonth);
                if (yearOneDayOfMonthDialogue != null)
                {
                    return yearOneDayOfMonthDialogue;
                }
            }
            var firstOrLaterYearDayOfMonthDialogue = npc.TryGetDialogue(preface + Game1.dayOfMonth + "_" + year);
            if (firstOrLaterYearDayOfMonthDialogue != null)
            {
                return firstOrLaterYearDayOfMonthDialogue;
            }
            for (var hearts = -10; hearts <= -2; hearts += 2)
            {
                if (heartLevel <= hearts)
                {
                    var heartDialogue = npc.TryGetDialogue(preface + dayName + hearts + "_" + year) ?? npc.TryGetDialogue(preface + dayName + hearts);
                    if (heartDialogue != null)
                    {
                        return heartDialogue;
                    }
                }
            }
            var dayNameDialogue = npc.TryGetDialogue(preface + dayName);
            var firstOrLaterYearDayNameDialogue = npc.TryGetDialogue(preface + dayName + "_" + year);
            return firstOrLaterYearDayNameDialogue ?? dayNameDialogue;
        }
    }
}