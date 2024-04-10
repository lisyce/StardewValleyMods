using BZP_Allergies.Apis;
using StardewModdingAPI;

namespace BZP_Allergies.Config
{
    internal class ConfigMenuInit
    {
        public static void SetupMenuUI(IGenericModConfigMenuApi configMenu, IManifest modManifest)
        {
            // add a link to config
            configMenu.AddPageLink(modManifest, "BarleyZP.BzpAllergies_Farmer", () => "Farmer Allergies");

            // switch to page
            configMenu.AddPage(modManifest, "BarleyZP.BzpAllergies_Farmer", () => "Farmer Allergies");

            // add some config options
            configMenu.AddSectionTitle(
                mod: modManifest,
                text: () => "Farmer Allergies"
            );
            configMenu.AddParagraph(
                mod: modManifest,
                text: () => "Eating a food containing an allergen results in a loss of energy and debuffs. Both raw ingredients and derived items may cause reactions."
            );

            GenericAllergenConfig farmerConfig = ModEntry.Config.Farmer;

            List<string> mainAllergies = new()
            {
                "egg", "wheat", "fish", "shellfish", "treenuts", "dairy"
            };
            mainAllergies.Sort();

            foreach (string id in mainAllergies)
            {
                string displayName = AllergenManager.GetAllergenDisplayName(id);
                configMenu.AddBoolOption(
                    mod: modManifest,
                    name: () => displayName,
                    tooltip: () => "Your farmer will be allergic to any foods containing " + displayName.ToLower() + ".",
                    getValue: () => farmerConfig.Allergies[id],
                    setValue: value => farmerConfig.Allergies[id] = value
                );
            }
        }

        public static void SetupContentPackConfig(IGenericModConfigMenuApi configMenu, IManifest modManifest, IContentPack pack)
        {
            // switch to farmer allergies page
            configMenu.AddPage(modManifest, "BarleyZP.BzpAllergies_Farmer", () => "Farmer Allergies");

            // add a link to config
            configMenu.AddPageLink(modManifest, pack.Manifest.UniqueID, () => pack.Manifest.Name);

            // switch to page
            configMenu.AddPage(modManifest, pack.Manifest.UniqueID, () => "Farmer Allergies");

            // add a link back
            configMenu.AddPageLink(modManifest, "BarleyZP.BzpAllergies_Farmer", () => "Back");

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
                    getValue: () => farmerConfig.Allergies[allergen],
                    setValue: value => farmerConfig.Allergies[allergen] = value
                );
            }
        }
    }
}
