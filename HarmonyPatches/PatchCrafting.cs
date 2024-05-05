using BZP_Allergies.HarmonyPatches.UI;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;

namespace BZP_Allergies.HarmonyPatches
{
    internal class InventoryData
    {
        public InventoryData()
        {
            Stack = 0;
            @Item = null;
        }

        public int Stack {  get; set; }
        public Item? @Item { get; set; }
    }


    internal class CraftingPatches
    {
        public static StardewValley.Object? craftedObj = null;
        public static void SpaceCoreVaeRecipeDescription_Postfix(SpaceCore.VanillaAssetExpansion.VAECraftingRecipe __instance, ref string __result)
        {
            try
            {
                if (__result.Length > 0)
                {
                    return;
                }

                Traverse traverse = Traverse.Create(__instance);
                bool cooking = traverse.Field("cooking").GetValue<bool>();
                if (cooking)
                {
                    string id = traverse.Field("id").GetValue<string>();
                    CraftingRecipe vanillaRecipe = new(id, true);
                    __result = vanillaRecipe.description;
                }

            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.Log($"Failed in {nameof(SpaceCoreVaeRecipeDescription_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static bool SpaceCoreFrameworkRecipeDescription_Prefix(SpaceCore.Framework.CustomCraftingRecipe __instance, SpriteBatch b,
            Vector2 position, int width, IList<Item> additional_crafting_items)
        {
            SpaceCore.CustomCraftingRecipe privRecipe = Traverse.Create(__instance).Field("recipe").GetValue<SpaceCore.CustomCraftingRecipe>();

            int lineExpansion = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 8 : 0);
            b.Draw(Game1.staminaRect, new Rectangle((int)(position.X + 8f), (int)(position.Y + 32f + Game1.smallFont.MeasureString("Ing!").Y) - 4 - 2 - (int)((float)lineExpansion * 1.5f), width - 32, 2), Game1.textColor * 0.35f);
            Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.567"), Game1.smallFont, position + new Vector2(8f, 28f), Game1.textColor * 0.75f);
            for (int i = 0; i < privRecipe.Ingredients.Length; i++)
            {
                var ingred = privRecipe.Ingredients[i];
                int required_count = ingred.Quantity;
                int bag_count = ingred.GetAmountInList(Game1.player.Items);
                int containers_count = 0;
                required_count -= bag_count;
                if (additional_crafting_items != null)
                {
                    containers_count = ingred.GetAmountInList(additional_crafting_items);
                    if (required_count > 0)
                    {
                        required_count -= containers_count;
                    }
                }
                string ingredient_name_text = ingred.DispayName;
                Color drawColor = ((required_count <= 0) ? Game1.textColor : Color.Red);
                b.Draw(ingred.IconTexture, new Vector2(position.X, position.Y + 64f + (float)(i * 64 / 2) + (float)(i * 4)), ingred.IconSubrect, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.86f);
                Utility.drawTinyDigits(ingred.Quantity, b, new Vector2(position.X + 32f - Game1.tinyFont.MeasureString(ingred.Quantity.ToString() ?? "").X, position.Y + 64f + (float)(i * 64 / 2) + (float)(i * 4) + 21f), 2f, 0.87f, Color.AntiqueWhite);
                Vector2 text_draw_position = new Vector2(position.X + 32f + 8f, position.Y + 64f + (float)(i * 64 / 2) + (float)(i * 4) + 4f);
                Utility.drawTextWithShadow(b, ingredient_name_text, Game1.smallFont, text_draw_position, drawColor);
                if (Game1.options.showAdvancedCraftingInformation)
                {
                    text_draw_position.X = position.X + (float)width - 40f;
                    b.Draw(Game1.mouseCursors, new Rectangle((int)text_draw_position.X, (int)text_draw_position.Y + 2, 22, 26), new Rectangle(268, 1436, 11, 13), Color.White);
                    Utility.drawTextWithShadow(b, (bag_count + containers_count).ToString() ?? "", Game1.smallFont, text_draw_position - new Vector2(Game1.smallFont.MeasureString(bag_count + containers_count + " ").X, 0f), drawColor);
                }
            }
            b.Draw(Game1.staminaRect, new Rectangle((int)position.X + 8, (int)position.Y + lineExpansion + 64 + 4 + privRecipe.Ingredients.Length * 36, width - 32, 2), Game1.textColor * 0.35f);
            Utility.drawTextWithShadow(b, Game1.parseText(privRecipe.Description, Game1.smallFont, width - 8), Game1.smallFont, position + new Vector2(0f, 76 + privRecipe.Ingredients.Length * 36 + lineExpansion), Game1.textColor * 0.75f);
            
            return false;  // don't run original (this is pretty much a copy anyway)
        }

        public static void CreateItem_Postfix(ref Item __result)
        {
            try
            {
                if (__result is StardewValley.Object obj && obj.Edibility > -300 && !obj.QualifiedItemId.Equals("(O)921"))  // must be edible and not the ravioli
                {
                    craftedObj = obj;
                } else
                {
                    craftedObj = null;
                }
            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.Log($"Failed in {nameof(CreateItem_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void ConsumeIngredients_Prefix(out Dictionary<string, InventoryData>? __state, List<IInventory> additionalMaterials)
        {
            try
            {
                if (craftedObj == null)
                {
                    __state = null;
                    return;
                }

                // item counts and qualified ids in the original inventory
                __state = CopyInventoryData(additionalMaterials);
            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.Log($"Failed in {nameof(ConsumeIngredients_Prefix)}:\n{ex}", LogLevel.Error);
                __state = null;
            }
        }

        public static void ConsumeIngredients_Postfix(Dictionary<string, InventoryData>? __state, List<IInventory> additionalMaterials)
        {
            try
            {
                if (craftedObj == null)
                {
                    return;
                }

                if (__state == null)
                {
                    throw new Exception(nameof(ConsumeIngredients_Prefix) + " did not set output state.");
                }

                // get the item counts after ingredients consumed
                Dictionary<string, InventoryData> afterConsume = CopyInventoryData(additionalMaterials);

                // subtract afterConsume amounts from __state (before consume) to get the consumed materials
                ISet<InventoryData> usedItems = new HashSet<InventoryData>();
                foreach (var pair in __state)
                {
                    __state[pair.Key].Stack -= afterConsume.GetValueOrDefault(pair.Key, new()).Stack;

                    if (__state[pair.Key].Stack > 0)
                    {
                        // we used some of this item
                        usedItems.Add(pair.Value);
                    }
                }

                // what allergens did we cook this with?
                craftedObj.modData[Constants.ModDataMadeWith] = "";
                foreach (InventoryData item in usedItems)
                {
                    if (item == null || item.Item == null) continue;

                    // what allergens does it have?
                    ISet<string> allergens = AllergenManager.GetAllergensInObject(item.Item as StardewValley.Object);
                    foreach (string allergen in allergens)
                    {
                        AllergenManager.ModDataSetAdd(craftedObj, Constants.ModDataMadeWith, allergen);
                    }
                }
            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.Log($"Failed in {nameof(ConsumeIngredients_Postfix)}:\n{ex}", LogLevel.Error);
            }
            finally
            {
                craftedObj = null;  // reset for next run
            }
        }

        // includes farmer inventory (Game1.player.Items)
        private static Dictionary<string, InventoryData> CopyInventoryData (List<IInventory>? inventories)
        {
            Dictionary<string, InventoryData> results = new();
            foreach (Item i in Game1.player.Items)
            {
                if (i == null) continue;
                if (!results.ContainsKey(i.QualifiedItemId))
                {
                    results[i.QualifiedItemId] = new();
                }
                InventoryData data = results[i.QualifiedItemId];
                data.Stack += i.Stack;
                data.Item = i;
            }

            if (inventories == null)
            {
                return results;
            }

            foreach (IInventory container in inventories)
            {
                foreach (Item i in container)
                {
                    if (i == null) continue;
                    if (!results.ContainsKey(i.QualifiedItemId))
                    {
                        results[i.QualifiedItemId] = new();
                    }
                    InventoryData data = results[i.QualifiedItemId];
                    data.Stack += i.Stack;
                    data.Item = i;
                }
            }

            return results;
        }
    }

}
