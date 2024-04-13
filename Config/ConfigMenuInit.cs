using BZP_Allergies.Apis;
using StardewModdingAPI;

namespace BZP_Allergies.Config
{
    internal class ConfigMenuInit
    {
        public static void SetupMenuUI(IGenericModConfigMenuApi configMenu, IManifest modManifest)
        {
            // add a link to config
            configMenu.AddPageLink(modManifest, "BzpAllergies_Farmer", () => "Farmer Allergies");          

            // switch to page
            configMenu.AddPage(modManifest, "BzpAllergies_Farmer", () => "Farmer Allergies");

            // general settings
            configMenu.AddSectionTitle(
                mod: modManifest,
                text: () => "General Settings"
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => "Hints before eating",
                tooltip: () => "Select to get a hint in the popup before you eat something to warn you of your allergies.",
                getValue: () => ModEntry.Config.HintBeforeEating,
                setValue: value => ModEntry.Config.HintBeforeEating = value
            );

            // randomization options
            configMenu.AddSectionTitle(
                mod: modManifest,
                text: () => "Randomize Allergies"
            );

            configMenu.AddParagraph(
                mod: modManifest,
                text: () => "Randomize your allergies for an extra challenge! If you choose this option, the game will randomly roll some allergies for you and you'll have to discover them as you play!"
            );

            // add some config options
            configMenu.AddSectionTitle(
                mod: modManifest,
                text: () => "Farmer Allergies"
            );
            configMenu.AddParagraph(
                mod: modManifest,
                text: () => "Eating a food containing an allergen results in a loss of energy and debuffs. Both raw ingredients and derived items may cause reactions."
            );


            List<string> mainAllergies = new()
            {
                "egg", "wheat", "fish", "shellfish", "treenuts", "dairy", "mushroom"
            };
            mainAllergies.Sort();

            foreach (string id in mainAllergies)
            {
                string displayName = AllergenManager.GetAllergenDisplayName(id);
                configMenu.AddBoolOption(
                    mod: modManifest,
                    name: () => displayName,
                    tooltip: () => "Your farmer will be allergic to any foods containing " + displayName.ToLower() + ".",
                    getValue: () => ModEntry.Config.Farmer.Allergies.GetValueOrDefault(id, false),
                    setValue: value => ModEntry.Config.Farmer.Allergies[id] = value
                );
            }
        }

        public static void SetupContentPackConfig(IGenericModConfigMenuApi configMenu, IManifest modManifest, IContentPack pack)
        {
            // switch to farmer allergies page
            configMenu.AddPage(modManifest, "BzpAllergies_Farmer", () => "Farmer Allergies");

            // add a link to config
            configMenu.AddPageLink(modManifest, pack.Manifest.UniqueID, () => pack.Manifest.Name);

            // switch to page
            configMenu.AddPage(modManifest, pack.Manifest.UniqueID, () => "Farmer Allergies");

            // add a link back
            configMenu.AddPageLink(modManifest, "BzpAllergies_Farmer", () => "Back");

            // title
            configMenu.AddSectionTitle(
                mod: modManifest,
                text: () => pack.Manifest.Name
            );

            // register options
            GenericAllergenConfig farmerConfig = ModEntry.Config.Farmer;
            List<string> sortedAllergens = AllergenManager.ALLERGEN_CONTENT_PACK[pack.Manifest.UniqueID].ToList();
            sortedAllergens.Sort();

            foreach (var allergen in sortedAllergens)
            {
                string displayName = AllergenManager.GetAllergenDisplayName(allergen);
                configMenu.AddBoolOption(
                    mod: modManifest,
                    name: () => displayName,
                    tooltip: () => "Your farmer will be allergic to any foods containing " + displayName.ToLower() + ".",
                    getValue: () => ModEntry.Config.Farmer.Allergies.GetValueOrDefault(allergen, false),
                    setValue: value => ModEntry.Config.Farmer.Allergies[allergen] = value
                );
            }
        }
    }
}
