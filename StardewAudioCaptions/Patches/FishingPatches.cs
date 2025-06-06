using HarmonyLib;
using StardewAudioCaptions.Captions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace StardewAudioCaptions.Patches;

public class FishingPatches : ICaptionPatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(FishingRod), nameof(FishingRod.tickUpdate)),
            new Caption("cast", "fishing.cast"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(FishingRod), nameof(FishingRod.DoFunction)),
            new Caption("dropItemInWater", "fishing.splash"),
            new Caption("FishHit", "fishing.hit"));
        
        PatchGenerator.GeneratePrefix(
            harmony,
            monitor,
            AccessTools.Method(typeof(Farmer), nameof(Farmer.PlayFishBiteChime)),
            new Caption(CaptionManager.AnyCue, "fishing.bite"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(BobberBar), nameof(BobberBar.update)),
            new Caption("fastReel", "fishing.reeling"),
            new Caption("slowReel", "fishing.reeling"),
            new Caption("fishingRodBend", "fishing.reeling"),
            new Caption("fishEscape", "fishing.escape"),
            new Caption("jingle1", "fishing.caught"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(FishingRod), nameof(FishingRod.openChestEndFunction)),
            new Caption("discoverMineral", "fishing.troutTag"));
    }
}