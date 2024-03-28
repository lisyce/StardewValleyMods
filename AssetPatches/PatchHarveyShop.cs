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
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, ShopData>();
                    ShopItemData allergyMedicine = new()
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
                        Id = "(O)" + AllergenManager.ALLERGY_RELIEF_ID,
                        ItemId = "(O)" + AllergenManager.ALLERGY_RELIEF_ID,
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

                    ShopItemData lactasePills = new()
                    {
                        ActionsOnPurchase = null,
                        CustomFields = null,
                        TradeItemId = null,
                        TradeItemAmount = 1,
                        Price = 1500,
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
                        Id = "(O)" + AllergenManager.LACTASE_PILLS_ID,
                        ItemId = "(O)" + AllergenManager.LACTASE_PILLS_ID,
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
                    editor.Data["Hospital"].Items.Add(allergyMedicine);
                    editor.Data["Hospital"].Items.Add(lactasePills);
                });
            }
        }
    }

}
