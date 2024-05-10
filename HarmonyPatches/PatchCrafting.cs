using BZP_Allergies.HarmonyPatches.UI;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using System.Reflection.Emit;
using System.Reflection;

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

        public static IEnumerable<CodeInstruction> ClickCraftingRecipe_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // find the "createItem()" instruction
            // its result is in local var 1
            // we will use local 3 since it appears unused
            var createItemMethod = AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.createItem));

            bool foundCreateItem = false;
            bool done = false;

            var inputInstrList = new List<CodeInstruction>(instructions);

            var codes = new List<CodeInstruction>();
            for (int i = 0; i < inputInstrList.Count(); i++)
            {
                var instr = inputInstrList[i];

                if (foundCreateItem && !done)
                {
                    // put the recipe on the stack
                    codes.Add(new CodeInstruction(OpCodes.Ldloc_0));

                    // put the item back on the stack with lodloc.1
                    codes.Add(new CodeInstruction(OpCodes.Ldloc_1));

                    // put this._materialContainers on the stack
                    // ldarg.0
                    codes.Add(new CodeInstruction(OpCodes.Ldarg_0));

                    // ldfld class(List<IInventory>) StardewValley.Menus.CraftingPage::_materialContainers
                    var _materialContainers = AccessTools.Field(typeof(CraftingPage), nameof(CraftingPage._materialContainers));
                    codes.Add(new CodeInstruction(OpCodes.Ldfld, _materialContainers));

                    // call AddPotentialAllergiesFromCraftingToItem
                    var mine = AccessTools.Method(typeof(CraftingPatches), nameof(AddPotentialAllergiesFromCraftingToItem));
                    codes.Add(new CodeInstruction(OpCodes.Call, mine));
                }

                if (instr.opcode == OpCodes.Callvirt && instr.operand is MethodInfo minfo && minfo == createItemMethod)
                {
                    foundCreateItem = true;
                    codes.Add(instr);
                    i++;
                    codes.Add(inputInstrList[i]);
                }

                else codes.Add(instr);
            }       

            return codes.AsEnumerable();
        }

        public static void AddPotentialAllergiesFromCraftingToItem(CraftingRecipe recipe, Item crafted, List<IInventory> additionalMaterials)
        {
            // copy the inventories
            Inventory playerItems = CopyInventory(Game1.player.Items);

            List<IInventory?> additionalMaterialsCopy = new();
            foreach (IInventory inv in additionalMaterials)
            {
                if (inv == null)
                {
                    additionalMaterialsCopy.Add(inv);
                    continue;
                }
                additionalMaterialsCopy.Add(CopyInventory(inv));
            }

            // consume ingredients
            Dictionary<string, InventoryData> beforeConsume = CopyInventoryData(additionalMaterialsCopy);
            recipe.consumeIngredients(additionalMaterialsCopy);

            // see what was used
            Dictionary<string, InventoryData> afterConsume = CopyInventoryData(additionalMaterialsCopy);

            // subtract afterConsume amounts from beforeConsume to get the consumed materials
            ISet<InventoryData> usedItems = new HashSet<InventoryData>();
            foreach (var pair in beforeConsume)
            {
                beforeConsume[pair.Key].Stack -= afterConsume.GetValueOrDefault(pair.Key, new()).Stack;

                if (beforeConsume[pair.Key].Stack > 0)
                {
                    // we used some of this item
                    usedItems.Add(pair.Value);
                }
            }
            

            // what allergens did we cook this with?
            crafted.modData[Constants.ModDataMadeWith] = "";
            foreach (InventoryData item in usedItems)
            {
                if (item == null || item.Item == null) continue;

                // what allergens does it have?
                ISet<string> allergens = AllergenManager.GetAllergensInObject(item.Item as StardewValley.Object);
                foreach (string allergen in allergens)
                {
                    AllergenManager.ModDataSetAdd(crafted, Constants.ModDataMadeWith, allergen);
                }
            }

            // restore inventories
            Game1.player.Items.OverwriteWith(playerItems);

            for (int i=0; i < additionalMaterials.Count; i++)
            {
                if (additionalMaterials[i] == null) continue;
                additionalMaterials[i].OverwriteWith(additionalMaterialsCopy[i]);
            }
        }

        private static Inventory CopyInventory(IInventory inventory)
        {
            Inventory result = new();

            foreach (Item i in inventory)
            {
                if (i is null)
                {
                    result.Add(i);
                    continue;
                }

                Item copy = i.getOne();
                copy.Stack = i.Stack;
                result.Add(copy);
            }


            return result;
        }

        // includes farmer inventory (Game1.player.Items)
        private static Dictionary<string, InventoryData> CopyInventoryData (List<IInventory?>? inventories)
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

            foreach (IInventory? container in inventories)
            {
                if (container == null) continue;
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
