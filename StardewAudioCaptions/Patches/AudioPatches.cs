using HarmonyLib;
using Microsoft.Xna.Framework.Audio;

namespace StardewAudioCaptions.Patches;

public class AudioPatches
{
    public static void Patch(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.Method(typeof(Cue), nameof(Cue.Play)),
            postfix: new HarmonyMethod(typeof(AudioPatches), nameof(PlayPostfix))
            );
        
        harmony.Patch(
            original: AccessTools.Method(typeof(Cue), nameof(Cue.Resume)),
            postfix: new HarmonyMethod(typeof(AudioPatches), nameof(PlayPostfix))
        );
    }

    private static void PlayPostfix(Cue __instance)
    {
        ModEntry.CaptionManager.OnSoundPlayed(__instance);
    }
}