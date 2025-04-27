using System.Reflection;
using HarmonyLib;
using StardewModdingAPI;
using StardewSubtitles.Subtitles;

namespace StardewSubtitles.Patches;

public class PatchGenerator
{
    private static readonly Dictionary<MethodBase, List<Subtitle>> SubtitleLookup = new();
    
    public static void GeneratePatchPair(Harmony harmony, IMonitor monitor, MethodInfo original, Subtitle subtitle)
    {
        try
        {
            if (!SubtitleLookup.ContainsKey(original))
            {
                SubtitleLookup.Add(original, new List<Subtitle>());
                
                // we only want to patch once!
                harmony.Patch(
                    original: original,
                    prefix: new HarmonyMethod(typeof(PatchGenerator), nameof(Prefix)),
                    finalizer: new HarmonyMethod(typeof(PatchGenerator), nameof(Finalizer))
                );
            }

            var list = SubtitleLookup[original];
            list.Add(subtitle);
            monitor.Log($"Registered prefix/finalizer pair for {original.DeclaringType?.Name + "::" ?? ""}{original.Name}. cueId: {subtitle.CueId}, subtitleId: {subtitle.SubtitleId}");
        }
        catch (Exception e)
        {
            monitor.Log($"Failed to apply harmony patch on {original.Name}; skipping these subtitles.", LogLevel.Warn);
            monitor.Log($"Error: {e}", LogLevel.Warn);
        }
    }

    public static void GeneratePatchPairs(Harmony harmony, IMonitor monitor, MethodInfo original,
        params Subtitle[] subtitles)
    {
        foreach (var subtitle in subtitles)
        {
            GeneratePatchPair(harmony, monitor, original, subtitle);
        }
    }
    
    private static void Prefix(MethodBase __originalMethod)
    {
        if (!SubtitleLookup.TryGetValue(__originalMethod, out var subtitles)) return;  // should never happen

        foreach (var subtitle in subtitles)
        {
            ModEntry._subtitleManager.RegisterSubtitleForNextCue(subtitle);
        }
    }

    private static void Finalizer(MethodBase __originalMethod)
    {
        if (!SubtitleLookup.TryGetValue(__originalMethod, out var subtitles)) return;  // should never happen

        foreach (var subtitle in subtitles)
        {
            ModEntry._subtitleManager.UnRegisterSubtitleForNextCue(subtitle.CueId, subtitle.SubtitleId);
        }
    }
    
    
}