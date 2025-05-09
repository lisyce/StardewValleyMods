using System.Reflection.Emit;
using HarmonyLib;
using StardewAudioCaptions.Captions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Machines;

namespace StardewAudioCaptions.Patches;

public class MachinePatches : ICaptionPatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.TranspilerPatch(
            harmony,
            monitor,
            AccessTools.Method(typeof(MachineDataUtility), nameof(MachineDataUtility.PlayEffects)),
            PlayEffectsTranspiler);
    }

    private static IEnumerable<CodeInstruction> PlayEffectsTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var fld = AccessTools.Field(typeof(MachineSoundData), nameof(MachineSoundData.Delay));
        var helper = AccessTools.Method(typeof(MachinePatches), nameof(PlayEffectsHelper));
        
        var matcher = new CodeMatcher(instructions);
        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldfld, fld),
                new CodeMatch(OpCodes.Ldc_I4_0))
            .ThrowIfNotMatch("Could not find sound.Delay <= 0 expression")
            .Insert(new CodeInstruction(OpCodes.Call, helper));

        return matcher.InstructionEnumeration();
    }

    private static void PlayEffectsHelper()
    {
        ModEntry.CaptionManager.RegisterCaptionForNextCue(
            new Caption(CaptionManager.AnyCue, 
                "world.machineProcessing"));
    }
}