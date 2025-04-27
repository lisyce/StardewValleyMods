using HarmonyLib;
using StardewModdingAPI;
using StardewSubtitles.Subtitles;
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
            new Subtitle("thunder_small", "events.earthquake"),
            new Subtitle("windstorm", "events.racoon0", 9 * 60),
            new Subtitle("windstorm", "events.racoon1", 9 * 60),
            new Subtitle("UFO", "events.ufo", 3 * 60),
            new Subtitle("Meteorite", "events.meteorite", 3 * 60),
            new Subtitle("dogs", "events.dogs", 4 * 60),
            new Subtitle("owl", "events.owl"));
    }
}