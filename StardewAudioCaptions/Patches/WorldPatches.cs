using HarmonyLib;
using StardewModdingAPI;
using StardewAudioCaptions.Captions;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;

namespace StardewAudioCaptions.Patches;

public class WorldPatches : ICaptionPatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(MineShaft), nameof(MineShaft.UpdateWhenCurrentLocation)),
            new Caption("crystal", "world.elevator", shouldLog: false));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(JumpingFish), nameof(JumpingFish.Splash)),
            new Caption("dropItemInWater", "world.fishSplash"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.UpdateWhenCurrentLocation)),
            new Caption("dropItemInWater", "world.fishSplash", shouldLog: false));
    
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(MineShaft), nameof(MineShaft.createLadderAt)),
            new Caption("sandyStep", "world.ladderAppear"),
            new Caption("hoeHit", "world.ladderAppear"),
            new Caption("newArtifact", "world.ladderAppear"));
        
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
        
        PatchGenerator.TranspilerPatch(
            harmony,
            monitor,
            AccessTools.Method(typeof(Grass), nameof(Grass.performToolAction)),
            GrassPerformToolActionTranspiler);
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Tree), "performTreeFall"),
            new Caption("treecrack", "world.treeCrack"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.cutWeed)),
            new Caption("cut", "world.weedRustle"),
            new Caption("weed_cut", "world.weedRustle"),
            new Caption("breakingGlass", "world.crystalShatter"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(ParrotPlatform), nameof(ParrotPlatform.Update)),
            new Caption("treethud", "world.parrotExpressLand"));

    }
    
    private static IEnumerable<CodeInstruction> ObjectPlacementActionTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        // there are 3 bomb types to match
        var matcher = new SoundCueCodeMatcher(instructions);
        matcher.FindCue("fuse", SoundCueCodeMatcher.NetAudioStartPlaying)
            .RegisterCaptionForNextCue("fuse", "world.fuseHiss", CaptionManager.InfiniteDuration)
            .FindCue("fuse", SoundCueCodeMatcher.NetAudioStartPlaying)
            .RegisterCaptionForNextCue("fuse", "world.fuseHiss", CaptionManager.InfiniteDuration)
            .FindCue("fuse", SoundCueCodeMatcher.NetAudioStartPlaying)
            .RegisterCaptionForNextCue("fuse", "world.fuseHiss", CaptionManager.InfiniteDuration);

        return matcher.InstructionEnumeration();
    }

    private static IEnumerable<CodeInstruction> TasUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new SoundCueCodeMatcher(instructions);
        matcher.FindCue("explosion", SoundCueCodeMatcher.GameLocationPlaySound)
            .RegisterCaptionForNextCue("explosion", "world.bombExplode", CaptionManager.InfiniteDuration);
        return matcher.InstructionEnumeration();
    }
    
    private static IEnumerable<CodeInstruction> GrassPerformToolActionTranspiler(
        IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new SoundCueCodeMatcher(instructions);
        matcher.FindCue("daggerswipe", SoundCueCodeMatcher.DelayedActionPlaySound)
            .RegisterCaptionForNextCue("daggerswipe", "world.grassRustle", CaptionManager.InfiniteDuration)
            .FindCue("swordswipe", SoundCueCodeMatcher.GameLocationPlaySound)
            .RegisterCaptionForNextCue("swordswipe", "world.grassRustle", CaptionManager.InfiniteDuration);
        return matcher.InstructionEnumeration();
    }
}