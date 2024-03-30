using BZP_Allergies.Apis;
using StardewModdingAPI;

namespace BZP_Allergies.Config
{
    internal class ConfigMenuInit
    {
        public static void SetupMenuUI(IGenericModConfigMenuApi configMenu, IManifest modManifest, ModConfig config)
        {
            
            // add some config options
            configMenu.AddSectionTitle(
                mod: modManifest,
                text: () => "Farmer Allergies"
            );
            configMenu.AddParagraph(
                mod: modManifest,
                text: () => "Eating a food containing an allergen results in a loss of energy and debuffs. Both raw ingredients and cooked foods may cause reactions."
            );

            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => "Eggs",
                tooltip: () => "Your farmer will be allergic to any foods containing eggs.",
                getValue: () => config.Farmer.EggAllergy,
                setValue: value => config.Farmer.EggAllergy = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => "Wheat",
                tooltip: () => "Your farmer will be allergic to any foods containing wheat.",
                getValue: () => config.Farmer.WheatAllergy,
                setValue: value => config.Farmer.WheatAllergy = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => "Fish",
                tooltip: () => "Your farmer will be allergic to any foods containing fish.",
                getValue: () => config.Farmer.FishAllergy,
                setValue: value => config.Farmer.FishAllergy = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => "Shellfish",
                tooltip: () => "Your farmer will be allergic to any foods containing shellfish.",
                getValue: () => config.Farmer.ShellfishAllergy,
                setValue: value => config.Farmer.ShellfishAllergy = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => "Tree Nuts",
                tooltip: () => "Your farmer will be allergic to any foods containing tree nuts.",
                getValue: () => config.Farmer.TreenutAllergy,
                setValue: value => config.Farmer.TreenutAllergy = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => "Dairy",
                tooltip: () => "Your farmer will be allergic to any foods containing dairy.",
                getValue: () => config.Farmer.DairyAllergy,
                setValue: value => config.Farmer.DairyAllergy = value
            );
        }
    }
}
