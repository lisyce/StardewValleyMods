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
        _subtitleHudMessage = new SubtitleHUDMessage(_config);
        _subtitleManager = new SubtitleManager(Helper, _subtitleHudMessage, Monitor, _config);

        AudioPatches.Patch(_harmony);
        var patchManager = new PatchManager(Monitor, _harmony);
        patchManager.Patch();
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
            save: () =>
            {
                Helper.WriteConfig(_config);
                _subtitleManager.Config = _config;
            }
            );
        
        configMenu.AddSectionTitle(
            mod: ModManifest, 
            text: () => Helper.Translation.Get("config.generalSectionTitle")
            );
        
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.subtitlesEnabled"),
            tooltip: () => Helper.Translation.Get("config.subtitlesEnabled.tooltip"),
            getValue: () => _config.SubtitlesOn,
            setValue: value =>
            {
                _config.SubtitlesOn = value;
                _subtitleHudMessage.SubtitlesOn = value;
            });
        
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
            name: () => Helper.Translation.Get("config.minDurationTicks"),
            tooltip: () => Helper.Translation.Get("config.minDurationTicks.tooltip"),
            getValue: () => _config.MinDurationTicks,
            setValue: value =>
            {
                _config.MinDurationTicks = value;
                SubtitleManager.MinDurationTicks = value;
            });

        configMenu.AddSectionTitle(
            mod: ModManifest,
            text: () => Helper.Translation.Get("config.toggleIndividualSectionTitle"));
        
        configMenu.AddParagraph(
            mod: ModManifest,
            text: () => Helper.Translation.Get("config.categories.paragraph"));
        
        var categories = _subtitleManager.SubtitlesByCategory();

        foreach (var category in categories)
        {
            configMenu.AddPageLink(
                mod: ModManifest,
                pageId: category.Key,
                text: () => Helper.Translation.Get(category.Key + ".category"));
        }
        
        foreach (var category in categories)
        {
            configMenu.AddPage(
                mod: ModManifest,
                pageId: category.Key,
                pageTitle: () => Helper.Translation.Get(category.Key + ".category"));

            foreach (var subtitleId in category.Value)
            {
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get(subtitleId + ".subtitle"),
                    getValue: () => _config.SubtitleToggles.GetValueOrDefault(subtitleId, true),
                    setValue: value => _config.SubtitleToggles[subtitleId] = value);
            }
        }
    }

    private void RegisterDefaultSubtitles()
    {
        _subtitleManager.RegisterDefaultSubtitle(new Subtitle("doorClose", "interaction.doorClose"));
        
        _subtitleManager.RegisterDefaultSubtitle(new Subtitle("eat", "player.eating"));
        _subtitleManager.RegisterDefaultSubtitle(new Subtitle("gulp", "player.drinking"));
        _subtitleManager.RegisterDefaultSubtitle(new Subtitle("ow", "player.hurts"));
        
        _subtitleManager.RegisterDefaultSubtitle(new Subtitle("throwDownITem", "environment.itemThrown"));
        _subtitleManager.RegisterDefaultSubtitle(new Subtitle("thunder", "environment.thunder"));
        _subtitleManager.RegisterDefaultSubtitle(new Subtitle("thunder_small", "environment.thunder"));
        _subtitleManager.RegisterDefaultSubtitle(new Subtitle("trainWhistle", "environment.trainWhistle"));
        _subtitleManager.RegisterDefaultSubtitle(new Subtitle("distantTrain", "environment.distantTrain"));
        _subtitleManager.RegisterDefaultSubtitle(new Subtitle("trainLoop", "environment.trainLoop", 80 * 60));
    }
}