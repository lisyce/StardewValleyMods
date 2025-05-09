using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewAudioCaptions.Captions;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.FloorsAndPaths;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

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
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Chest), nameof(Chest.updateWhenCurrentLocation)),
            new Caption("doorCreakReverse", "interaction.chestClose", shouldLog: false));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(Chest), nameof(Chest.UpdateFarmerNearby)),
            new Caption("doorCreak", "interaction.chest", shouldLog: false),
            new Caption("doorCreakReverse", "interaction.chestClose", shouldLog: false));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(ShippingBin), "openShippingBinLid"),
            new Caption("doorCreak", "interaction.shippingBinOpen", shouldLog: false));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(ShippingBin), "closeShippingBinLid"),
            new Caption("doorCreakReverse", "interaction.shippingBinClose", shouldLog: false));
        
        PatchGenerator.GeneratePrefix(
            harmony,
            monitor,
            AccessTools.Method(typeof(ShippingBin), "showShipment"),
            new Caption("Ship", "interaction.itemShipped"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Furniture), nameof(Furniture.setFireplace)),
            new Caption("fireball", "interaction.fire"));
        
        PatchGenerator.GeneratePrefixes(
            harmony,
            monitor,
            AccessTools.Method(typeof(MineShaft), nameof(MineShaft.enterMineShaft)),
            new Caption("fallDown", "interaction.shaftFalling"),
            new Caption("clubSmash", "interaction.shaftLanding"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.openDoor)),
            new Caption("doorOpen", "interaction.doorOpen"),
            new Caption("doorCreak", "interaction.doorOpen"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(StardewValley.Object), "CheckForActionOnSingingStone"),
            new Caption("crystal", "interaction.singingStone"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(Building), nameof(Building.ToggleAnimalDoor)),
            new Caption("doorCreak", "interaction.doorClose"),
            new Caption("doorCreakReverse", "interaction.doorOpen"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(StardewValley.Object), "CheckForActionOnFeedHopper"),
            new Caption("shwip", "interaction.hayHopper"));
        
        PatchGenerator.TranspilerPatch(
            harmony,
            monitor,
            AccessTools.Method(typeof(BreakableContainer), nameof(BreakableContainer.performToolAction)),
            BreakableContainerPerformToolActionTranspiler);
        
        PatchGenerator.TranspilerPatch(
            harmony,
            monitor,
            AccessTools.Method(typeof(MineShaft), nameof(MineShaft.checkAction)),
            MineshaftCheckActionTranspiler);
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction)),
            new Caption("woodyStep", "interaction.itemPlaced"),
            new Caption("dirtyHit", "interaction.planted"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Furniture), nameof(Furniture.performObjectDropInAction)),
            new Caption("woodyStep", "interaction.itemPlaced"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Torch), nameof(Torch.checkForAction)),
            new Caption("fireball", "interaction.fire"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.TryApplyFairyDust)),
            new Caption("yoba", "interaction.fairyDust"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Flooring), nameof(Flooring.performToolAction)),
            new Caption(CaptionManager.AnyCue, "interaction.itemBreak"));
        
        PatchGenerator.TranspilerPatch(
            harmony,
            monitor,
            AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction)),
            ObjectPlacementActionTranspiler);
        
        // flute block stuff
        PatchGenerator.TranspilerPatch(
            harmony,
            monitor,
            AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.farmerAdjacentAction)),
            FluteBlockTranspiler);
        
        PatchGenerator.TranspilerPatch(
            harmony,
            monitor,
            AccessTools.Method(typeof(StardewValley.Object), "CheckForActionOnFluteBlock"),
            FluteBlockTranspiler);
        
        PatchGenerator.FinalizerPatch(
            harmony,
            monitor,
            AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.farmerAdjacentAction)),
            FluteBlockFinalizer);
        
        PatchGenerator.FinalizerPatch(
            harmony,
            monitor,
            AccessTools.Method(typeof(StardewValley.Object), "CheckForActionOnFluteBlock"),
            FluteBlockFinalizer);
        
        // drum block stuff
        PatchGenerator.TranspilerPatch(
            harmony,
            monitor,
            AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.farmerAdjacentAction)),
            DrumBlockTranspiler);
        
        PatchGenerator.TranspilerPatch(
            harmony,
            monitor,
            AccessTools.Method(typeof(StardewValley.Object), "CheckForActionOnDrumBlock"),
            DrumBlockTranspiler);
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

    private static IEnumerable<CodeInstruction> BreakableContainerPerformToolActionTranspiler(
        IEnumerable<CodeInstruction> instructions)
    {
        var playSound =
            AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.playNearbySoundAll));
        
        // break sound
        var matcher = new CodeMatcher(instructions);
        matcher.MatchStartForward(
                new CodeMatch(OpCodes.Call, playSound))
            .ThrowIfNotMatch("Couldn't find call to method playNearbySoundAll");

        var soundCueMatcher = new SoundCueCodeMatcher(matcher);
        soundCueMatcher.RegisterCaptionForNextCue(CaptionManager.AnyCue, "interaction.containerBreak");
        
        // hit sound
        matcher.MatchStartForward(
                new CodeMatch(OpCodes.Call, playSound))
            .ThrowIfNotMatch("Couldn't find call to method playNearbySoundAll");
        soundCueMatcher.RegisterCaptionForNextCue(CaptionManager.AnyCue, "interaction.containerCrack");

        return soundCueMatcher.InstructionEnumeration();
    }

    private static IEnumerable<CodeInstruction> ObjectPlacementActionTranspiler(
        IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);
        var placementSoundFld = AccessTools.Field(typeof(FloorPathData), nameof(FloorPathData.PlacementSound));
        var isFloorPath =
            AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.IsFloorPathItem));

        matcher.MatchStartForward(new CodeMatch(OpCodes.Callvirt, isFloorPath))
            .MatchStartForward(
                new CodeMatch(OpCodes.Ldfld, placementSoundFld),
                new CodeMatch(OpCodes.Brfalse_S),
                new CodeMatch(OpCodes.Callvirt, SoundCueCodeMatcher.GameLocationPlaySound))
            .ThrowIfNotMatch("Could not find flooring placement sound");
        
        var soundCueMatcher = new SoundCueCodeMatcher(matcher);
        soundCueMatcher.RegisterCaptionForNextCue(CaptionManager.AnyCue, "interaction.itemPlaced");
        return soundCueMatcher.InstructionEnumeration();
    }

    private static IEnumerable<CodeInstruction> MineshaftCheckActionTranspiler(
        IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new SoundCueCodeMatcher(instructions);
        matcher.FindCue("openBox", SoundCueCodeMatcher.GameLocationPlaySound)
            .RegisterCaptionForNextCue("openBox", "interaction.minecart", CaptionManager.InfiniteDuration);
        return matcher.InstructionEnumeration();
    }

    private static IEnumerable<CodeInstruction> FluteBlockTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);
        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "flute"))
            .ThrowIfNotMatch("could not find where flute sound is loaded")
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(InteractionPatches), nameof(FluteBlockHelper)))
            );
        return matcher.InstructionEnumeration();
    }

    private static void FluteBlockFinalizer()
    {
        var pairs = new[]
        {
            ("flute", "interaction.fluteBlock"),
            ("clam_tone", "interaction.fluteBlockClam"),
            ("telephone_buttonPush", "interaction.fluteBlockTelephone"),
            ("miniharp_note", "interaction.fluteBlockHarp"),
            ("pig", "interaction.fluteBlockPig"),
            ("crystal", "interaction.fluteBlockCrystal"),
            ("Duck", "interaction.fluteBlockDuck"),
            ("toyPiano", "interaction.fluteBlockPiano"),
            ("dustMeep", "interaction.fluteBlockMeep")
        };
        
        foreach (var p in pairs)
        {
            ModEntry.CaptionManager.UnregisterCaptionForNextCue(new Caption(p.Item1, p.Item2));
        }

    }

    private static void FluteBlockHelper(StardewValley.Object obj)
    {
        if (!int.TryParse(obj.preservedParentSheetIndex.Value, out var preservedParentSheetInt))
            preservedParentSheetInt = 0;

        preservedParentSheetInt /= 100;

        var tokens = new { pitch = preservedParentSheetInt.ToString() };

        var pairs = new[]
        {
            ("flute", "interaction.fluteBlock"),
            ("clam_tone", "interaction.fluteBlockClam"),
            ("telephone_buttonPush", "interaction.fluteBlockTelephone"),
            ("miniharp_note", "interaction.fluteBlockHarp"),
            ("pig", "interaction.fluteBlockPig"),
            ("crystal", "interaction.fluteBlockCrystal"),
            ("Duck", "interaction.fluteBlockDuck"),
            ("toyPiano", "interaction.fluteBlockPiano"),
            ("dustMeep", "interaction.fluteBlockMeep")
        };
        
        foreach (var p in pairs)
        {
            ModEntry.CaptionManager.RegisterCaptionForNextCue(new Caption(p.Item1, p.Item2, tokens: tokens));
        }
    }

    private static IEnumerable<CodeInstruction> DrumBlockTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);
        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "drumkit"))
            .ThrowIfNotMatch("could not find where drumkit sound is loaded")
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(InteractionPatches), nameof(DrumBlockHelper)))
            );
        return matcher.InstructionEnumeration();
    }
    
    private static void DrumBlockHelper(StardewValley.Object obj)
    {
        if (!int.TryParse(obj.preservedParentSheetIndex.Value, out var preservedParentSheetInt))
            preservedParentSheetInt = 0;
        
        var tokens = new { pitch = preservedParentSheetInt.ToString() };
        ModEntry.CaptionManager.RegisterCaptionForNextCue(new Caption(CaptionManager.AnyCue, "interaction.drumBlock", tokens: tokens));
    }
}