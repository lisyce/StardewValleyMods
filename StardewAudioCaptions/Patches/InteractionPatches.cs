using System.Reflection;
using HarmonyLib;
using StardewModdingAPI;
using StardewAudioCaptions.Captions;
using StardewValley;
using StardewValley.Objects;

namespace StardewAudioCaptions.Patches;

public class InteractionPatches : ICaptionPatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Fence), nameof(Fence.toggleGate),
                new[] { typeof(bool), typeof(bool), typeof(Farmer) }),
            new Caption("doorClose", "interaction.fenceGate"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.CheckGarbage)),
            new Caption("trashcan", "interaction.trashCan"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(Chest), nameof(Chest.checkForAction)),
            new Caption("openChest", "interaction.chest"),
            new Caption("Ship", "interaction.giftbox"));

        if (TryGetChestDelegate(out var chestDelegate))
        {
            PatchGenerator.GeneratePatchPairs(
                harmony,
                monitor,
                chestDelegate!,
                new Caption("openChest", "interaction.chest"),
                new Caption("doorCreak", "interaction.fridge"));
        }
        else
        {
            monitor.Log("Failed to apply harmony patch on the Chest::checkForAction delegate; skipping these captions.", LogLevel.Warn);
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