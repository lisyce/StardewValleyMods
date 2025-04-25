using HarmonyLib;
using StardewSubtitles.Subtitles;
using StardewValley.Audio;

namespace StardewSubtitles.Patches;

public class SoundsHelperPatches
{
    public static void Patch(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.Method(typeof(SoundsHelper), nameof(SoundsHelper.PlayLocal)),
            postfix: new HarmonyMethod(typeof(SoundsHelperPatches), nameof(PlayLocalPostfix))
            );
    }

    private static void PlayLocalPostfix(string cueName, bool __result)
    {
        if (__result)
        {
            ModEntry._subtitleManager.OnSoundPlayed(cueName);
        }
    }
}