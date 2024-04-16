using BZP_Allergies.Apis;
using StardewModdingAPI;

namespace BZP_Allergies.Config
{
    internal class ConfigMenuInit
    {
        public static void SetupMenuUI(IGenericModConfigMenuApi configMenu, IManifest modManifest)
        {
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
        }
    }
}
