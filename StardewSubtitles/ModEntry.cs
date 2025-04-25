using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewSubtitles.APIs;
using StardewSubtitles.Patches;
using StardewSubtitles.Subtitles;
using StardewValley;

namespace StardewSubtitles;

public class ModEntry : Mod
{
    private ModConfig _config;
    public static SubtitleManager _subtitleManager;  // has to be public static so that harmony patches can use it
    private SubtitleHUDMessage _subtitleHudMessage;
    private Harmony _harmony;
    
    public override void Entry(IModHelper helper)
    {
        Helper.Events.Display.RenderedHud += OnRenderedHud;
        Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        
        _config = Helper.ReadConfig<ModConfig>();
        _harmony = new Harmony(ModManifest.UniqueID);

        SoundsHelperPatches.Patch(_harmony);
        FencePatches.Patch(_harmony);
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (Game1.game1.takingMapScreenshot || Game1.HostPaused) return;
        _subtitleHudMessage.Draw(e.SpriteBatch);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        _subtitleHudMessage.Update();
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        SetupGmcmIntegration();
        
        _subtitleHudMessage = new SubtitleHUDMessage(_config.FontScaling, _config.MaxVisibleSubtitles,
            _config.DefaultDurationTicks);
        _subtitleManager = new SubtitleManager(Helper, _subtitleHudMessage, Monitor);
        RegisterDefaultSubtitles();
    }

    private void SetupGmcmIntegration()
    {
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu is null)
            return;
        
        configMenu.Register(
            mod: ModManifest,
            reset: () => _config = new ModConfig(),
            save: () => Helper.WriteConfig(_config)
            );
        
        configMenu.AddSectionTitle(
            mod: ModManifest, 
            text: () => Helper.Translation.Get("config.generalSectionTitle")
            );
        
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.fontScaling"),
            tooltip: () => Helper.Translation.Get("config.fontScaling.tooltip"),
            getValue: () => _config.FontScaling,
            setValue: value =>
            {
                _config.FontScaling = value;
                _subtitleHudMessage.FontScaling = value;
            });
        
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.maxVisibleSubtitles"),
            tooltip: () => Helper.Translation.Get("config.maxVisibleSubtitles.tooltip"),
            getValue: () => _config.MaxVisibleSubtitles,
            setValue: value =>
            {
                _config.MaxVisibleSubtitles = value;
                _subtitleHudMessage.MaxVisible = value;
            });
        
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.defaultDurationTicks"),
            tooltip: () => Helper.Translation.Get("config.defaultDurationTicks.tooltip"),
            getValue: () => _config.DefaultDurationTicks,
            setValue: value =>
            {
                _config.DefaultDurationTicks = value;
                _subtitleHudMessage.DefaultDurationTicks = value;
            });
    }

    private void RegisterDefaultSubtitles()
    {
        _subtitleManager.RegisterDefaultSubtitle("doorClose", "environment.doorClose");
    }
}