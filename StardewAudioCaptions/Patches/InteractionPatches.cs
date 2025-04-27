using System.Reflection;
using HarmonyLib;
using StardewModdingAPI;
using StardewSubtitles.Subtitles;
using StardewValley;
using StardewValley.Objects;

namespace StardewSubtitles.Patches;

public class InteractionPatches : ISubtitlePatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Fence), nameof(Fence.toggleGate),
                new[] { typeof(bool), typeof(bool), typeof(Farmer) }),
            new Subtitle("doorClose", "interaction.fenceGate"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.CheckGarbage)),
            new Subtitle("trashcan", "interaction.trashCan"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(Chest), nameof(Chest.checkForAction)),
            new Subtitle("openChest", "interaction.chest"),
            new Subtitle("Ship", "interaction.giftbox"));

        if (TryGetChestDelegate(out var chestDelegate))
        {
            PatchGenerator.GeneratePatchPairs(
                harmony,
                monitor,
                chestDelegate!,
                new Subtitle("openChest", "interaction.chest"),
                new Subtitle("doorCreak", "interaction.fridge"));
        }
        else
        {
            monitor.Log("Failed to apply harmony patch on the Chest::checkForAction delegate; skipping these subtitles.", LogLevel.Warn);
        }
    }

    private bool TryGetChestDelegate(out MethodInfo? chestDelegate)
    {
        foreach (var method in typeof(Chest).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            if (method.GetParameters().Length == 0 &&
                method.Name.Contains("<checkForAction", StringComparison.Ordinal))
            {
                chestDelegate = method;
                return true;
            }
        }

        chestDelegate = null;
        return false;
    }
}