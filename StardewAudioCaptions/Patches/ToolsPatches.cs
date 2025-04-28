using HarmonyLib;
using StardewAudioCaptions.Captions;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace StardewAudioCaptions.Patches;

public class ToolsPatches : ICaptionPatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(MilkPail), nameof(MilkPail.beginUsing)),
            new Caption("Milking", "tools.pail"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.performToolAction)),
            new Caption("axchop", "tools.axe"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(Bush), nameof(Bush.performToolAction)),
            new Caption("axchop", "tools.axe"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(FruitTree), nameof(FruitTree.performToolAction)),
            new Caption("axchop", "tools.axe"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(GiantCrop), nameof(GiantCrop.performToolAction)),
            new Caption("axchop", "tools.axe"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(Tree), nameof(Tree.performToolAction)),
            new Caption("axchop", "tools.axe"));
    }
}