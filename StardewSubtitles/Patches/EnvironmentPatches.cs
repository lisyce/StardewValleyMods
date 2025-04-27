using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Locations;

namespace StardewSubtitles.Patches;

public class EnvironmentPatches : ISubtitlePatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(MineShaft), nameof(MineShaft.UpdateWhenCurrentLocation)),
            "crystal",
            "environment.elevator");
    }
}