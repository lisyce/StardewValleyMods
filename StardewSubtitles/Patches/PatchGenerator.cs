using System.Reflection;
using HarmonyLib;
using StardewModdingAPI;

namespace StardewSubtitles.Patches;

public class PatchGenerator
{
    private static readonly Dictionary<MethodBase, List<(string cueId, string subtitleId)>> SubtitleLookup = new();
    
    public static void GeneratePatchPair(Harmony harmony, IMonitor monitor, MethodInfo original, string cueId, string subtitleId)
    {
        try
        {
            if (!SubtitleLookup.ContainsKey(original)) SubtitleLookup.Add(original, new List<(string, string)>());

            var list = SubtitleLookup[original];
            list.Add((cueId, subtitleId));
        
            harmony.Patch(
                original: original,
                prefix: new HarmonyMethod(typeof(PatchGenerator), nameof(Prefix)),
                finalizer: new HarmonyMethod(typeof(PatchGenerator), nameof(Finalizer))
            ); 
        }
        catch (Exception e)
        {
            monitor.Log($"Failed to apply harmony patch on {original.Name}; skipping these subtitles.", LogLevel.Warn);
            monitor.Log($"Error: {e}", LogLevel.Warn);
        }
    }

    public static void GeneratePatchPairs(Harmony harmony, IMonitor monitor, MethodInfo original,
        List<(string cueId, string subtitleId)> pairs)
    {
        foreach (var (cueId, subtitleId) in pairs)
        {
            GeneratePatchPair(harmony, monitor, original, cueId, subtitleId);
        }
    }
    
    private static void Prefix(MethodBase __originalMethod)
    {
        if (!SubtitleLookup.TryGetValue(__originalMethod, out var pairs)) return;  // should never happen

        foreach (var (cueId, subtitleId) in pairs)
        {
            ModEntry._subtitleManager.RegisterSubtitleForNextCue(cueId, subtitleId);
        }
    }

    private static void Finalizer(MethodBase __originalMethod)
    {
        if (!SubtitleLookup.TryGetValue(__originalMethod, out var pairs)) return;  // should never happen

        foreach (var (cueId, subtitleId) in pairs)
        {
            ModEntry._subtitleManager.UnRegisterSubtitleForNextCue(cueId, subtitleId);
        }
    }
    
    
}