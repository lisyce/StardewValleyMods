using HarmonyLib;
using StardewAudioCaptions.Captions;
using StardewModdingAPI;
using StardewValley.Locations;

namespace StardewAudioCaptions.Patches;

public class MonsterPatches : ICaptionPatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(MineShaft), nameof(MineShaft.spawnFlyingMonsterOffScreen)),
            new Caption("serpentDie", "monsters.serpentSpawn"),
            new Caption("rockGolemHit", "monsters.skullSpawn"));
    }
}