using StardewValley.GameData.Objects;
using StardewModdingAPI;
using System.Text.RegularExpressions;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;

namespace BZP_Allergies
{
    internal class AllergenManager : Initializable
    {
        public static readonly string ALLERIC_REACTION_DEBUFF = string.Format("{0}_allergic_reaction", ModEntry.MOD_ID);
        public static readonly string LACTASE_PILLS_BUFF = string.Format("{0}_buff_2", ModEntry.MOD_ID);
        public static readonly string REACTION_EVENT = string.Format("{0}_had_allergic_reaction", ModEntry.MOD_ID);

        public static readonly string ALLERGY_RELIEF_ID = string.Format("{0}_AllergyMedicine", ModEntry.MOD_ID);
        public static readonly string LACTASE_PILLS_ID = string.Format("{0}_LactasePills", ModEntry.MOD_ID);

        public static readonly string REACTION_DIALOGUE_KEY = string.Format("{0}_farmer_allergic_reaction", ModEntry.MOD_ID);
        public static readonly string FARMER_DISCOVERED_ALLERGIES_MODDATA_KEY = "BarleyZP.BzpAllergies_DiscoveredAllergies";
        public static readonly string FARMER_HAS_ALLERGIES_MODDATA_KEY = "BarleyZP.BzpAllergies_PlayerAllergies";

        public static readonly Dictionary<string, AllergenModel> ALLERGEN_DATA = new();

        public static void InitDefault()
        {
            ALLERGEN_DATA.Clear();

            AllergenModel egg = new("Eggs");
            egg.AddObjectIds(new HashSet<string>{
                    "194", "195", "201", "203", "211", "213", "220", "221", "223", "234", "240", "648",
                    "732"
                });
            egg.AddTags(new HashSet<string> { "egg_item", "mayo_item", "large_egg_item" });
            ALLERGEN_DATA["egg"] = egg;

            AllergenModel wheat = new("Wheat");
            wheat.AddObjectIds(new HashSet<string>{
                    "198", "201", "202", "203", "206", "211", "214", "216", "220", "221", "222", "223",
                    "224", "234", "239", "241", "604", "608", "611", "618", "651", "731", "732", "246",
                    "262", "346"
                });
            ALLERGEN_DATA["wheat"] = wheat;

            AllergenModel fish = new("Fish");
            fish.AddObjectIds(new HashSet<string>{
                    "198", "202", "204", "212", "213", "214", "219", "225", "226", "227", "228", "242",
                    "265", "445"
                });
            ALLERGEN_DATA["fish"] = fish;

            AllergenModel shellfish = new("Shellfish");
            shellfish.AddObjectIds(new HashSet<string>{
                    "203", "218", "227", "228", "727", "728", "729", "730", "732", "733", "715", "372",
                    "717", "718", "719", "720", "723", "716", "721", "722"
                });
            ALLERGEN_DATA["shellfish"] = shellfish;

            AllergenModel treenuts = new("Tree Nuts");
            treenuts.AddObjectIds(new HashSet<string>{
                    "239", "607", "408"
                });
            ALLERGEN_DATA["treenuts"] = treenuts;

            AllergenModel dairy = new("Dairy");
            dairy.AddObjectIds(new HashSet<string>{
                    "195", "197", "199", "201", "206", "215", "232", "233", "236", "240", "243", "605",
                    "608", "727", "730", "904", "424", "426"
                });
            dairy.AddTags(new HashSet<string> { "milk_item", "large_milk_item", "cow_milk_item", "goat_milk_item" });
            ALLERGEN_DATA["dairy"] = dairy;

            AllergenModel mushroom = new("Mushrooms");
            mushroom.AddObjectIds(new HashSet<string>{
                    "404", "205", "606", "218", "420", "422", "281", "257", "773", "851"
                });
            ALLERGEN_DATA["mushroom"] = mushroom;
        }

        public static void ThrowIfAllergenDoesntExist(string allergen)
        {
            if (!ALLERGEN_DATA.ContainsKey(allergen))
            {
                throw new Exception("No allergen found with Id " + allergen.ToString());
            }
        }

        public static string GetAllergenContextTag(string allergen)
        {
            ThrowIfAllergenDoesntExist(allergen);
            return ModEntry.MOD_ID + "_allergen_" + allergen.ToLower();
        }

        public static string GetMadeWithContextTag(string allergen)
        {
            ThrowIfAllergenDoesntExist(allergen);
            return ModEntry.MOD_ID + "_made_with_id_" + allergen.ToLower();
        }

        public static string GetAllergenDisplayName(string allergen)
        {
            ThrowIfAllergenDoesntExist(allergen);
            return ALLERGEN_DATA[allergen].DisplayName;
        }

        public static ISet<string> GetObjectsWithAllergen(string allergen, IAssetDataForDictionary<string, ObjectData> data)
        {
            ThrowIfAllergenDoesntExist(allergen);

            // labeled items
            ISet<string> result = ALLERGEN_DATA[allergen].ObjectIds;

            // fish special case
            if (allergen == "fish")
            {
                ISet<string> fishItems = GetFishItems(data);
                result.UnionWith(fishItems);
            }

            ISet<string> items = GetItemsWithContextTags(ALLERGEN_DATA[allergen].ContextTags, data);
            result.UnionWith(items);

            return result;
        }

        public static bool ModDataSetContains(IHaveModData obj, string key, string item)
        {
            return ModDataSetGet(obj, key).Contains(item);
        }

        // items cannot contain commas
        public static bool ModDataSetAdd(IHaveModData obj, string key, string item)
        {
            if (ModDataSetContains(obj, key, item)) return false;  // don't add duplicates
            item = item.Replace(",", "");  // sanitize

            if (ModDataGet(obj, key, out string val) && val.Length > 0)
            {
                obj.modData[key] = val + "," + item;
            }
            else
            {
                obj.modData[key] = item;
            }
            return true;
        }

        public static ISet<string> ModDataSetGet(IHaveModData obj, string key)
        { 
            if (ModDataGet(obj, key, out string val) && val.Length > 0)
            {
                return val.Split(',').ToHashSet();
            }
            return new HashSet<string>();
        }

        public static bool ModDataGet(IHaveModData obj, string key, out string val)
        {
            if (obj.modData.TryGetValue(key, out string datastr))
            {
                val = datastr;
                return true;
            }
            val = "";
            return false;
        }

        public static bool FarmerIsAllergic(string allergen)
        {
            ThrowIfAllergenDoesntExist(allergen);
            return ModDataSetContains(Game1.player, FARMER_HAS_ALLERGIES_MODDATA_KEY, allergen);
        }

        public static bool FarmerIsAllergic (StardewValley.Object @object)
        {
            ISet<string> containsAllergens = GetAllergensInObject(@object);

            foreach (string a in containsAllergens)
            {
                if (FarmerIsAllergic(a))
                {
                    return true;
                }
            }
            return false;
        }

        public static ISet<string> GetAllergensInObject(StardewValley.Object? @object)
        {
            ISet<string> result = new HashSet<string>();
            if (@object == null)
            {
                return result;
            }

            // special case: preserves item
            List<StardewValley.Object> madeFrom = TryGetMadeFromObjects(@object);

            if (madeFrom.Count > 0)
            {
                foreach (StardewValley.Object madeFromObj in madeFrom)
                {
                    foreach (var tag in madeFromObj.GetContextTags())
                    {
                        if (tag.StartsWith(ModEntry.MOD_ID + "_allergen_"))
                        {
                            result.Add(tag.Split("_").Last());
                        }
                    }
                }
            }
            // special case: cooked item
            else if (@object.modData.ContainsKey("BarleyZP.BzpAllergies_CookedWith"))
            {
                
                // try looking in the modData field for what the thing was crafted with
                foreach (string allergen in ModDataSetGet(@object, "BarleyZP.BzpAllergies_CookedWith"))
                {
                    result.Add(allergen);
                }
            }
            // else: boring normal item
            else
            {
                foreach (var tag in @object.GetContextTags())
                {
                    if (tag.StartsWith(ModEntry.MOD_ID + "_allergen_"))
                    {
                        result.Add(tag.Split("_").Last());
                    }
                }
            }

            return result;
        }

        public static List<StardewValley.Object> TryGetMadeFromObjects(StardewValley.Object @object)
        {
            List<StardewValley.Object> result = new();

            // get context tags
            ISet<string> tags = @object.GetContextTags();

            // find the "preserve_sheet_index_{id}" tag
            Regex rx = new(@"^preserve_sheet_index_\d+$");
            List<string> filteredTags = tags.Where(t => rx.IsMatch(t)).ToList();

            if (filteredTags.Count == 0)  // no preserves index
            {
                return result;
            }

            foreach (string tag in filteredTags)
            {
                // get the id of the object it was made from
                Match m = Regex.Match(tag, @"\d+");
                if (m.Success)
                {
                    string madeFromId = m.Value;
                    if (ItemRegistry.Create(madeFromId) is StardewValley.Object casted)
                    {
                        result.Add(casted);
                    }
                }
            }
            return result;
        }

        public static List<string> RollRandomKAllergies(int k)
        {
            Random random = new();

            if (k == -1)
            {
                // generate k from binomial distribution with p = 0.5
                k = 1;
                
                int trials = ALLERGEN_DATA.Count - 1;
                for (int i = 0; i < trials; i++)
                {
                    if (random.NextDouble() < 0.5)
                    {
                        k++;
                    }
                }
            }

            // select k random allergens
            List<string> result = new();
            List<string> possibleAllergies = ALLERGEN_DATA.Keys.ToList();
            for (int i = 0; i < k; i++)
            {
                int idx = random.Next(possibleAllergies.Count);
                result.Add(possibleAllergies[idx]);
                possibleAllergies.RemoveAt(idx);
            }

            return result;
        }

        public static bool PlayerHasDiscoveredAllergy(string allergyId)
        {
            return ModDataSetContains(Game1.player, FARMER_DISCOVERED_ALLERGIES_MODDATA_KEY, allergyId);
        }

        public static bool DiscoverPlayerAllergy(string allergyId)
        {
            return ModDataSetAdd(Game1.player, FARMER_DISCOVERED_ALLERGIES_MODDATA_KEY, allergyId);
        }

        public static void TogglePlayerHasAllergy(string allergyId, bool has)
        {
            Monitor.Log(has.ToString(), LogLevel.Debug);
        }

        private static ISet<string> GetFishItems (IAssetDataForDictionary<string, ObjectData> data)
        {
            ISet<string> result = new HashSet<string>();

            foreach (var item in data.Data)
            {
                ObjectData v = item.Value;
                if (v.Category == StardewValley.Object.FishCategory)
                {
                    result.Add(item.Key);
                }
            }

            // remove shellfish
            ISet<string> shellfish = ALLERGEN_DATA["shellfish"].ObjectIds;
            
            foreach (var item in data.Data)
            {
                List<string> tags = item.Value.ContextTags ?? new();
                if (shellfish.Contains(item.Key) || tags.Contains(GetAllergenContextTag("shellfish")))
                {
                    result.Remove(item.Key);
                }
            }

            return result;
        }

        private static ISet<string> GetItemsWithContextTags (ISet<string> tags, IAssetDataForDictionary<string, ObjectData> data)
        {
            ISet<string> result = new HashSet<string>();

            foreach (var item in data.Data)
            {
                ObjectData v = item.Value;
                foreach (string tag in tags)
                {
                    if (v.ContextTags != null && v.ContextTags.Contains(tag))
                    {
                        result.Add(item.Key);
                    }
                }
            }

            return result;
        }
    }

    internal class AllergenModel
    {
        public ISet<string> ObjectIds { get; }
        public ISet<string> ContextTags {  get; }
        public string DisplayName { get; }
        public string? AddedByContentPack { get; }  // null if not added by any pack, otherwise unique Id of the pack

        public AllergenModel (string displayName, string? addedByContentPack = null)
        {
            ObjectIds = new HashSet<string>();
            ContextTags = new HashSet<string>();
            DisplayName = displayName;
            AddedByContentPack = addedByContentPack;
        }

        public void AddObjectIds (IEnumerable<string> ids)
        {
            foreach (string id in ids)
            {
                ObjectIds.Add(id);
            }
        }

        public void AddTags(IEnumerable<string> tags)
        {
            foreach (string tag in tags)
            {
                ContextTags.Add(tag);
            }
        }
    }
}
