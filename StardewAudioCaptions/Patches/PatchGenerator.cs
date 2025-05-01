using System.Reflection;
using HarmonyLib;
using StardewModdingAPI;
using StardewAudioCaptions.Captions;

namespace StardewAudioCaptions.Patches;

public class PatchGenerator
{
    private static readonly Dictionary<MethodBase, List<Caption>> CaptionLookup = new();
    
    public static void GeneratePatchPair(Harmony harmony, IMonitor monitor, MethodInfo original, Caption caption)
    {
        try
        {
            if (!CaptionLookup.ContainsKey(original))
            {
                CaptionLookup.Add(original, new List<Caption>());
                
                // we only want to patch once!
                harmony.Patch(
                    original: original,
                    prefix: new HarmonyMethod(typeof(PatchGenerator), nameof(Prefix)),
                    finalizer: new HarmonyMethod(typeof(PatchGenerator), nameof(Finalizer))
                );
            }

            var list = CaptionLookup[original];
            list.Add(caption);
            monitor.Log($"Registered prefix/finalizer pair for {original.DeclaringType?.Name + "::" ?? ""}{original.Name}. cueId: {caption.CueId}, captionId: {caption.CaptionId}");
        }
        catch (Exception e)
        {
            monitor.Log($"Failed to apply harmony patch on {original.Name}; skipping these captions.", LogLevel.Warn);
            monitor.Log($"Error: {e}", LogLevel.Debug);
        }
    }

    public static void GeneratePatchPairs(Harmony harmony, IMonitor monitor, MethodInfo original,
        params Caption[] captions)
    {
        foreach (var caption in captions)
        {
            GeneratePatchPair(harmony, monitor, original, caption);
        }
    }

    public static void GeneratePrefix(Harmony harmony, IMonitor monitor, MethodInfo original, Caption caption)
    {
        try
        {
            if (!CaptionLookup.ContainsKey(original))
            {
                CaptionLookup.Add(original, new List<Caption>());
                harmony.Patch(
                    original: original,
                    prefix: new HarmonyMethod(typeof(PatchGenerator), nameof(Prefix))
                );
            }

            var list = CaptionLookup[original];
            list.Add(caption);
            monitor.Log($"Registered prefix for {original.DeclaringType?.Name + "::" ?? ""}{original.Name}. cueId: {caption.CueId}, captionId: {caption.CaptionId}");
        }
        catch (Exception e)
        {
            monitor.Log($"Failed to apply harmony patch on {original.Name}; skipping these captions.", LogLevel.Warn);
            monitor.Log($"Error: {e}", LogLevel.Debug);
        }
    }

    public static void GeneratePrefixes(Harmony harmony, IMonitor monitor, MethodInfo original,
        params Caption[] captions)
    {
        foreach (var caption in captions)
        {
            GeneratePrefix(harmony, monitor, original, caption);
        }
    }

    public static void TranspilerPatch(Harmony harmony, IMonitor monitor, MethodInfo original,
        Func<IEnumerable<CodeInstruction>, IEnumerable<CodeInstruction>> transpiler)
    {
        try
        {
            harmony.Patch(
                original: original,
                transpiler: new HarmonyMethod(transpiler.GetMethodInfo())
            );
            monitor.Log($"Registered transpiler for {original.DeclaringType?.Name + "::" ?? ""}{original.Name}.");
        }
        catch (Exception e)
        {
            monitor.Log($"Failed to apply harmony patch on {original.Name}; skipping these captions.", LogLevel.Warn);
            monitor.Log($"Error: {e}", LogLevel.Debug);
        }
    }
    
    private static void Prefix(MethodBase __originalMethod)
    {
        if (!CaptionLookup.TryGetValue(__originalMethod, out var captions)) return;  // should never happen

        foreach (var caption in captions)
        {
            ModEntry.CaptionManager.RegisterCaptionForNextCue(caption);
        }
    }

    private static void Finalizer(MethodBase __originalMethod)
    {
        if (!CaptionLookup.TryGetValue(__originalMethod, out var captions)) return;  // should never happen

        foreach (var caption in captions)
        {
            ModEntry.CaptionManager.UnregisterCaptionForNextCue(caption);
        }
    }
    
    
}