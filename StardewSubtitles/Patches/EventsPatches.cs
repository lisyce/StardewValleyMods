using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Events;

namespace StardewSubtitles.Patches;

public class EventsPatches : ISubtitlePatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(SoundInTheNightEvent), nameof(SoundInTheNightEvent.tickUpdate)),
            ("thunder_small", "events.earthquake"),
            ("windstorm", "events.racoon0"),
            ("windstorm", "events.racoon1"),
            ("UFO", "events.ufo"),
            ("Meteorite", "events.meteorite"),
            ("dogs", "events.dogs"),
            ("owl", "events.owl"));
    }
}