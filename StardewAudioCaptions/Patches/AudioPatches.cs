using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using StardewValley;
using StardewValley.GameData;

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
        
        harmony.Patch(
            original: AccessTools.Method(typeof(Event), nameof(Event.ReplaceAllCommands)),
            postfix: new HarmonyMethod(typeof(AudioPatches), nameof(EventReplaceAllCommandsPostfix))
        );
        
        harmony.Patch(
            original: AccessTools.Method(typeof(Game1), nameof(Game1.changeMusicTrack)),
            postfix: new HarmonyMethod(typeof(AudioPatches), nameof(ChangeMusicPrefix))
        );
    }

    private static void PlayPostfix(Cue __instance)
    {
        if (__instance == null) return;
        ModEntry.ModCaptionManager.OnSoundPlayed(new CueWrapper(__instance));
    }

    private static void EventPlaySoundPrefix()
    {
        ModEntry.EventCaptionManager.Value.BeforePlaySound();
    }

    private static void ChangeMusicPrefix(MusicContext music_context)
    {
        if (music_context != MusicContext.Event) return;
        ModEntry.EventCaptionManager.Value.BeforePlaySound();
    }

    private static void UpdateMusicPostfix()
    {
        if (Game1.currentSong != null)
        {
            ModEntry.ModCaptionManager.OnSoundPlayed(Game1.currentSong);
        }
    }

    private static void EventReplaceAllCommandsPostfix(Event __instance)
    {
        ModEntry.EventCaptionManager.Value.CleanupAfterEvent();
        ModEntry.EventCaptionManager.Value.PrepareForEvent(__instance);
    }
}