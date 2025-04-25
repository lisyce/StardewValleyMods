using HarmonyLib;
using StardewValley;

namespace StardewSubtitles.Patches;

public class GameLocationPatches : ISubtitlePatch
{
    public void Patch(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.CheckGarbage)),
            transpiler: new HarmonyMethod(typeof(GameLocationPatches), nameof(CheckGarbageTranspiler))
        );
    }

    private static IEnumerable<CodeInstruction> CheckGarbageTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new SoundCueCodeMatcher(instructions);
        matcher.FindCue("trashcan", SoundCueCodeMatcher.GameLocationPlaySound)
            .RegisterSubtitleForNextCue("trashcan", "interaction.trashCan");
        return matcher.InstructionEnumeration();
    }
}