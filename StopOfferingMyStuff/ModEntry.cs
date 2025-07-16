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
        if (e.Button != Config.ToggleKeybind) return;

        Config.ModEnabled = !Config.ModEnabled;
        Helper.WriteConfig(Config);
        
        if (Config.ModEnabled)
        {
            Game1.showGlobalMessage(Helper.Translation.Get("offering-off"));
        }
        else
        {
            Game1.showGlobalMessage(Helper.Translation.Get("offering-on"));
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
        var halt = AccessTools.Method(typeof(Character), nameof(Character.Halt));
        var enabledFld = AccessTools.Field(typeof(ModConfig), nameof(ModConfig.ModEnabled));
        var config = AccessTools.Field(typeof(ModEntry), nameof(Config));

        codeMatcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldstr, "Strings\\UI:GiftPlayerItem_"))
            .MatchStartBackwards(
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Callvirt, halt))
            .CreateLabel(out var lbl)
            .Insert(
                new CodeInstruction(OpCodes.Ldsfld, config),
                new CodeInstruction(OpCodes.Ldfld, enabledFld),
                new CodeMatch(OpCodes.Brfalse, lbl),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Ret));

        return codeMatcher.InstructionEnumeration();
    }
}