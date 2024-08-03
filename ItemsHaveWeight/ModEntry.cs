using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.Objects;
using System;

namespace Encumbrance
{
    public class ModEntry : Mod
    {
        public float totalWeight;

        public Texture2D sprites;

        public static float defaultWeight = 1f;

        public static Dictionary<int, float> defaultCategoryWeights = new Dictionary<int, float>()
        {
            { StardewValley.Object.MilkCategory, 2 },
            { StardewValley.Object.BigCraftableCategory, 3 },
            { StardewValley.Object.metalResources, 3 },
            { StardewValley.Object.junkCategory, 0.5f },
            { StardewValley.Object.baitCategory, 0.5f },
            { StardewValley.Object.tackleCategory, 0.5f },
            { StardewValley.Object.syrupCategory, 2 },
            { StardewValley.Object.SeedsCategory, 0.5f },
            { StardewValley.Object.flowersCategory, 0.75f },
            { StardewValley.Object.ringCategory, 0.5f },
            { StardewValley.Object.weaponCategory, 2 },
            { StardewValley.Object.toolCategory, 2 },
        };

        public static Dictionary<int, float> furnitureWeights = new Dictionary<int, float>()
        {
            { Furniture.chair, 3 },
            { Furniture.bench, 4 },
            { Furniture.couch, 4 },
            { Furniture.armchair, 3 },
            { Furniture.dresser, 4 },
            { Furniture.longTable, 4 },
            { Furniture.painting, 2 },
            { Furniture.lamp, 2 },
            { Furniture.decor, 2 },
            { Furniture.bookcase, 4 },
            { Furniture.table, 3 },
            { Furniture.rug, 2 },
            { Furniture.window, 2 },
            { Furniture.fireplace, 4 },
            { Furniture.bed, 4 },
        };

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Player.InventoryChanged += OnInventoryChanged;
            helper.Events.Content.AssetRequested += OnAssetRequested;
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            totalWeight = TotalWeight(Game1.player.Items);

            CheckEncumbrance();
        }

        private void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer) return;

            totalWeight += TotalWeight(e.Added);
            totalWeight -= TotalWeight(e.Removed);
            totalWeight += TotalWeight(e.QuantityChanged);

            CheckEncumbrance();
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo("BarleyZP.ItemsHaveWeight/Sprites"))
            {
                e.LoadFromModFile<Texture2D>("assets/Sprites.png", AssetLoadPriority.Medium);
            }
        }

        private void CheckEncumbrance()
        {
            int encumbranceLevel = (int)Math.Floor(totalWeight / 750);
            if (encumbranceLevel == 0)
            {
                Game1.player.buffs.Remove("BarleyZP.encumbrance");
            }
            else
            {
                sprites = Game1.content.Load<Texture2D>("BarleyZP.ItemsHaveWeight/Sprites");
                BuffEffects effects = new(new StardewValley.GameData.Buffs.BuffAttributesData
                {
                    Speed = encumbranceLevel * -0.5f
                });
                Buff debuff = new("BarleyZP.encumbrance", duration: Buff.ENDLESS, iconTexture: sprites, iconSheetIndex: 0, effects: effects,
                    displayName: "Encumbered " + encumbranceLevel, description: "Everything you're\ncarrying is so heavy...");
                Game1.player.applyBuff(debuff);
            }
        }

        private float ItemWeight(Item item)
        {
            if (item is Furniture f) return furnitureWeights.GetValueOrDefault(f.furniture_type.Value, defaultWeight);
            return defaultCategoryWeights.GetValueOrDefault(item.Category, defaultWeight);
        }

        private float TotalWeight(IEnumerable<Item> items)
        {
            float weight = 0;

            foreach (Item item in items)
            {
                if (item is null) continue;
                weight += ItemWeight(item) * item.Stack;
            }

            return weight;
        }

        private float TotalWeight(IEnumerable<ItemStackSizeChange> items)
        {
            float weight = 0;

            foreach (ItemStackSizeChange item in items)
            {
                if (item is null || item.Item is null) continue;

                int quantityDiff = item.NewSize - item.OldSize;
                weight += ItemWeight(item.Item) * quantityDiff;
            }

            return weight;
        }
    }
}
