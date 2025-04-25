using HarmonyLib;

namespace StardewSubtitles.Patches;

public interface ISubtitlePatch
{
    public void Patch(Harmony harmony);
}