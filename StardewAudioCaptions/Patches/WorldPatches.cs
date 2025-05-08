using HarmonyLib;
using StardewModdingAPI;
using StardewAudioCaptions.Captions;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

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
            AccessTools.Constructor(typeof(MineElevatorMenu)),
            new Caption("crystal", "world.elevator"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(JumpingFish), nameof(JumpingFish.Splash)),
            new Caption("dropItemInWater", "world.fishSplash"));
        
    
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
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Tree), nameof(Tree.performToolAction)),
            new Caption("moss_cut", "world.moss"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performOrePanTenMinuteUpdate)),
            new Caption("slosh", "world.panSpot", shouldLog: false));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(FishingRod), nameof(FishingRod.draw)),
            new Caption("waterSlosh", "world.waterSlosh"));

    }
    
    private static IEnumerable<CodeInstruction> ObjectPlacementActionTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        // there are 3 bomb types to match
        var matcher = new SoundCueCodeMatcher(instructions);
        matcher.FindCue("fuse", SoundCueCodeMatcher.NetAudioStartPlaying)
            .RegisterCaptionForNextCue("fuse", "world.fuseHiss")
            .FindCue("fuse", SoundCueCodeMatcher.NetAudioStartPlaying)
            .RegisterCaptionForNextCue("fuse", "world.fuseHiss")
            .FindCue("fuse", SoundCueCodeMatcher.NetAudioStartPlaying)
            .RegisterCaptionForNextCue("fuse", "world.fuseHiss");

        return matcher.InstructionEnumeration();
    }

    private static IEnumerable<CodeInstruction> TasUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new SoundCueCodeMatcher(instructions);
        matcher.FindCue("explosion", SoundCueCodeMatcher.GameLocationPlaySound)
            .RegisterCaptionForNextCue("explosion", "world.bombExplode");
        return matcher.InstructionEnumeration();
    }
    
    private static IEnumerable<CodeInstruction> GrassPerformToolActionTranspiler(
        IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new SoundCueCodeMatcher(instructions);
        matcher.FindCue("daggerswipe", SoundCueCodeMatcher.DelayedActionPlaySound)
            .RegisterCaptionForNextCue("daggerswipe", "world.grassRustle")
            .FindCue("swordswipe", SoundCueCodeMatcher.GameLocationPlaySound)
            .RegisterCaptionForNextCue("swordswipe", "world.grassRustle");
        return matcher.InstructionEnumeration();
    }
}