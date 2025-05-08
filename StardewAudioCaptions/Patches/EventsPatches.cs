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
            new Caption("thunder_small", "events.earthquake"),
            new Caption("windstorm", "events.raccoon0"),
            new Caption("windstorm", "events.raccoon1"),
            new Caption("UFO", "events.ufo"),
            new Caption("Meteorite", "events.meteorite"),
            new Caption("dogs", "events.dogs"),
            new Caption("owl", "events.owl"));
    }
}