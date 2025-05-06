using System.Collections;
using HarmonyLib;
using StardewAudioCaptions.Captions;
using StardewModdingAPI;
using StardewValley;
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
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.performToolAction)),
            new Caption("axchop", "tools.axe"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Bush), nameof(Bush.performToolAction)),
            new Caption("axchop", "tools.axe"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(FruitTree), nameof(FruitTree.performToolAction)),
            new Caption("axchop", "tools.axe"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(GiantCrop), nameof(GiantCrop.performToolAction)),
            new Caption("axchop", "tools.axe"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Tree), nameof(Tree.performToolAction)),
            new Caption("axchop", "tools.axe"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Pickaxe), nameof(Pickaxe.DoFunction)),
            new Caption("hammer", "tools.pickaxe"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Pan), nameof(Pan.playSlosh)),
            new Caption("slosh", "tools.pan"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Tool), nameof(Tool.endUsing)),
            new Caption("wateringCan", "tools.wateringCan", shouldLog: false));
    }
}