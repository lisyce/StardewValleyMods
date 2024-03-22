using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using static BZP_Allergies.AllergenManager;

namespace BZP_Allergies {
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod {

        internal Harmony harmony;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper) {
            helper.Events.Content.AssetRequested += this.OnAssetRequested;

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
    }
}