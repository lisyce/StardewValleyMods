using StardewValley.GameData.Objects;
using StardewModdingAPI;
using System.Text.RegularExpressions;

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

        private static readonly Dictionary<string, ISet<string>> ENUM_TO_ALLERGEN_OBJECTS = new()
        {
            { "egg", new HashSet<string>{
                "194", "195", "201", "203", "211", "213", "220", "221", "223", "234", "240", "648",
                "732"
            }},
            { "wheat", new HashSet<string>{
                "198", "201", "202", "203", "206", "211", "214", "216", "220", "221", "222", "223",
                "224", "234", "239", "241", "604", "608", "611", "618", "651", "731", "732", "246",
                "262"
            }},
            { "fish", new HashSet<string>{
                "198", "202", "204", "212", "213", "214", "219", "225", "226", "227", "228", "242",
                "265", "447", "445", "812", "SmokedFish"
            }},
            { "shellfish", new HashSet<string>{
                "203", "218", "227", "228", "727", "728", "729", "730", "732", "733", "447", "812",
                "SmokedFish", "715", "372", "717", "718", "719", "720", "723", "716", "721", "722"
            }},
            { "treenuts", new HashSet<string>{
                "239", "607", "408"
            }},
            { "dairy", new HashSet<string>{
                "195", "197", "199", "201", "206", "215", "232", "233", "236", "240", "243", "605",
                "608", "727", "730", "904", "424", "426"
            }}
        };

        public static readonly Dictionary<string, string> ALLERGENS_TO_DISPLAY_NAME = new()
        {
            { "egg", "Eggs" },
            { "wheat", "Wheat" },
            { "fish", "Fish" },
            { "shellfish", "Shellfish" },
            { "treenuts", "Tree Nuts" },
            { "dairy", "Dairy" }
        };

        public static string GetAllergenContextTag(string allergen)
        {
            return ModEntry.MOD_ID + "_" + allergen.ToLower();
        }

        public static string GetAllergenDisplayName(string allergen)
        {
            string result = ALLERGENS_TO_DISPLAY_NAME.GetValueOrDefault(allergen, "");
            if (result.Equals(""))
            {
                throw new Exception("No readable string was defined for the allergen " + allergen.ToString());
            }
            return result;
        }

        public static ISet<string> GetObjectsWithAllergen(string allergen, IAssetDataForDictionary<string, ObjectData> data)
        {
            // labeled items
            ISet<string> result = ENUM_TO_ALLERGEN_OBJECTS.GetValueOrDefault(allergen, new HashSet<string>());

            // category items
            if (allergen == "egg")
            {
                ISet<string> rawEggItems = GetItemsWithContextTags(new List<string> { "egg_item", "mayo_item", "large_egg_item" }, data);
                result.UnionWith(rawEggItems);
            }
            else if (allergen == "fish")
            {
                ISet<string> fishItems = GetFishItems(data);
                result.UnionWith(fishItems);
            }
            else if (allergen == "dairy")
            {
                ISet<string> dairyItems = GetItemsWithContextTags(new List<string> { "milk_item", "large_milk_item", "cow_milk_item", "goat_milk_item" }, data);
                result.UnionWith(dairyItems);
            }

            if (result.Count == 0)
            {
                throw new Exception("No objects have been assigned the allergen " + allergen.ToString());
            }
            return result;

            
        }
        public static bool FarmerIsAllergic(string allergen)
        {
            return allergen switch
            {
                "egg" => Config.Farmer.EggAllergy,
                "wheat" => Config.Farmer.WheatAllergy,
                "fish" => Config.Farmer.FishAllergy,
                "shellfish" => Config.Farmer.ShellfishAllergy,
                "treenuts" => Config.Farmer.TreenutAllergy,
                "dairy" => Config.Farmer.DairyAllergy,
                _ => false,
            };
        }

        public static bool FarmerIsAllergic (StardewValley.Object @object)
        {
            // special case: roe, aged roe, or smoked fish
            // need to differentiate fish vs shellfish ingredient
            // TODO: 
            // 1. check if there are multiple allergen context tags
            // 2. if so, check if we have preserve_sheet_index context tag
            // 3. if so, use the preserve index to figure out what it's made of
            // 4. if no preserves tag, just see if we're allergic to any of the multiple tags
            List<string> fishShellfishDifferentiation = new() { "(O)447", "(O)812", "(O)SmokedFish" };
            if (fishShellfishDifferentiation.Contains(@object.QualifiedItemId))
            {
                try
                {
                    // get context tags
                    ISet<string> tags = @object.GetContextTags();

                    // find the "preserve_sheet_index_{id}" tag
                    Regex rx = new(@"^preserve_sheet_index_\d+$");
                    List<string> filtered_tags = tags.Where(t => rx.IsMatch(t)).ToList();
                    string preserve_sheet_tag = filtered_tags[0];

                    // get the id of the object it was made from
                    Match m = Regex.Match(preserve_sheet_tag, @"\d+");
                    if (!m.Success)
                    {
                        throw new Exception("No regex match for item id in preserve_sheet_index context tag");
                    }

                    string madeFromId = m.Value;
                    // load Data/Objects for context tags
                    IDictionary<string, ObjectData> objData = GameContent.Load<Dictionary<string, ObjectData>>("Data/Objects");

                    // !isShellfish = isFish since these can only be made from one of the two
                    bool isShellfish = objData[madeFromId].ContextTags.Contains(GetAllergenContextTag("shellfish"));

                    if (isShellfish && FarmerIsAllergic("shellfish"))
                    {
                        return true;
                    }
                    else
                    {
                        return !isShellfish && FarmerIsAllergic("fish");
                    }
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Failed in {nameof(FarmerIsAllergic)}:\n{ex}", LogLevel.Error);
                    Monitor.Log("Unable to determine whether eaten Object was fish or shellfish");
                    // we failed to determine, so let's just fall through and
                    // return whether the farmer is allergic to fish or shellfish
                }
            }

            // check each of the allergens
            foreach (string a in ALLERGENS_TO_DISPLAY_NAME.Keys)
            {
                if (@object.HasContextTag(GetAllergenContextTag(a)) && FarmerIsAllergic(a))
                {
                    return true;
                }
            }

            return false;
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
            ISet<string> shellfish = ENUM_TO_ALLERGEN_OBJECTS.GetValueOrDefault("shellfish", new HashSet<string>());
            
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

        private static ISet<string> GetItemsWithContextTags (List<string> tags, IAssetDataForDictionary<string, ObjectData> data)
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
}
