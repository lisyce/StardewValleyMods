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
            new Caption("windstorm", "events.racoon0", 9 * 60),
            new Caption("windstorm", "events.racoon1", 9 * 60),
            new Caption("UFO", "events.ufo", 3 * 60),
            new Caption("Meteorite", "events.meteorite", 3 * 60),
            new Caption("dogs", "events.dogs", 4 * 60),
            new Caption("owl", "events.owl"));
    }
}