using HarmonyLib;
using Microsoft.Xna.Framework;
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
        
        harmony.Patch(
            original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.PlayMusic)),
            prefix: new HarmonyMethod(typeof(AudioPatches), nameof(EventPlaySoundPrefix))
        );
        
        harmony.Patch(
            original: AccessTools.Method(typeof(Game1), nameof(Game1.updateMusic)),
            postfix: new HarmonyMethod(typeof(AudioPatches), nameof(UpdateMusicPostfix))
        );
    }

    private static void PlayPostfix(Cue __instance)
    {
        ModEntry.ModCaptionManager.OnSoundPlayed(new CueWrapper(__instance));
    }

    private static void EventPlaySoundPrefix()
    {
        ModEntry.EventCaptionManager.BeforePlaySound();
    }

    private static void UpdateMusicPostfix()
    {
        if (Game1.currentSong != null)
        {
            ModEntry.ModCaptionManager.OnSoundPlayed(Game1.currentSong);
        }
    }
}