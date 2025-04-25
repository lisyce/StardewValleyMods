using HarmonyLib;
using StardewValley.Objects;

namespace StardewSubtitles.Patches;

public class ChestPatches : ISubtitlePatch
{
    public void Patch(Harmony harmony)
    {
        PatchGenerator.PrefixPostfixPatch(
            harmony,
            AccessTools.Method(typeof(Chest), nameof(Chest.checkForAction)),
            "openChest",
            "interaction.chest"
            );
    }
}