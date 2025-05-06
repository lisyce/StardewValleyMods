using HarmonyLib;
using StardewAudioCaptions.Captions;
using StardewModdingAPI;
using StardewValley;

namespace StardewAudioCaptions.Patches;

public class AnimalPatches : ICaptionPatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.TranspilerPatch(
            harmony,
            monitor,
            AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.behaviors)),
            FarmAnimalBehaviorsTranspiler);
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.updateWhenCurrentLocation)),
            new Caption("dwoop", "animals.enterHouse", shouldLog: false));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.Splash)),
            new Caption("dropItemInWater", "animals.splash"));
    }

    private static IEnumerable<CodeInstruction> FarmAnimalBehaviorsTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new SoundCueCodeMatcher(instructions);
        matcher.FindCue("dirtyHit", SoundCueCodeMatcher.DelayedActionPlaySound)
            .RegisterCaptionForNextCue("dirtyHit", "animals.digging", CaptionManager.InfiniteDuration, false)
            .FindCue("dirtyHit", SoundCueCodeMatcher.DelayedActionPlaySound)
            .RegisterCaptionForNextCue("dirtyHit", "animals.digging", CaptionManager.InfiniteDuration, false)
            .FindCue("dirtyHit", SoundCueCodeMatcher.DelayedActionPlaySound)
            .RegisterCaptionForNextCue("dirtyHit", "animals.digging", CaptionManager.InfiniteDuration, false);
        return matcher.InstructionEnumeration();
    }
}