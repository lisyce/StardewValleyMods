using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StopOfferingMyStuff.ExternalApis;

namespace StopOfferingMyStuff;

public class ModEntry : Mod
{
    private static ModConfig Config = null!;  // set in Entry
    
    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.Input.ButtonPressed += OnButtonPressed;
        Config = Helper.ReadConfig<ModConfig>();

        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.Method(typeof(Farmer), nameof(Farmer.checkAction)),
            transpiler: new HarmonyMethod(typeof(ModEntry), nameof(FarmerCheckActionTranspiler))
            );
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!e.Button.Equals(Config.ToggleKeybind)) return;

        Config.ModEnabled = !Config.ModEnabled;
        Helper.WriteConfig(Config);
        
        if (Config.ModEnabled)
        {
            Game1.showGlobalMessage(Helper.Translation.Get("offering-on"));
        }
        else
        {
            Game1.showGlobalMessage(Helper.Translation.Get("offering-off"));
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var gmcmApi = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        
        gmcmApi?.Register(
            mod: ModManifest,
            reset: () => Config = new ModConfig(),
            save: () => Helper.WriteConfig(Config)
            );
        
        gmcmApi?.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.enabled"),
            getValue: () => Config.ModEnabled,
            setValue: value => Config.ModEnabled = value);
        
        gmcmApi?.AddKeybind(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.toggle"),
            tooltip: () => Helper.Translation.Get("config.toggle.tooltip"),
            getValue: () => Config.ToggleKeybind,
            setValue: value => Config.ToggleKeybind = value);
    }

    private static IEnumerable<CodeInstruction> FarmerCheckActionTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var codeMatcher = new CodeMatcher(instructions, generator);
        var createDialogue = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.createQuestionDialogue),
            new[] { typeof(string), typeof(Response[]), typeof(GameLocation.afterQuestionBehavior), typeof(NPC) });

        codeMatcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldstr, "Strings\\UI:GiftPlayerItem_"))
            .MatchStartForward(
                new CodeMatch(OpCodes.Callvirt, createDialogue))
            .SetOpcodeAndAdvance(OpCodes.Nop)
            .Insert(
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Pop));

        return codeMatcher.InstructionEnumeration();
    }
}