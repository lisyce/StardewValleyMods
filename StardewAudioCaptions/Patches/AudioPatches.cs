using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using StardewValley;

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

        harmony.Patch(
            original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.PlaySound)),
            prefix: new HarmonyMethod(typeof(AudioPatches), nameof(EventPlaySoundPrefix))
            );
    }

    private static void PlayPostfix(Cue __instance)
    {
        ModEntry.CaptionManager.OnSoundPlayed(__instance);
    }

    private static void EventPlaySoundPrefix()
    {
        ModEntry.EventCaptionManager.BeforePlaySound();
    }
}