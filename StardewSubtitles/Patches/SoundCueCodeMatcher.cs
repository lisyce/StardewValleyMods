using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley;

namespace StardewSubtitles.Patches;

public class SoundCueCodeMatcher
{
    private CodeMatcher _matcher;

    public static readonly MethodInfo GameLocationPlaySound =
        AccessTools.Method(typeof(GameLocation), nameof(GameLocation.playSound));
    
    public SoundCueCodeMatcher(IEnumerable<CodeInstruction> instructions)
    {
        _matcher = new CodeMatcher(instructions);
    }

    public SoundCueCodeMatcher FindCue(string cueId, MethodInfo playSoundMethod)
    {
        _matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, cueId))
            .SearchForward(instruction => instruction.opcode == OpCodes.Call &&
                                              instruction.operand is MethodInfo mi && mi.Equals(playSoundMethod));
        return this;
    }

    public SoundCueCodeMatcher RegisterSubtitleForNextCue(string cueId, string subtitleId)
    {
        var helper = AccessTools.Method(typeof(SoundCueCodeMatcher), nameof(RegisterSubtitleForNextCueHelper));

        _matcher.Insert(
            new CodeInstruction(OpCodes.Ldstr, cueId),
            new CodeInstruction(OpCodes.Ldstr, subtitleId),
            new CodeInstruction(OpCodes.Call, helper));
        return this;
    }

    public IEnumerable<CodeInstruction> InstructionEnumeration()
    {
        return _matcher.InstructionEnumeration();
    }

    private static void RegisterSubtitleForNextCueHelper(string cueId, string subtitleId)
    {
        ModEntry._subtitleManager.RegisterSubtitleForNextCue(cueId, subtitleId);
    }
}