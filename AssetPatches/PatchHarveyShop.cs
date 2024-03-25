using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley.GameData;
using StardewValley.GameData.Shops;

namespace BZP_Allergies.AssetPatches
{
    internal class PatchHarveyShop
    {
        public static void Patch(AssetRequestedEventArgs e)
        {
            string path = PathUtilities.NormalizeAssetName(@"Data/Shops");
            if (e.NameWithoutLocale.IsEquivalentTo(path))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, ShopData>();
                    ShopItemData item = new()
                    {
                        ActionsOnPurchase = null,
                        CustomFields = null,
                        TradeItemId = null,
                        TradeItemAmount = 1,
                        Price = 1000,
                        ApplyProfitMargins = null,
                        AvailableStock = -1,
                        AvailableStockLimit = LimitedStockMode.Global,
                        AvoidRepeat = false,
                        UseObjectDataPrice = false,
                        IgnoreShopPriceModifiers = false,
                        PriceModifiers = null,
                        PriceModifierMode = QuantityModifier.QuantityModifierMode.Stack,
                        AvailableStockModifiers = null,
                        AvailableStockModifierMode = QuantityModifier.QuantityModifierMode.Stack,
                        Condition = null,
                        Id = "(O)BzpAllergies_AllergyMedicine",
                        ItemId = "(O)BzpAllergies_AllergyMedicine",
                        RandomItemId = null,
                        MaxItems = null,
                        MinStack = -1,
                        MaxStack = -1,
                        Quality = -1,
                        ObjectInternalName = null,
                        ObjectDisplayName = null,
                        ToolUpgradeLevel = -1,
                        IsRecipe = false,
                        StackModifiers = null,
                        StackModifierMode = QuantityModifier.QuantityModifierMode.Stack,
                        QualityModifiers = null,
                        QualityModifierMode = QuantityModifier.QuantityModifierMode.Stack,
                        ModData = null,
                        PerItemCondition = null
                    };

                    editor.Data["Hospital"].Items ??= new();
                    editor.Data["Hospital"].Items.Add(item);
                });
            }
        }
    }

}
