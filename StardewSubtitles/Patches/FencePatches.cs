using HarmonyLib;
using StardewValley;

namespace StardewSubtitles.Patches;

public class FencePatches : ISubtitlePatch
{
    public void Patch(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.Method(typeof(Fence), nameof(Fence.toggleGate),
                new[] { typeof(bool), typeof(bool), typeof(Farmer) }),
            transpiler: new HarmonyMethod(typeof(FencePatches), nameof(ToggleGateTranspiler))
        );
    }

    private static IEnumerable<CodeInstruction> ToggleGateTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new SoundCueCodeMatcher(instructions);
        matcher.FindCue("doorClose", SoundCueCodeMatcher.GameLocationPlaySound)
            .RegisterSubtitleForNextCue("doorClose", "interaction.fenceGate");
        
        return matcher.InstructionEnumeration();
    }
}