using HarmonyLib;
using StardewModdingAPI;

namespace StardewSubtitles.Patches;

public class PatchManager
{
    private IMonitor _monitor;
    private Harmony _harmony;
    public PatchManager(IMonitor monitor, Harmony harmony)
    {
        _monitor = monitor;
        _harmony = harmony;
    }

    public void Patch()
    {
        var type = typeof(IPatch);
        var toInstantiate = System.Reflection.Assembly
            .GetExecutingAssembly().GetTypes()
            .Where(t => t.GetInterfaces().Contains(type));
        
        foreach (var patchType in toInstantiate)
        {
            try
            {
                var patcher = Activator.CreateInstance(patchType);
                if (patcher is not IPatch iPatch)
                {
                    _monitor.Log($"Could not apply subtitle patches for type {patchType}.", LogLevel.Warn);
                    continue;
                }

                iPatch.Patch(_harmony);
                _monitor.Log($"Applied harmony patch {patcher.GetType()}", LogLevel.Debug);
            }
            catch (Exception e)
            {
                _monitor.Log($"Failed to apply harmony patch {patchType}; skipping these subtitles.", LogLevel.Warn);
                _monitor.Log($"Error: {e}", LogLevel.Warn);
            }
        }
    }
}