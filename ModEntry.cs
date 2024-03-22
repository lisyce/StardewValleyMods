using System;
using System.Linq;
using System.Reflection;
using GenericModConfigMenu;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using static BZP_Allergies.AllergenManager;

namespace BZP_Allergies {
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod {

        internal Harmony harmony;
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
            PatchFarmerAllergies.Initialize(this.Monitor);

            this.harmony = new(this.ModManifest.UniqueID);
            harmony.PatchAll();            
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
                    PatchAllergenObjects.AddAllergen(e, a);
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
                save: () => this.Helper.WriteConfig(this.Config),
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
                getValue: () => this.Config.FamerEggAllergy,
                setValue: value => this.Config.FamerEggAllergy = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Wheat",
                tooltip: () => "Your farmer will be allergic to any foods containing wheat.",
                getValue: () => this.Config.FamerWheatAllergy,
                setValue: value => this.Config.FamerWheatAllergy = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Fish",
                tooltip: () => "Your farmer will be allergic to any foods containing fish.",
                getValue: () => this.Config.FamerFishAllergy,
                setValue: value => this.Config.FamerFishAllergy = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Shellfish",
                tooltip: () => "Your farmer will be allergic to any foods containing shellfish.",
                getValue: () => this.Config.FamerShellfishAllergy,
                setValue: value => this.Config.FamerShellfishAllergy = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Tree Nuts",
                tooltip: () => "Your farmer will be allergic to any foods containing tree nuts.",
                getValue: () => this.Config.FamerTreenutAllergy,
                setValue: value => this.Config.FamerTreenutAllergy = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Dairy",
                tooltip: () => "Your farmer will be allergic to any foods containing dairy.",
                getValue: () => this.Config.FamerDairyAllergy,
                setValue: value => this.Config.FamerDairyAllergy = value
            );
        }
    }
}