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
    public static EventCaptionManager EventCaptionManager;
    private CaptionHudMessage _captionHudMessage;
    private Harmony _harmony;
    
    public override void Entry(IModHelper helper)
    {
        Helper.Events.Display.RenderedStep += OnRenderedStep;
        Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        
        _config = Helper.ReadConfig<ModConfig>();
        _harmony = new Harmony(ModManifest.UniqueID);
        _captionHudMessage = new CaptionHudMessage();
        CaptionManager = new CaptionManager(Helper, _captionHudMessage, Monitor, _config);
        EventCaptionManager = new EventCaptionManager(helper, Monitor, CaptionManager);
        
        AudioPatches.Patch(_harmony);
        var patchManager = new PatchManager(Monitor, _harmony);
        patchManager.Patch();
    }

    private void OnRenderedStep(object? sender, RenderedStepEventArgs e)
    {
        if (Game1.game1.takingMapScreenshot || Game1.HostPaused || e.Step != RenderSteps.Overlays) return;
        _captionHudMessage.Draw(e.SpriteBatch, CaptionManager.Config);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        _captionHudMessage.Update();
        EventCaptionManager.Update();
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
                _captionHudMessage.ClearDisabledCaptions(_config);
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
            setValue: value => _config.CaptionsOn = value);
        
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.fontScaling"),
            tooltip: () => Helper.Translation.Get("config.fontScaling.tooltip"),
            getValue: () => _config.FontScaling,
            setValue: value => _config.FontScaling = value);
        
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.maxVisibleCaptions"),
            tooltip: () => Helper.Translation.Get("config.maxVisibleCaptions.tooltip"),
            getValue: () => _config.MaxVisibleCaptions,
            setValue: value =>  _config.MaxVisibleCaptions = value);
        
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.minDurationTicks"),
            tooltip: () => Helper.Translation.Get("config.minDurationTicks.tooltip"),
            getValue: () => _config.MinDurationTicks,
            setValue: value => _config.MinDurationTicks = value);
        
        configMenu.AddTextOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.captionPosition"),
            getValue: () => _config.CaptionPosition,
            setValue: value => _config.CaptionPosition = value,
            allowedValues: new []{ "Top Left", "Center Left", "Bottom Left", "Bottom Center", "Bottom Right", "Center Right" });
        
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.hideInMenu"),
            getValue: () => _config.HideInMenu,
            setValue: value => _config.HideInMenu = value);
        
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.offsetX"),
            tooltip: () => Helper.Translation.Get("config.offset.tooltip"),
            getValue: () => _config.CaptionOffsetX,
            setValue: value => _config.CaptionOffsetX = value);
        
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.offsetY"),
            tooltip: () => Helper.Translation.Get("config.offset.tooltip"),
            getValue: () => _config.CaptionOffsetY,
            setValue: value => _config.CaptionOffsetY = value);
        
        // auto-generated config for categories
        configMenu.AddSectionTitle(
            mod: ModManifest,
            text: () => Helper.Translation.Get("config.toggleIndividualSectionTitle"));
        
        configMenu.AddParagraph(
            mod: ModManifest,
            text: () => Helper.Translation.Get("config.categories.paragraph"));
        
        var categories = CaptionManager.CaptionsByCategory();

        var translatedCategories = categories.Select(x => new
            {
                Id = x.Key,
                Translation = Helper.Translation.Get(x.Key + ".category"),
                Captions = x.Value
            })
            .OrderBy(x => x.Translation.ToString());
        
        foreach (var category in translatedCategories)
        {
            configMenu.AddPageLink(
                mod: ModManifest,
                pageId: category.Id,
                text: () => category.Translation);
            
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => category.Translation + Helper.Translation.Get("config.categoryColor"),
                getValue: () => _config.CategoryColors.GetValueOrDefault(category.Id, "White"),
                setValue: value => _config.CategoryColors[category.Id] = value,
                allowedValues: CaptionManager.AllowedColors.Keys.ToArray());
        }
        
        foreach (var category in translatedCategories)
        {
            configMenu.AddPage(
                mod: ModManifest,
                pageId: category.Id,
                pageTitle: () => category.Translation);

            var translatedCaptions = category.Captions.Select(x => new
            {
                Id = x,
                Translation = Helper.Translation.Get(x + ".caption")
            })
            .OrderBy(x => x.Translation.ToString());
            
            foreach (var caption in translatedCaptions)
            {
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => caption.Translation,
                    getValue: () => _config.CaptionToggles.GetValueOrDefault(caption.Id, true),
                    setValue: value => _config.CaptionToggles[caption.Id] = value);
            }
        }
    }

    private void RegisterDefaultCaptions()
    {
        CaptionManager.RegisterDefaultCaption(new Caption("babblingBrook", "ambient.brook"));
        CaptionManager.RegisterDefaultCaption(new Caption("cracklingFire", "ambient.fireCrackle"));
        CaptionManager.RegisterDefaultCaption(new Caption("heavyEngine", "ambient.engine"));
        CaptionManager.RegisterDefaultCaption(new Caption("cricketsAmbient", "ambient.cricket"));
        CaptionManager.RegisterDefaultCaption(new Caption("waterfall", "ambient.waterfall"));
        CaptionManager.RegisterDefaultCaption(new Caption("waterfall_big", "ambient.waterfall"));
        CaptionManager.RegisterDefaultCaption(new Caption("moneyDial", "ambient.coinsClinking"));
        
        CaptionManager.RegisterDefaultCaption(new Caption("monkey1", "critters.monkey"));
        CaptionManager.RegisterDefaultCaption(new Caption("parrot_squawk", "critters.parrot"));
        
        CaptionManager.RegisterDefaultCaption(new Caption("doorClose", "interaction.doorClose"));
        CaptionManager.RegisterDefaultCaption(new Caption("stairsdown", "interaction.footstepsDescend"));
        CaptionManager.RegisterDefaultCaption(new Caption("harvest", "interaction.cropHarvest"));
        CaptionManager.RegisterDefaultCaption(new Caption("book_read", "interaction.bookRead"));

        CaptionManager.RegisterDefaultCaption(new Caption("eat", "player.eating"));
        CaptionManager.RegisterDefaultCaption(new Caption("gulp", "player.drinking"));
        CaptionManager.RegisterDefaultCaption(new Caption("ow", "player.hurts"));
        CaptionManager.RegisterDefaultCaption(new Caption("jingleBell", "player.footstepsJingle"));
        CaptionManager.RegisterDefaultCaption(new Caption("throwDownITem", "player.itemThrown"));
        CaptionManager.RegisterDefaultCaption(new Caption("questcomplete", "player.questComplete"));
        CaptionManager.RegisterDefaultCaption(new Caption("stardrop", "player.stardrop"));
        
        CaptionManager.RegisterDefaultCaption(new Caption("thunder", "ambient.thunder"));
        CaptionManager.RegisterDefaultCaption(new Caption("thunder_small", "ambient.thunder"));
        CaptionManager.RegisterDefaultCaption(new Caption("trainWhistle", "ambient.trainWhistle"));
        CaptionManager.RegisterDefaultCaption(new Caption("distantTrain", "ambient.distantTrain"));
        CaptionManager.RegisterDefaultCaption(new Caption("trainLoop", "ambient.trainLoop", 80 * 60));
        CaptionManager.RegisterDefaultCaption(new Caption("slosh", "world.waterSlosh"));
        CaptionManager.RegisterDefaultCaption(new Caption("rooster", "world.rooster"));
        CaptionManager.RegisterDefaultCaption(new Caption("leafrustle", "world.leafRustle"));
        CaptionManager.RegisterDefaultCaption(new Caption("cavedrip", "ambient.caveDrip"));
        CaptionManager.RegisterDefaultCaption(new Caption("bugLevelLoop", "ambient.bugLoop"));
        CaptionManager.RegisterDefaultCaption(new Caption("wind", "ambient.wind"));
        
        CaptionManager.RegisterDefaultCaption(new Caption("junimoMeep1", "critters.junimo"));
        CaptionManager.RegisterDefaultCaption(new Caption("seagulls", "critters.seagull"));
        CaptionManager.RegisterDefaultCaption(new Caption("SpringBirds", "critters.birdChirp"));
        CaptionManager.RegisterDefaultCaption(new Caption("camel", "critters.camel"));

        CaptionManager.RegisterDefaultCaption(new Caption("scissors", "tools.shears"));
        CaptionManager.RegisterDefaultCaption(new Caption("horse_flute", "tools.horseFlute"));

        CaptionManager.RegisterDefaultCaption(new Caption("batScreech", "monsters.batScreech"));
        CaptionManager.RegisterDefaultCaption(new Caption("squid_move", "monsters.blueSquidMove"));
        CaptionManager.RegisterDefaultCaption(new Caption("Duggy", "monsters.duggyDig"));
        CaptionManager.RegisterDefaultCaption(new Caption("flybuzzing", "monsters.flyBuzz"));
        CaptionManager.RegisterDefaultCaption(new Caption("slime", "monsters.slime"));
        
        CaptionManager.RegisterDefaultCaption(new Caption("slingshot", "weapons.slingshot"));
        
        CaptionManager.RegisterDefaultCaption(new Caption("parachute", "nightEvents.parachute"));
        CaptionManager.RegisterDefaultCaption(new Caption("planeflyby", "nightEvents.planefly"));
        
        CaptionManager.RegisterDefaultCaption(new Caption("cluck", "animals.chicken"));
        CaptionManager.RegisterDefaultCaption(new Caption("Duck", "animals.duck"));
        CaptionManager.RegisterDefaultCaption(new Caption("rabbit", "animals.rabbit"));
        CaptionManager.RegisterDefaultCaption(new Caption("cow", "animals.cow"));
        CaptionManager.RegisterDefaultCaption(new Caption("goat", "animals.goat"));
        CaptionManager.RegisterDefaultCaption(new Caption("sheep", "animals.sheep"));
        CaptionManager.RegisterDefaultCaption(new Caption("pig", "animals.pig"));
        CaptionManager.RegisterDefaultCaption(new Caption("Ostrich", "animals.ostrich"));
        
        CaptionManager.RegisterDefaultCaption(new Caption("cat", "pets.cat"));
        CaptionManager.RegisterDefaultCaption(new Caption("dog_bark", "pets.dogBark"));
        CaptionManager.RegisterDefaultCaption(new Caption("dog_pant", "pets.dogPant"));
        CaptionManager.RegisterDefaultCaption(new Caption("turtle_pet", "pets.turtle"));
    }
}