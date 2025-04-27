using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewAudioCaptions.Captions;
using StardewAudioCaptions.Captions;
using StardewValley;

namespace StardewAudioCaptions.Patches;

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

    public SoundCueCodeMatcher RegisterCaptionForNextCue(string cueId, string captionId)
    {
        var helper = AccessTools.Method(typeof(SoundCueCodeMatcher), nameof(RegisterCaptionForNextCueHelper));

        _matcher.Insert(
            new CodeInstruction(OpCodes.Ldstr, cueId),
            new CodeInstruction(OpCodes.Ldstr, captionId),
            new CodeInstruction(OpCodes.Call, helper));
        return this;
    }

    public IEnumerable<CodeInstruction> InstructionEnumeration()
    {
        return _matcher.InstructionEnumeration();
    }

    private static void RegisterCaptionForNextCueHelper(Caption caption)
    {
        ModEntry.CaptionManager.RegisterCaptionForNextCue(caption);
    }
}