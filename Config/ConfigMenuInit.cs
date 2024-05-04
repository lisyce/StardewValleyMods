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
                getValue: () => ModEntry.Instance.Config.HintBeforeEating,
                setValue: value => ModEntry.Instance.Config.HintBeforeEating = value
            );

            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => "Random Allergy Count Hint",
                tooltip: () => "Select to get a hint in allergy menu to see how many more allergies you haven't discovered.",
                getValue: () => ModEntry.Instance.Config.AllergenCountHint,
                setValue: value => ModEntry.Instance.Config.AllergenCountHint = value
            );

            configMenu.AddNumberOption(
                mod: modManifest,
                name: () => "Number of Random Allergies",
                tooltip: () => "If you roll random allergies in-game, this determines how many you'll get.",
                getValue: () => ModEntry.Instance.Config.NumberRandomAllergies,
                setValue: value =>
                {
                    if (value < -1) value = -1;
                    if (value > AllergenManager.ALLERGEN_DATA_ASSET.Count) value = AllergenManager.ALLERGEN_DATA_ASSET.Count;
                    ModEntry.Instance.Config.NumberRandomAllergies = value;
                }
            );

            configMenu.AddParagraph(modManifest, () => "Set to -1 for a random number of random allergies!");
        }
    }
}
