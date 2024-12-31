using HarmonyLib;
using StardewValley;
using System.Reflection.Emit;

namespace EnemyOfTheValley.Patches
{
    internal class NPCPatches
    {
        public static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.checkForNewCurrentDialogue)),
                transpiler: new HarmonyMethod(typeof(NPCPatches), nameof(checkForNewCurrentDialogue_Transpiler))
                );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToRetrieveDialogue)),
                transpiler: new HarmonyMethod(typeof(NPCPatches), nameof(tryToRetrieveDialogue_Transpiler))
                );
        }

        public static IEnumerable<CodeInstruction> tryToRetrieveDialogue_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // match the hearts >= -2 condition in the for loop to make it a -10
            CodeMatcher matcher = new(instructions);

            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ret),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_2),
                new CodeMatch(OpCodes.Sub),
                new CodeMatch(OpCodes.Stloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_2))
                .ThrowIfNotMatch("could not find place to change for loop bounds")
                .Set(OpCodes.Ldc_I4, -10);

            return matcher.InstructionEnumeration();
        }

        public static IEnumerable<CodeInstruction> checkForNewCurrentDialogue_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // match the num -= 2, then the dialogue2 == null so we can replace the num (hearts) >= 2 with num >= -10
            // (NPC::checkForNewCurrentDialogue on line 3945) which is a while loop condition
            CodeMatcher matcher = new(instructions);

            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_2),
                new CodeMatch(OpCodes.Sub),
                new CodeMatch(OpCodes.Stloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Brtrue_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_2)
                )
                .ThrowIfNotMatch("could not find place to change while loop bounds")
                .Set(OpCodes.Ldc_I4, -10);

            return matcher.InstructionEnumeration();
        }
    }
}
