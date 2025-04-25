using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewSubtitles.APIs;
using StardewValley;

namespace StardewSubtitles;

public class ModEntry : Mod
{
    public static ModConfig Config;
    
    public override void Entry(IModHelper helper)
    {
        Helper.Events.Display.RenderedHud += OnRenderedHud;
        Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        
        Config = Helper.ReadConfig<ModConfig>();
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (Game1.game1.takingMapScreenshot || Game1.HostPaused) return;
        SubtitleHUDMessage.Instance.Draw(e.SpriteBatch);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        SubtitleHUDMessage.Instance.Update();
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        SetupGmcmIntegration();
    }

    private void SetupGmcmIntegration()
    {
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu is null)
            return;
        
        configMenu.Register(
            mod: ModManifest,
            reset: () => Config = new ModConfig(),
            save: () => Helper.WriteConfig(Config)
            );
        
        configMenu.AddSectionTitle(
            mod: ModManifest, 
            text: () => Helper.Translation.Get("config.generalSectionTitle")
            );
        
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.fontScaling"),
            tooltip: () => Helper.Translation.Get("config.fontScaling.tooltip"),
            getValue: () => Config.FontScaling,
            setValue: value => Config.FontScaling = value);
        
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.maxVisibleSubtitles"),
            tooltip: () => Helper.Translation.Get("config.maxVisibleSubtitles.tooltip"),
            getValue: () => Config.MaxVisibleSubtitles,
            setValue: value => Config.MaxVisibleSubtitles = value);
        
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.defaultDurationTicks"),
            tooltip: () => Helper.Translation.Get("config.defaultDurationTicks.tooltip"),
            getValue: () => Config.DefaultDurationTicks,
            setValue: value => Config.DefaultDurationTicks = value);
    }
}