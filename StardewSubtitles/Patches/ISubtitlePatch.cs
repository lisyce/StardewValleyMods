using HarmonyLib;
using StardewModdingAPI;

namespace StardewSubtitles.Patches;

public interface ISubtitlePatch
{
    public void Patch(Harmony harmony, IMonitor monitor);
}