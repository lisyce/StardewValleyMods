using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;

namespace BZP_Allergies.HarmonyPatches
{
    public class InventoryData
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
            ModEntry.Instance.Monitor.Log("consume", LogLevel.Debug);
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
