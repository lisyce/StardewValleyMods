using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewAudioCaptions.APIs;
using StardewAudioCaptions.Patches;
using StardewAudioCaptions.Captions;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Mods;

namespace StardewAudioCaptions;

public class ModEntry : Mod
{
    private ModConfig _config;
    public static CaptionManager ModCaptionManager;  // has to be public static so that harmony patches can use it
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
        ModCaptionManager = new CaptionManager(Helper, _captionHudMessage, Monitor, _config);
        EventCaptionManager = new EventCaptionManager(helper, Monitor, ModCaptionManager);
        AudioPatches.Patch(_harmony);
        var patchManager = new PatchManager(Monitor, _harmony);
        patchManager.Patch();
    }

    private void OnRenderedStep(object? sender, RenderedStepEventArgs e)
    {
        if (Game1.game1.takingMapScreenshot || Game1.HostPaused || e.Step != RenderSteps.Overlays) return;
        _captionHudMessage.Draw(e.SpriteBatch, _config);
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
                ModCaptionManager.Config = _config;
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
        
        var categories = ModCaptionManager.CaptionsByCategory();
        categories.Remove("events");

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
        ModCaptionManager.RegisterDefaultCaption(new Caption("babblingBrook", "ambient.brook"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("cracklingFire", "ambient.fireCrackle"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("heavyEngine", "ambient.engine"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("cricketsAmbient", "ambient.cricket"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("waterfall", "ambient.waterfall"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("waterfall_big", "ambient.waterfall"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("moneyDial", "ambient.coinsClinking"));
        
        ModCaptionManager.RegisterDefaultCaption(new Caption("monkey1", "critters.monkey"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("parrot_squawk", "critters.parrot"));
        
        ModCaptionManager.RegisterDefaultCaption(new Caption("doorClose", "interaction.doorClose"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("stairsdown", "interaction.footstepsDescend"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("harvest", "interaction.cropHarvest"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("book_read", "interaction.bookRead"));

        ModCaptionManager.RegisterDefaultCaption(new Caption("eat", "player.eating"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("gulp", "player.drinking"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("ow", "player.hurts"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("jingleBell", "player.footstepsJingle"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("throwDownITem", "player.itemThrown"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("questcomplete", "player.questComplete"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("stardrop", "player.stardrop"));
        
        ModCaptionManager.RegisterDefaultCaption(new Caption("thunder", "ambient.thunder"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("thunder_small", "ambient.thunder"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("trainWhistle", "ambient.trainWhistle"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("distantTrain", "ambient.distantTrain"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("trainLoop", "ambient.trainLoop", 80 * 60));
        ModCaptionManager.RegisterDefaultCaption(new Caption("slosh", "world.waterSlosh"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("rooster", "world.rooster"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("leafrustle", "world.leafRustle"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("cavedrip", "ambient.caveDrip"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("bugLevelLoop", "ambient.bugLoop"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("wind", "ambient.wind"));
        
        ModCaptionManager.RegisterDefaultCaption(new Caption("junimoMeep1", "critters.junimo"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("seagulls", "critters.seagull"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("SpringBirds", "critters.birdChirp"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("camel", "critters.camel"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("Raccoon", "critters.raccoon"));

        ModCaptionManager.RegisterDefaultCaption(new Caption("scissors", "tools.shears"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("horse_flute", "tools.horseFlute"));

        ModCaptionManager.RegisterDefaultCaption(new Caption("batScreech", "monsters.batScreech"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("squid_move", "monsters.blueSquidMove"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("Duggy", "monsters.duggyDig"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("flybuzzing", "monsters.flyBuzz"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("slime", "monsters.slime"));
        
        ModCaptionManager.RegisterDefaultCaption(new Caption("slingshot", "weapons.slingshot"));
        
        ModCaptionManager.RegisterDefaultCaption(new Caption("parachute", "nightEvents.parachute"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("planeflyby", "nightEvents.planefly"));
        
        ModCaptionManager.RegisterDefaultCaption(new Caption("cluck", "animals.chicken"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("Duck", "animals.duck"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("rabbit", "animals.rabbit"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("cow", "animals.cow"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("goat", "animals.goat"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("sheep", "animals.sheep"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("pig", "animals.pig"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("Ostrich", "animals.ostrich"));
        
        ModCaptionManager.RegisterDefaultCaption(new Caption("cat", "pets.cat"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("dog_bark", "pets.dogBark"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("dog_pant", "pets.dogPant"));
        ModCaptionManager.RegisterDefaultCaption(new Caption("turtle_pet", "pets.turtle"));
    }
}