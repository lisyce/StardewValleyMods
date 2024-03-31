using BZP_Allergies.Apis;
using StardewModdingAPI;

namespace BZP_Allergies.Config
{
    internal class ConfigMenuInit
    {
        public static void SetupMenuUI(IGenericModConfigMenuApi configMenu, IManifest modManifest)
        {

            // add some config options
            configMenu.AddSectionTitle(
                mod: modManifest,
                text: () => "Farmer Allergies"
            );
            configMenu.AddParagraph(
                mod: modManifest,
                text: () => "Eating a food containing an allergen results in a loss of energy and debuffs. Both raw ingredients and derived items may cause reactions."
            );

            ISet<string> mainAllergies = new HashSet<string>()
            {
                "egg", "wheat", "fish", "shellfish", "treenuts", "dairy"
            };


            bool hasContentPackAllergens = ModEntry.Config.Farmer.Allergies.Count > mainAllergies.Count;

            foreach (string id in ModEntry.Config.Farmer.Allergies.Keys)
            {
                string displayName = AllergenManager.GetAllergenDisplayName(id);
                configMenu.AddBoolOption(
                    mod: modManifest,
                    name: () => displayName,
                    tooltip: () => "Your farmer will be allergic to any foods containing " + displayName.ToLower() + ".",
                    getValue: () => ModEntry.Config.Farmer.Allergies[id],
                    setValue: value => ModEntry.Config.Farmer.Allergies[id] = value
                );

                if (!mainAllergies.Contains(id))
                {
                    configMenu.AddParagraph(
                        mod: modManifest,
                        text: () => "From " + AllergenManager.ALLERGEN_CONTENT_PACK.GetValueOrDefault(id, "Unknown Pack")
                    );
                }
            }            
        }
    }
}
