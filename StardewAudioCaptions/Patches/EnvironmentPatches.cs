using HarmonyLib;
using StardewModdingAPI;
using StardewAudioCaptions.Captions;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace StardewAudioCaptions.Patches;

public class EnvironmentPatches : ICaptionPatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(MineShaft), nameof(MineShaft.UpdateWhenCurrentLocation)),
            new Caption("crystal", "environment.elevator", shouldLog: false));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(JumpingFish), nameof(JumpingFish.Splash)),
            new Caption("dropItemInWater", "environment.fishSplash"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.UpdateWhenCurrentLocation)),
            new Caption("dropItemInWater", "environment.fishSplash", shouldLog: false));
    
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(MineShaft), nameof(MineShaft.createLadderAt)),
            new Caption("sandyStep", "environment.ladderAppear"),
            new Caption("hoeHit", "environment.ladderAppear"),
            new Caption("newArtifact", "environment.ladderAppear"));
        
        PatchGenerator.TranspilerPatch(
            harmony,
            monitor,
            AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction)),
            ObjectPlacementActionTranspiler);
        
        PatchGenerator.TranspilerPatch(
            harmony,
            monitor,
            AccessTools.Method(typeof(TemporaryAnimatedSprite), nameof(TemporaryAnimatedSprite.update)),
            TasUpdateTranspiler);
    }
    
    private static IEnumerable<CodeInstruction> ObjectPlacementActionTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        // there are 3 bomb types to match
        var matcher = new SoundCueCodeMatcher(instructions);
        matcher.FindCue("fuse", SoundCueCodeMatcher.NetAudioStartPlaying)
            .RegisterCaptionForNextCue("fuse", "environment.fuseHiss", CaptionManager.InfiniteDuration)
            .FindCue("fuse", SoundCueCodeMatcher.NetAudioStartPlaying)
            .RegisterCaptionForNextCue("fuse", "environment.fuseHiss", CaptionManager.InfiniteDuration)
            .FindCue("fuse", SoundCueCodeMatcher.NetAudioStartPlaying)
            .RegisterCaptionForNextCue("fuse", "environment.fuseHiss", CaptionManager.InfiniteDuration);

        return matcher.InstructionEnumeration();
    }

    private static IEnumerable<CodeInstruction> TasUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new SoundCueCodeMatcher(instructions);
        matcher.FindCue("explosion", SoundCueCodeMatcher.GameLocationPlaySound)
            .RegisterCaptionForNextCue("explosion", "environment.bombExplode", CaptionManager.InfiniteDuration);
        return matcher.InstructionEnumeration();
    }
}