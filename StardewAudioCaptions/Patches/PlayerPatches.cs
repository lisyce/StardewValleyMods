using HarmonyLib;
using StardewModdingAPI;
using StardewAudioCaptions.Captions;
using StardewValley;

namespace StardewAudioCaptions.Patches;

public class PlayerPatches : ICaptionPatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Debris), nameof(Debris.updateChunks)),
            new Caption("coin", "environment.itemCollect"));
    }
}