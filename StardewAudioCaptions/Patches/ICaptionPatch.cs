using HarmonyLib;
using StardewModdingAPI;

namespace StardewAudioCaptions.Patches;

public interface ICaptionPatch
{
    public void Patch(Harmony harmony, IMonitor monitor);
}