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
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(FairyEvent), nameof(FairyEvent.tickUpdate)),
            new Caption("batFlap", "nightEvents.fairyFlap", shouldLog: false),
            new Caption("yoba", "nightEvents.fairySparkle", shouldLog: false));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(WitchEvent), nameof(WitchEvent.setUp)),
            new Caption("cacklingWitch", "nightEvents.witchCackle"),
            new Caption("yoba", "nightEvents.witchSparkle"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(WitchEvent), nameof(WitchEvent.tickUpdate)),
            new Caption("discoverMineral", "nightEvents.witchSpell", shouldLog: false),
            new Caption("debuffSpell", "nightEvents.witchSpell", shouldLog: false));
    }
}