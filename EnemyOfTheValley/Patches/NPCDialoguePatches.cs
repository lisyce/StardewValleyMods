using HarmonyLib;
using StardewValley;
using System.Reflection.Emit;
using System.Reflection;


namespace EnemyOfTheValley.Patches
{
    internal class NPCDialoguePatches
    {
        public static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.checkForNewCurrentDialogue)),
                transpiler: new HarmonyMethod(typeof(NPCActionPatches), nameof(checkForNewCurrentDialogue_Transpiler))
                );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.grantConversationFriendship)),
                transpiler: new HarmonyMethod(typeof(NPCActionPatches), nameof(grantConversationFriendship_Transpiler))
                );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToRetrieveDialogue)),
                transpiler: new HarmonyMethod(typeof(NPCActionPatches), nameof(tryToRetrieveDialogue_Transpiler))
                );
        }

        public static IEnumerable<CodeInstruction> grantConversationFriendship_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new(instructions);
            MethodInfo changeAmt = AccessTools.Method(typeof(NPCActionPatches), nameof(ChangeConversationFriendshipAmount));

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
       
        public static IEnumerable<CodeInstruction> tryToRetrieveDialogue_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new(instructions, generator);
            MethodInfo negativeDayDialogue = AccessTools.Method(typeof(NPCActionPatches), nameof(NegativeDayDialogue));

            // we want to insert after the for-loop
            matcher.MatchEndForward(
               new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_2),
                new CodeMatch(OpCodes.Sub),
                new CodeMatch(OpCodes.Stloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_2),
                new CodeMatch(OpCodes.Bge)
                )
                .ThrowIfNotMatch("could not find end of for loop to insert after")
                .Advance(1)
                .CreateLabel(out Label jmpLabel)
                .Insert(
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldarg_2),
                    new(OpCodes.Ldarg_1),
                    new(OpCodes.Ldarg_3),
                    new(OpCodes.Ldloc_S, (byte)1),
                    new(OpCodes.Ldloc_S, (byte)0),
                    new(OpCodes.Call, negativeDayDialogue),
                    new(OpCodes.Stloc_S, (byte)8),
                    new(OpCodes.Ldloc_S, (byte)8),
                    new(OpCodes.Brfalse, jmpLabel),
                    new(OpCodes.Ldloc_S, (byte)8),
                    new(OpCodes.Ret));


            return matcher.InstructionEnumeration();
        }

        public static IEnumerable<CodeInstruction> checkForNewCurrentDialogue_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo negativeLocDialogue = AccessTools.Method(typeof(NPCActionPatches), nameof(NegativeLocationDialogue));
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

        public static int ChangeConversationFriendshipAmount(int amount, Farmer who, Friendship friendship)
        {
            if (who.hasBuff("statue_of_blessings_4")) return amount;
            return friendship.Points <= -500 ? -amount : amount;
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

        public static Dialogue? NegativeDayDialogue(NPC npc, int heartLevel, string preface, string appendToEnd, string day_name, int year)
        {
            for (int hearts = -10; hearts <= -2; hearts += 2)
            {
                if (heartLevel <= hearts)
                {
                    Dialogue? d = npc.TryGetDialogue(preface + day_name + hearts + "_" + year + appendToEnd) ?? npc.TryGetDialogue(preface + day_name + hearts + appendToEnd);
                    ModEntry.Monitor.Log(preface + day_name + hearts + "_" + year + appendToEnd, StardewModdingAPI.LogLevel.Debug);
                    ModEntry.Monitor.Log(preface + day_name + hearts + appendToEnd, StardewModdingAPI.LogLevel.Debug);
                    if (d != null) return d;
                }
            }
            return null;
        }
    }
}
