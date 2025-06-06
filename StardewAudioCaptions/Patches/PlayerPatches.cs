using System.Numerics;
using HarmonyLib;
using StardewModdingAPI;
using StardewAudioCaptions.Captions;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;
using xTile.Dimensions;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace StardewAudioCaptions.Patches;

public class PlayerPatches : ICaptionPatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Debris), nameof(Debris.updateChunks)),
            new Caption("coin", "player.itemCollect"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(FarmerSprite), "checkForFootstep"),
            new Caption("thudStep", "player.footsteps"),
            new Caption("thudStep", "player.footsteps"),
            new Caption("sandyStep", "player.footsteps"),
            new Caption("stoneStep", "player.footsteps"),
            new Caption("snowyStep", "player.footsteps"),
            new Caption("grassyStep", "player.footsteps"),
            new Caption("woodyStep", "player.footsteps"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
            new Caption("pickUpItem", "player.itemCollect"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Furniture), nameof(Furniture.clicked)),
            new Caption("coin", "player.itemCollect"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(GameLocation), "removeQueuedFurniture"),
            new Caption("coin", "player.itemCollect"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(StardewValley.Object), "CheckForActionOnMachine"),
            new Caption("coin", "player.itemCollect"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(StardewValley.Object), "totemWarp"),
            new Caption("wand", "player.teleport"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Wand), nameof(Wand.DoFunction)),
            new Caption("wand", "player.teleport"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Building), nameof(Building.PerformObeliskWarp)),
            new Caption("wand", "player.teleport"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTouchAction), new []{typeof(string[]), typeof(Vector2)}),
            new Caption("wand", "player.teleport"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(IslandWest), nameof(IslandWest.performAction), new []{typeof(string[]), typeof(Farmer), typeof(Location)}),
            new Caption("wand", "player.teleport"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(StardewValley.Object), "CheckForActionOnMiniObelisk"),
            new Caption("wand", "player.teleport"));
    }
}