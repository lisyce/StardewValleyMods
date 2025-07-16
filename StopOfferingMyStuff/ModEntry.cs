using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StopOfferingMyStuff.ExternalApis;

namespace StopOfferingMyStuff;

public class ModEntry : Mod
{
    private ModConfig Config;
    
    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.Input.ButtonPressed += OnButtonPressed;
        Config = Helper.ReadConfig<ModConfig>();
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
}