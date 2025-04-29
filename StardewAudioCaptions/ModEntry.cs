using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewAudioCaptions.APIs;
using StardewAudioCaptions.Patches;
using StardewAudioCaptions.Captions;
using StardewValley;
using StardewValley.Mods;

namespace StardewAudioCaptions;

public class ModEntry : Mod
{
    private ModConfig _config;
    public static CaptionManager CaptionManager;  // has to be public static so that harmony patches can use it
    private CaptionHudMessage _captionHudMessage;
    private Harmony _harmony;
    
    public override void Entry(IModHelper helper)
    {
        Helper.Events.Display.RenderedStep += OnRenderedStep;
        Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        
        _config = Helper.ReadConfig<ModConfig>();
        _harmony = new Harmony(ModManifest.UniqueID);
        _captionHudMessage = new CaptionHudMessage(_config);
        CaptionManager = new CaptionManager(Helper, _captionHudMessage, Monitor, _config);

        Harmony.DEBUG = true;
        AudioPatches.Patch(_harmony);
        var patchManager = new PatchManager(Monitor, _harmony);
        patchManager.Patch();
    }

    private void OnRenderedStep(object? sender, RenderedStepEventArgs e)
    {
        if (Game1.game1.takingMapScreenshot || Game1.HostPaused || e.Step != RenderSteps.Overlays) return;
        _captionHudMessage.Draw(e.SpriteBatch);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        _captionHudMessage.Update();
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        SetupGmcmIntegration();
        RegisterDefaultCaptions();
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
                CaptionManager.Config = _config;
            }
            );
        
        configMenu.AddSectionTitle(
            mod: ModManifest, 
            text: () => Helper.Translation.Get("config.generalSectionTitle")
            );
        
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.captionsEnabled"),
            tooltip: () => Helper.Translation.Get("config.captionsEnabled.tooltip"),
            getValue: () => _config.CaptionsOn,
            setValue: value =>
            {
                _config.CaptionsOn = value;
                _captionHudMessage.CaptionsOn = value;
            });
        
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.fontScaling"),
            tooltip: () => Helper.Translation.Get("config.fontScaling.tooltip"),
            getValue: () => _config.FontScaling,
            setValue: value =>
            {
                _config.FontScaling = value;
                _captionHudMessage.FontScaling = value;
            });
        
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.maxVisibleCaptions"),
            tooltip: () => Helper.Translation.Get("config.maxVisibleCaptions.tooltip"),
            getValue: () => _config.MaxVisibleCaptions,
            setValue: value =>
            {
                _config.MaxVisibleCaptions = value;
                _captionHudMessage.MaxVisible = value;
            });
        
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.minDurationTicks"),
            tooltip: () => Helper.Translation.Get("config.minDurationTicks.tooltip"),
            getValue: () => _config.MinDurationTicks,
            setValue: value =>
            {
                _config.MinDurationTicks = value;
                CaptionManager.MinDurationTicks = value;
            });

        configMenu.AddSectionTitle(
            mod: ModManifest,
            text: () => Helper.Translation.Get("config.toggleIndividualSectionTitle"));
        
        configMenu.AddParagraph(
            mod: ModManifest,
            text: () => Helper.Translation.Get("config.categories.paragraph"));
        
        var categories = CaptionManager.CaptionsByCategory();

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

            foreach (var captionId in category.Value)
            {
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get(captionId + ".caption"),
                    getValue: () => _config.CaptionToggles.GetValueOrDefault(captionId, true),
                    setValue: value => _config.CaptionToggles[captionId] = value);
            }
        }
    }

    private void RegisterDefaultCaptions()
    {
        CaptionManager.RegisterDefaultCaption(new Caption("doorClose", "interaction.doorClose"));
        
        CaptionManager.RegisterDefaultCaption(new Caption("eat", "player.eating"));
        CaptionManager.RegisterDefaultCaption(new Caption("gulp", "player.drinking"));
        CaptionManager.RegisterDefaultCaption(new Caption("ow", "player.hurts"));
        CaptionManager.RegisterDefaultCaption(new Caption("jingleBell", "player.footstepsJingle"));
        
        CaptionManager.RegisterDefaultCaption(new Caption("throwDownITem", "player.itemThrown"));
        CaptionManager.RegisterDefaultCaption(new Caption("thunder", "ambient.thunder"));
        CaptionManager.RegisterDefaultCaption(new Caption("thunder_small", "ambient.thunder"));
        CaptionManager.RegisterDefaultCaption(new Caption("trainWhistle", "ambient.trainWhistle"));
        CaptionManager.RegisterDefaultCaption(new Caption("distantTrain", "ambient.distantTrain"));
        CaptionManager.RegisterDefaultCaption(new Caption("trainLoop", "ambient.trainLoop", 80 * 60));
        CaptionManager.RegisterDefaultCaption(new Caption("slosh", "environment.waterSlosh"));
        CaptionManager.RegisterDefaultCaption(new Caption("seagulls", "ambient.seagull"));
        CaptionManager.RegisterDefaultCaption(new Caption("SpringBirds", "ambient.birdChirp"));
        CaptionManager.RegisterDefaultCaption(new Caption("rooster", "environment.rooster"));
        CaptionManager.RegisterDefaultCaption(new Caption("leafrustle", "ambient.leafRustle"));
        CaptionManager.RegisterDefaultCaption(new Caption("cavedrip", "ambient.caveDrip"));
        CaptionManager.RegisterDefaultCaption(new Caption("bugLevelLoop", "ambient.bugLoop"));
        
        CaptionManager.RegisterDefaultCaption(new Caption("scissors", "tools.shears"));
        
        CaptionManager.RegisterDefaultCaption(new Caption("batScreech", "monsters.batScreech"));
    }
}