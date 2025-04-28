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
            new Caption("coin", "environment.itemCollect", shouldLog: false));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(FarmerSprite), "checkForFootstep"),
            new Caption("thudStep", "player.footsteps", shouldLog: false),
            new Caption("thudStep", "player.footsteps", shouldLog: false),
            new Caption("sandyStep", "player.footsteps", shouldLog: false),
            new Caption("stoneStep", "player.footsteps", shouldLog: false),
            new Caption("snowyStep", "player.footsteps", shouldLog: false),
            new Caption("grassyStep", "player.footsteps", shouldLog: false),
            new Caption("woodyStep", "player.footsteps", shouldLog: false));
    }
}