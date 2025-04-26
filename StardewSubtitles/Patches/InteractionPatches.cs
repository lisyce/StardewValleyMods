using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace StardewSubtitles.Patches;

public class InteractionPatches : ISubtitlePatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Fence), nameof(Fence.toggleGate),
                new[] { typeof(bool), typeof(bool), typeof(Farmer) }),
            "doorClose",
            "interaction.fenceGate"
            );
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.CheckGarbage)),
            "trashcan",
            "interaction.trashCan"
            );
    }
}