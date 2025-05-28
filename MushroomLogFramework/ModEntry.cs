using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MushroomLogFramework;

public class ModEntry : Mod
{
    private static readonly string ProduceRulesAssetName = "BarleyZP.MushroomLogFramework/ProduceRules";
    private static Dictionary<string, MushroomLogData>? _produceRules;

    internal static IMonitor ModMonitor;

    internal static Dictionary<string, MushroomLogData> ProduceRules
    {
        get
        {
            _produceRules ??= Game1.content.Load<Dictionary<string, MushroomLogData>>(ProduceRulesAssetName);
            return _produceRules;
        }
    }
    
    public override void Entry(IModHelper helper)
    {
        ModMonitor = Monitor;
        
        helper.Events.Content.AssetRequested += OnAssetRequested;
        helper.Events.Content.AssetsInvalidated += OnAssetsInvalidated;

        var harmony = new Harmony(ModManifest.UniqueID);
        Harmony.DEBUG = true;
        HarmonyPatches.Patch(harmony);
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(ProduceRulesAssetName))
        {
            e.LoadFromModFile<Dictionary<string, MushroomLogData>>("assets/VanillaProduceRules.json", AssetLoadPriority.Exclusive);
        }
    }

    private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(x => x.IsEquivalentTo(ProduceRulesAssetName)))
        {
            _produceRules = null;
        }
    }
}