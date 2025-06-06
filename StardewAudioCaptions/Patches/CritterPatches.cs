using HarmonyLib;
using StardewAudioCaptions.Captions;
using StardewModdingAPI;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Locations;

namespace StardewAudioCaptions.Patches;

public class CritterPatches : ICaptionPatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Birdie), "playFlap"),
            new Caption("batFlap", "critters.birdFlap"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Birdie), "playPeck"),
            new Caption("shiny4", "critters.birdPeck"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Butterfly), nameof(Butterfly.update)),
            new Caption("yoba", "critters.butterflySparkle"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(CrabCritter), nameof(CrabCritter.update)),
            new Caption("dropItemInWater", "critters.crabSplash"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Crow), "playFlap"),
            new Caption("batFlap", "critters.birdFlap"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Crow), "playPeck"),
            new Caption("shiny4", "critters.birdPeck"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Crow), nameof(Crow.update)),
            new Caption("crow", "critters.crow"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(EmilysParrot), nameof(EmilysParrot.doAction)),
            new Caption("parrot", "critters.parrot"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(Frog), nameof(Frog.update)),
            new Caption("croak", "critters.frogCroak"),
            new Caption("dropItemInWater", "critters.frogSplash"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(OverheadParrot), nameof(OverheadParrot.update)),
            new Caption("parrot_flap", "critters.birdFlap"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(ParrotPlatform.Parrot), nameof(ParrotPlatform.Parrot.Update)),
            new Caption("batFlap", "critters.birdFlap"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(ParrotPlatform), nameof(ParrotPlatform.StartDeparture)),
            new Caption("parrot", "critters.parrot"));
                
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(ParrotUpgradePerch.Parrot), nameof(ParrotUpgradePerch.Parrot.Update)),
            new Caption("parrot_flap", "critters.birdFlap"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(SandDuggy), nameof(SandDuggy.AnimateWhacked)),
            new Caption("axchop", "critters.sandDuggyWhack"));
        
        PatchGenerator.TranspilerPatch(
            harmony,
            monitor,
            AccessTools.Method(typeof(SandDuggy), nameof(SandDuggy.Update)),
            SandDuggyUpdateTranspiler);
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(SebsFrogs), nameof(SebsFrogs.update)),
            new Caption("croak", "critters.frogCroak"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Woodpecker), "playFlap"),
            new Caption("batFlap", "critters.birdFlap"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Woodpecker), "playPeck"),
            new Caption("Cowboy_gunshot", "critters.birdPeck"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(CommunityCenter), nameof(CommunityCenter.UpdateWhenCurrentLocation)),
            new Caption("dustMeep", "critters.junimo"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Junimo), "junimoReachedHutToReturnBundle"),
            new Caption("Ship", "critters.junimoBundle"));
    }

    private static IEnumerable<CodeInstruction> SandDuggyUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new SoundCueCodeMatcher(instructions);
        matcher.FindCue("cowboy_gopher", SoundCueCodeMatcher.DelayedActionPlaySound)
            .RegisterCaptionForNextCue("cowboy_gopher", "critters.sandDuggyHide")
            .RegisterCaptionForNextCue("tinyWhip", "critters.sandDuggyHide");
        return matcher.InstructionEnumeration();
    }
}