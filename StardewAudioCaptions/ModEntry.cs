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
    public static PerScreen<CaptionManager> ModCaptionManager;  // has to be public static so that harmony patches can use it
    public static PerScreen<EventCaptionManager> EventCaptionManager;
    private PerScreen<CaptionHudMessage> _captionHudMessage;
    private Harmony _harmony;
    
    public override void Entry(IModHelper helper)
    {
        Helper.Events.Display.RenderedStep += OnRenderedStep;
        Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        
        _config = Helper.ReadConfig<ModConfig>();
        _harmony = new Harmony(ModManifest.UniqueID);
        _captionHudMessage = new PerScreen<CaptionHudMessage>(createNewState: () => new CaptionHudMessage());
        ModCaptionManager = new PerScreen<CaptionManager>(createNewState: () => new CaptionManager(Helper, _captionHudMessage.Value, Monitor, _config));
        EventCaptionManager = new PerScreen<EventCaptionManager>(createNewState: () =>
            new EventCaptionManager(helper, Monitor, ModCaptionManager.Value));
        AudioPatches.Patch(_harmony);
        var patchManager = new PatchManager(Monitor, _harmony);
        patchManager.Patch();
    }

    private void OnRenderedStep(object? sender, RenderedStepEventArgs e)
    {
        if (Game1.game1.takingMapScreenshot || Game1.HostPaused || e.Step != RenderSteps.Overlays) return;
        _captionHudMessage.Value.Draw(e.SpriteBatch, _config);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        _captionHudMessage.Value.Update();
        EventCaptionManager.Value.Update();
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
                ModCaptionManager.Value.Config = _config;
                _captionHudMessage.Value.ClearDisabledCaptions(_config);
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
        
        var categories = ModCaptionManager.Value.CaptionsByCategory();

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
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("babblingBrook", "ambient.brook"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("cracklingFire", "ambient.fireCrackle"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("heavyEngine", "ambient.engine"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("cricketsAmbient", "ambient.cricket"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("waterfall", "ambient.waterfall"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("waterfall_big", "ambient.waterfall"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("moneyDial", "ambient.coinsClinking"));
        
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("monkey1", "critters.monkey"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("parrot_squawk", "critters.parrot"));
        
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("doorClose", "interaction.doorClose"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("stairsdown", "interaction.footstepsDescend"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("harvest", "interaction.cropHarvest"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("book_read", "interaction.bookRead"));

        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("eat", "player.eating"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("gulp", "player.drinking"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("ow", "player.hurts"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("jingleBell", "player.footstepsJingle"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("throwDownITem", "player.itemThrown"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("questcomplete", "player.questComplete"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("stardrop", "player.stardrop"));
        
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("thunder", "ambient.thunder"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("thunder_small", "ambient.thunder"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("trainWhistle", "ambient.trainWhistle"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("distantTrain", "ambient.distantTrain"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("trainLoop", "ambient.trainLoop", 80 * 60));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("slosh", "world.waterSlosh"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("rooster", "world.rooster"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("leafrustle", "world.leafRustle"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("cavedrip", "ambient.caveDrip"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("bugLevelLoop", "ambient.bugLoop"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("wind", "ambient.wind"));
        
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("junimoMeep1", "critters.junimo"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("seagulls", "critters.seagull"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("SpringBirds", "critters.birdChirp"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("camel", "critters.camel"));

        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("scissors", "tools.shears"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("horse_flute", "tools.horseFlute"));

        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("batScreech", "monsters.batScreech"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("squid_move", "monsters.blueSquidMove"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("Duggy", "monsters.duggyDig"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("flybuzzing", "monsters.flyBuzz"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("slime", "monsters.slime"));
        
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("slingshot", "weapons.slingshot"));
        
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("parachute", "nightEvents.parachute"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("planeflyby", "nightEvents.planefly"));
        
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("cluck", "animals.chicken"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("Duck", "animals.duck"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("rabbit", "animals.rabbit"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("cow", "animals.cow"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("goat", "animals.goat"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("sheep", "animals.sheep"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("pig", "animals.pig"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("Ostrich", "animals.ostrich"));
        
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("cat", "pets.cat"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("dog_bark", "pets.dogBark"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("dog_pant", "pets.dogPant"));
        ModCaptionManager.Value.RegisterDefaultCaption(new Caption("turtle_pet", "pets.turtle"));
    }
}