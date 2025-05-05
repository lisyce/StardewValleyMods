using HarmonyLib;
using StardewAudioCaptions.Captions;
using StardewModdingAPI;
using StardewValley.Characters;

namespace StardewAudioCaptions.Patches;

public class PetPatches : ICaptionPatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Pet), nameof(Pet.applyButterflyPowder)),
            new Caption("fireball", "pets.butterflyPowder"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Pet), nameof(Pet.OnNewBehavior)),
            new Caption("dwop", "pets.jump"));
    }
}