using BZP_Allergies.Apis;
using BZP_Allergies.AssetPatches;
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
using System.Buffers;
using static BZP_Allergies.AllergenManager;

namespace BZP_Allergies
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {

        private Harmony Harmony;
        private ModConfig Config;
        private IModHelper ModHelper;
        private Texture2D Sprites;

        public static readonly string MOD_ID = "BarleyZP.BzpAllergies";

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper modHelper)
        {
            ModHelper = modHelper;

            // allergen manager
            AllergenManager.Initialize(Monitor, Config, ModHelper.GameContent, ModHelper.ModContent);

            // events
            modHelper.Events.GameLoop.GameLaunched += OnGameLaunched;
            modHelper.Events.Content.AssetRequested += OnAssetRequested;
            modHelper.Events.Content.AssetReady += OnAssetReady;

            // config
            Config = Helper.ReadConfig<ModConfig>();

            // harmony patches
            PatchFarmerDoneEating.Initialize(Monitor, Config, ModHelper.GameContent, ModHelper.ModContent);
            PatchEatQuestionPopup.Initialize(Monitor, Config, ModHelper.GameContent, ModHelper.ModContent);

            Harmony = new(ModManifest.UniqueID);
            Harmony.PatchAll();    
        }


        /*********
        ** Private methods
        *********/

        /// <inheritdoc cref="IContentEvents.AssetRequested"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                foreach (Allergens a in Enum.GetValues<Allergens>())
                {
                    PatchObjects.AddAllergen(e, a);
                }
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Mods/BarleyZP.BzpAllergies/Sprites"))
            {
                e.LoadFromModFile<Texture2D>(PathUtilities.NormalizePath(@"assets/Sprites.png"), AssetLoadPriority.Medium);
            }
        }

        /// <inheritdoc cref="IContentEvents.AssetReady"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnAssetReady(object sender, AssetReadyEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Mods/BarleyZP.BzpAllergies/Sprites"))
            {
                this.Sprites = Game1.content.Load<Texture2D>("Mods/BarleyZP.BzpAllergies/Sprites");
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
            {
                Monitor.Log("No mod config menu API found.", LogLevel.Debug);
                return;
            }

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => {
                    Helper.WriteConfig(Config);
                    Config = Helper.ReadConfig<ModConfig>();
                },
                titleScreenOnly: false
            );

            ConfigMenuInit.SetupMenuUI(configMenu, ModManifest, Config);
        }
    }
}