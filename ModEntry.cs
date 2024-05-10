using BZP_Allergies.Apis;
using BZP_Allergies.Config;
using BZP_Allergies.HarmonyPatches;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using System;
using static BZP_Allergies.AllergenManager;

namespace BZP_Allergies
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {

        public static ModEntry Instance { get; private set; }

        private Harmony Harmony;
        public ModConfigModel Config;
        public IModHelper ModHelper;

        public static readonly ISet<string> NpcsThatReactedToday = new HashSet<string>();

        public static readonly string MOD_ID = "BarleyZP.BzpAllergies";

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper modHelper)
        {
            Instance = this;

            ModHelper = modHelper;

            // allergen manager
            AllergenManager.InitDefault();

            // events
            modHelper.Events.GameLoop.GameLaunched += OnGameLaunched;
            modHelper.Events.Content.AssetRequested += OnAssetRequested;
            modHelper.Events.GameLoop.DayStarted += OnDayStarted;
            modHelper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            modHelper.Events.GameLoop.OneSecondUpdateTicking += OnOneSecondUpdateTicking;

            // config
            Config = Helper.ReadConfig<ModConfigModel>();

            // harmony patches
            Harmony = new(ModManifest.UniqueID);
            Harmony.Patch(
                original: AccessTools.Method(typeof(CraftingPage), "clickCraftingRecipe"),
                transpiler: new HarmonyMethod(typeof(CraftingPatches), nameof(CraftingPatches.ClickCraftingRecipe_Transpiler))
            );
            Harmony.Patch(
                original: AccessTools.PropertyGetter(AccessTools.TypeByName("SpaceCore.VanillaAssetExpansion.VAECustomCraftingRecipe"), "Description"),
                postfix: new HarmonyMethod(typeof(CraftingPatches), nameof(CraftingPatches.SpaceCoreVaeRecipeDescription_Postfix))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(SpaceCore.Framework.CustomCraftingRecipe), nameof(SpaceCore.Framework.CustomCraftingRecipe.drawRecipeDescription)),
                prefix: new HarmonyMethod(typeof(CraftingPatches), nameof(CraftingPatches.SpaceCoreFrameworkRecipeDescription_Prefix))
            );

            
            Harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.createQuestionDialogue), new Type[] { typeof(string), typeof(Response[]), typeof(string) }),
                prefix: new HarmonyMethod(typeof(PatchEatQuestionPopup), nameof(PatchEatQuestionPopup.CreateQuestionDialogue_Prefix))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.doneEating)),
                prefix: new HarmonyMethod(typeof(PatchFarmerDoneEating), nameof(PatchFarmerDoneEating.DoneEating_Prefix)),
                postfix: new HarmonyMethod(typeof(PatchFarmerDoneEating), nameof(PatchFarmerDoneEating.DoneEating_Postfix))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.getDescription)),
                postfix: new HarmonyMethod(typeof(PatchTooltip), nameof(PatchTooltip.GetDescription_Postfix))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(Item), nameof(Item.canStackWith)),
                postfix: new HarmonyMethod(typeof(PatchCanStack), nameof(PatchCanStack.CanStackWith_Postfix))
            );

            Harmony.Patch(
                original: AccessTools.Constructor(typeof(GameMenu), new Type[] { typeof(bool) }),
                postfix: new HarmonyMethod(typeof(PatchGameMenuConstructor), nameof(PatchGameMenuConstructor.Constructor_Postfix))
            );

            // console commands
            modHelper.ConsoleCommands.Add("bzpa_list_allergens", "Get a list of all possible allergens.", ListAllergens);
            modHelper.ConsoleCommands.Add("bzpa_get_held_allergens", "Get the allergens of the currently-held item.", GetAllergensOfHeldItem);
            modHelper.ConsoleCommands.Add("bzpa_player_allergies", "Get the player's allergies.", GetPlayerAllergies);
        }


        /*********
        ** Private methods
        *********/

        /// <inheritdoc cref="IContentEvents.AssetRequested"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("BarleyZP.BzpAllergies/Sprites"))
            {
                e.LoadFromModFile<Texture2D>(PathUtilities.NormalizePath(@"assets/Sprites.png"), AssetLoadPriority.Medium);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("BarleyZP.BzpAllergies/AllergyData"))
            {
                e.LoadFrom(() => AllergenManager.ALLERGEN_DATA, AssetLoadPriority.Medium);
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
            {
                Monitor.Log("No mod config menu API found.", LogLevel.Debug);
                return;
            }

            // config
            configMenu.Register(
                mod: ModManifest,
                reset: () => {
                    Config = new ModConfigModel();
                },
                save: () =>
                {
                    Helper.WriteConfig(Config);
                    Config = Helper.ReadConfig<ModConfigModel>();
                }
            );

            ConfigMenuInit.SetupMenuUI(configMenu, ModManifest);
        }

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            // make sure all the allergens the player "has" and "discovered" still exist
            ISet<string> has = AllergenManager.ModDataSetGet(Game1.player, Constants.ModDataHas);
            ISet<string> discovered = AllergenManager.ModDataSetGet(Game1.player, Constants.ModDataDiscovered);
            foreach (string id in has)
            {
                if (!AllergenManager.ALLERGEN_DATA_ASSET.ContainsKey(id))
                {
                    AllergenManager.ModDataSetRemove(Game1.player, Constants.ModDataHas, id);
                }
            }

            foreach (string id in discovered)
            {
                if (!AllergenManager.ALLERGEN_DATA_ASSET.ContainsKey(id))
                {
                    AllergenManager.ModDataSetRemove(Game1.player, Constants.ModDataDiscovered, id);
                }
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.DayStarted"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            NpcsThatReactedToday.Clear();
        }

        /// <inheritdoc cref="IGameLoopEvents.OneSecondUpdateTicking"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnOneSecondUpdateTicking(object? sender, OneSecondUpdateTickingEventArgs e)
        {
            if (!Config.HoldingReaction) return;

            bool hasReactionDebuff = Game1.player.hasBuff(Constants.ReactionDebuff);

            StardewValley.Object? held = Game1.player.ActiveObject;
            bool allergic = FarmerIsAllergic(held);
            if (held is not null && allergic)
            {
                if (hasReactionDebuff && !e.IsMultipleOf(300)) return;
                Game1.player.applyBuff(AllergenManager.GetAllergicReactionBuff(held.DisplayName, "hold", 60000));
                CheckForAllergiesToDiscover(Game1.player, GetAllergensInObject(held));
            }
        }

        private void ListAllergens(string command, string[] args) {

            string result = "\n{Allergen Id}: {Allergen Display Name}";

            foreach (var item in AllergenManager.ALLERGEN_DATA_ASSET)
            {
                result += "\n  " + item.Key + ": " + item.Value.DisplayName;
            }

            Monitor.Log(result, LogLevel.Info);
        }

        private void GetAllergensOfHeldItem(string command, string[] args)
        {
            ISet<string> result = new HashSet<string>();
            Item currItem = Game1.player.CurrentItem;

            if (currItem is StardewValley.Object currObj)
            {
                result = GetAllergensInObject(currObj);
            }

            Monitor.Log(string.Join(", ", result), LogLevel.Info);
        }

        private void GetPlayerAllergies(string command, string[] args)
        {
            ISet<string> has = ModDataSetGet(Game1.player, Constants.ModDataHas);
            ISet<string> discovered = ModDataSetGet(Game1.player, Constants.ModDataDiscovered);

            string result = "\n{Allergen Id}: {Discovered}";
            foreach (string a in has)
            {
                result += "\n  " + a + ": " + discovered.Contains(a);
            }
            
            Monitor.Log(result, LogLevel.Info);
        }
    }
}