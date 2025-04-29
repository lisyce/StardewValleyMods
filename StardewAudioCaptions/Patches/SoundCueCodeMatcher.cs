using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewAudioCaptions.Captions;
using StardewValley;
using StardewValley.Network;

namespace StardewAudioCaptions.Patches;

public class SoundCueCodeMatcher
{
    private CodeMatcher _matcher;

    public static readonly MethodInfo GameLocationPlaySound =
        AccessTools.Method(typeof(GameLocation), nameof(GameLocation.playSound));

    public static readonly MethodInfo NetAudioStartPlaying =
        AccessTools.Method(typeof(NetAudio), nameof(NetAudio.StartPlaying));
    
    public SoundCueCodeMatcher(IEnumerable<CodeInstruction> instructions)
    {
        _matcher = new CodeMatcher(instructions);
    }

    public SoundCueCodeMatcher FindCue(string cueId, MethodInfo playSoundMethod)
    {
        _matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, cueId))
            .ThrowIfNotMatch($"Couldn't find call to ldstr {cueId}")
            .SearchForward(instruction => (instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt) &&
                                          instruction.operand is MethodInfo mi && mi.Equals(playSoundMethod))
            .ThrowIfNotMatch($"Couldn't find call to method {playSoundMethod.Name}");
        return this;
    }

    public SoundCueCodeMatcher RegisterCaptionForNextCue(string cueId, string captionId, int duration, bool shouldLog = true)
    {
        var helper = AccessTools.Method(typeof(SoundCueCodeMatcher), nameof(RegisterCaptionForNextCueHelper));
        var captionCtor = AccessTools.Constructor(typeof(Caption), new []{ typeof(string), typeof(string), typeof(int?), typeof(bool) });
        var nullableCtor = AccessTools.Constructor(typeof(Nullable<int>), new[] { typeof(int) });
        
        _matcher.Insert(
            new CodeInstruction(OpCodes.Ldstr, cueId),
            new CodeInstruction(OpCodes.Ldstr, captionId),
            new CodeInstruction(OpCodes.Ldc_I4, duration),
            new CodeInstruction(OpCodes.Newobj, nullableCtor),
            new CodeInstruction(OpCodes.Ldc_I4, shouldLog ? 1 : 0),
            new CodeInstruction(OpCodes.Newobj, captionCtor),
            new CodeInstruction(OpCodes.Call, helper))
            .Advance(1);
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