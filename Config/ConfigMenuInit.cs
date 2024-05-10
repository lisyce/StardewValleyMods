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
                text: () => "Difficulty Settings"
            );

            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => "Hints Before Eating",
                tooltip: () => "Get a hint in the popup before you eat something to warn you of your allergies.",
                getValue: () => ModEntry.Instance.Config.HintBeforeEating,
                setValue: value => ModEntry.Instance.Config.HintBeforeEating = value
            );

            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => "Reaction From Holding Foods",
                tooltip: () => "Holding foods you're allergic to gives your farmer a reaction.",
                getValue: () => ModEntry.Instance.Config.HoldingReaction,
                setValue: value => ModEntry.Instance.Config.HoldingReaction = value
            );

            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => "Reaction From Cooking Foods",
                tooltip: () => "Cooking foods you're allergic to gives your farmer a reaction.",
                getValue: () => ModEntry.Instance.Config.CookingReaction,
                setValue: value => ModEntry.Instance.Config.CookingReaction = value
            );

            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => "Enable Nausea",
                tooltip: () => "Enable/disable the random nausea that accompanies eating a food you're allergic to.",
                getValue: () => ModEntry.Instance.Config.EnableNausea,
                setValue: value => ModEntry.Instance.Config.EnableNausea = value
            );

            configMenu.AddNumberOption(
                mod: modManifest,
                name: () => "Debuff Length (Seconds)",
                tooltip: () => "Length of the debuff from eating food; Other debuff lengths are based upon this value.",
                getValue: () => ModEntry.Instance.Config.EatingDebuffLengthSeconds,
                setValue: value => ModEntry.Instance.Config.EatingDebuffLengthSeconds = value
            );

            configMenu.AddNumberOption(
                mod: modManifest,
                name: () => "Debuff Severity Multiplier",
                tooltip: () => "Severity multiplier for the allergic reaction debuff to speed, attack, and defense.",
                getValue: () => ModEntry.Instance.Config.DebuffSeverityMultiplier,
                setValue: value => ModEntry.Instance.Config.DebuffSeverityMultiplier = value
            );

            configMenu.AddSectionTitle(
                mod: modManifest,
                text: () => "Random Allergy Settings"
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
