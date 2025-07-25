﻿using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace VolcanoButtonTracker;

public class ModEntry : Mod
{
    private static IModHelper _staticHelper = null!;
    private static IMonitor _staticMonitor = null!;
    
    public override void Entry(IModHelper helper)
    {
        _staticHelper = helper;
        _staticMonitor = Monitor;

        var harmony = new Harmony(ModManifest.UniqueID);
        
        harmony.Patch(
            original: AccessTools.Method(typeof(DwarfGate), nameof(DwarfGate.OnPress)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(OnPressPostfix))
        );
        
        harmony.Patch(
            original: AccessTools.Method(typeof(DwarfGate), nameof(DwarfGate.OpenGate)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(OnOpenPostfix))
        );

        Helper.Events.Player.Warped += OnWarped;
    }

    private static void OnWarped(object? sender, WarpedEventArgs e)
    {
        if (!e.IsLocalPlayer || e.OldLocation.Name == e.NewLocation.Name || e.NewLocation is not VolcanoDungeon dungeon ||
            !VolcanoDungeon.IsGeneratedLevel(dungeon.Name, out var level) ||
            level is 0 or 5) return;

        var leftToPress = GetRemainingSwitches(dungeon);
        if (leftToPress <= 0)
        {
            Game1.showGlobalMessage(_staticHelper.Translation.Get("NoButtons"));
        }
        else
        {
            DisplayButtonsLeft(leftToPress);
        }
    }

    private static void OnPressPostfix(DwarfGate __instance)
    {
        if (!Game1.IsMasterGame || !Game1.player.currentLocation.Equals(__instance.locationRef.Value)) return;
        if (Game1.player.currentLocation is not VolcanoDungeon dungeon) return;
        
        var leftToPress = GetRemainingSwitches(dungeon);
        DisplayButtonsLeft(leftToPress);
    }

    private static void OnOpenPostfix(DwarfGate __instance)
    {
        if (!Game1.player.currentLocation.Equals(__instance.locationRef.Value)) return;
        if (Game1.player.currentLocation is not VolcanoDungeon dungeon) return;
        
        var leftToPress = GetRemainingSwitches(dungeon);
        if (leftToPress == 0)
        {
            Game1.showGlobalMessage(_staticHelper.Translation.Get("GateOpen"));
        }
    }

    private static void DisplayButtonsLeft(int leftToPress)
    {
        if (leftToPress == 1)
        {
            Game1.showGlobalMessage(_staticHelper.Translation.Get("OneButton"));
        }
        else if (leftToPress > 0)
        {
            Game1.showGlobalMessage(_staticHelper.Translation.Get("NButtons", new { num = leftToPress }));
        }
    }
    
    private static int GetRemainingSwitches(VolcanoDungeon dungeon)
    {
        var totalSwitches = 0;
        var pressedSwitches = 0;
        foreach (var gate in dungeon.dwarfGates)
        {
            foreach (var pressed in gate.switches.Values)
            {
                totalSwitches++;
                if (pressed) pressedSwitches++;
            }
        }

        var leftToPress = totalSwitches - pressedSwitches;
        return leftToPress;
    }
}