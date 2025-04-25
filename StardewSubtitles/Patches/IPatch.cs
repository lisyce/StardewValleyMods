using HarmonyLib;

namespace StardewSubtitles.Patches;

public interface IPatch
{
    public void Patch(Harmony harmony);
}