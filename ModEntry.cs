using BZP_Allergies.Config;
using GenericModConfigMenu;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using static BZP_Allergies.AllergenManager;

namespace BZP_Allergies
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {

        private Harmony Harmony;
        private ModConfig Config;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper) {
            // events
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;

            // config
            this.Config = this.Helper.ReadConfig<ModConfig>();

            // harmony patches
            PatchFarmerAllergies.Initialize(this.Monitor, this.Config);

            this.Harmony = new(this.ModManifest.UniqueID);
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
                    PatchAllergenObjects.AddAllergen(e, a, Config);
                }
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
            {
                Monitor.Log("No mod config menu API found.", LogLevel.Debug);
                return;
            }

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => {
                    this.Monitor.Log("Saving", LogLevel.Debug);
                    this.Helper.WriteConfig(this.Config);
                    this.Config = this.Helper.ReadConfig<ModConfig>();
                    this.Helper.GameContent.InvalidateCache("Data/Objects");
                },
                titleScreenOnly: false
            );

            // add some config options
            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Famer Allergies"
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "Eating a food containing an allergen results in a loss of energy and debuffs."
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Eggs",
                tooltip: () => "Your farmer will be allergic to any foods containing eggs.",
                getValue: () => this.Config.Farmer.EggAllergy,
                setValue: value => this.Config.Farmer.EggAllergy = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Wheat",
                tooltip: () => "Your farmer will be allergic to any foods containing wheat.",
                getValue: () => this.Config.Farmer.WheatAllergy,
                setValue: value => this.Config.Farmer.WheatAllergy = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Fish",
                tooltip: () => "Your farmer will be allergic to any foods containing fish.",
                getValue: () => this.Config.Farmer.FishAllergy,
                setValue: value => this.Config.Farmer.FishAllergy = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Shellfish",
                tooltip: () => "Your farmer will be allergic to any foods containing shellfish.",
                getValue: () => this.Config.Farmer.ShellfishAllergy,
                setValue: value => this.Config.Farmer.ShellfishAllergy = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Tree Nuts",
                tooltip: () => "Your farmer will be allergic to any foods containing tree nuts.",
                getValue: () => this.Config.Farmer.TreenutAllergy,
                setValue: value => this.Config.Farmer.TreenutAllergy = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Dairy",
                tooltip: () => "Your farmer will be allergic to any foods containing dairy.",
                getValue: () => this.Config.Farmer.DairyAllergy,
                setValue: value => this.Config.Farmer.DairyAllergy = value
            );
        }
    }
}