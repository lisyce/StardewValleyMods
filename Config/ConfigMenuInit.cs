using BZP_Allergies.Apis;
using StardewModdingAPI;

namespace BZP_Allergies.Config
{
    internal class ConfigMenuInit
    {
        public static void SetupMenuUI(IGenericModConfigMenuApi configMenu, IManifest modManifest)
        {
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

            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => "Random Allergy Count Hint",
                tooltip: () => "Select to get a hint in allergy menu to see how many more allergies you haven't discovered.",
                getValue: () => ModEntry.Config.AllergenCountHint,
                setValue: value => ModEntry.Config.AllergenCountHint = value
            );

            configMenu.AddNumberOption(
                mod: modManifest,
                name: () => "Number of Random Allergies",
                tooltip: () => "If you roll random allergies in-game, this determines how many you'll get.",
                getValue: () => ModEntry.Config.NumberRandomAllergies,
                setValue: value =>
                {
                    if (value < -1) value = -1;
                    if (value > AllergenManager.ALLERGEN_DATA.Count) value = AllergenManager.ALLERGEN_DATA.Count;
                    ModEntry.Config.NumberRandomAllergies = value;
                }
            );

            configMenu.AddParagraph(modManifest, () => "Set to -1 for a random number of random allergies!");
        }
    }
}
