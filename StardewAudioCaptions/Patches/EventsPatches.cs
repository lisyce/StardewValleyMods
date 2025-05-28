using HarmonyLib;
using StardewModdingAPI;
using StardewAudioCaptions.Captions;
using StardewValley.Events;

namespace StardewAudioCaptions.Patches;

public class EventsPatches : ICaptionPatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(SoundInTheNightEvent), nameof(SoundInTheNightEvent.tickUpdate)),
            new Caption("thunder_small", "nightEvents.earthquake"),
            new Caption("windstorm", "nightEvents.raccoon0"),
            new Caption("windstorm", "nightEvents.raccoon1"),
            new Caption("UFO", "nightEvents.ufo"),
            new Caption("Meteorite", "nightEvents.meteorite"),
            new Caption("dogs", "nightEvents.dogs"),
            new Caption("owl", "nightEvents.owl"));
    }
}