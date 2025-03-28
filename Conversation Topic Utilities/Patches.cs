using System.Reflection.Emit;
using HarmonyLib;
using StardewValley;

namespace Conversation_Topic_Utilities;

public class Patches
{
    public static void Patch(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.Method(typeof(NPC), nameof(NPC.checkForNewCurrentDialogue)),
            transpiler: new HarmonyMethod(typeof(Patches), nameof(CheckForNewCurrentDialogueTranspiler)));
    }

    public static IEnumerable<CodeInstruction> CheckForNewCurrentDialogueTranspiler(
        IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);
        var helper = AccessTools.Method(typeof(Patches), nameof(CheckForNewCurrentDialogueHelper));
        var tryGetDialogue = AccessTools.Method(typeof(NPC), nameof(NPC.TryGetDialogue), new []{typeof(string)});

        matcher.MatchStartForward(
                new CodeMatch(OpCodes.Br))
            .MatchEndForward(
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldloc_2),
                new CodeMatch(OpCodes.Call, tryGetDialogue),
                new CodeMatch(OpCodes.Stloc_3))
            .ThrowIfNotMatch($"Could not find place to insert for {nameof(CheckForNewCurrentDialogueTranspiler)}")
            .Insert(
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, helper));  // this gets inserted right before stloc.3
        
        return matcher.InstructionEnumeration();
    }

    public static Dialogue? CheckForNewCurrentDialogueHelper(Dialogue? dialogue, string topicKey, NPC npc)
    {
        return dialogue ?? Util.TryGetDefaultCtDialogue(topicKey, npc);
    }
}