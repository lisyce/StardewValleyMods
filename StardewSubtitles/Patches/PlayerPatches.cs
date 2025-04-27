using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace StardewSubtitles.Patches;

public class PlayerPatches : ISubtitlePatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Debris), nameof(Debris.updateChunks)),
            "coin",
            "environment.itemCollect");
    }
}