using BZP_Allergies.Config;

namespace BZP_Allergies
{
    internal class AllergenManager
    {
        public enum Allergens {
            EGG,
            WHEAT,
            FISH,
            SHELLFISH,
            TREE_NUTS,
            DAIRY
        }

        private static readonly Dictionary<Allergens, ISet<string>> ENUM_TO_ALLERGEN_OBJECTS = new()
        {
            { Allergens.EGG, new HashSet<string>{
                "194", "195", "201", "203", "211", "213", "220", "221", "223", "234", "240", "648",
                "732"
            }},
            { Allergens.WHEAT, new HashSet<string>{
                "198", "201", "202", "203", "206", "211", "214", "216", "220", "221", "222", "223",
                "224", "234", "239", "241", "604", "608", "611", "618", "651", "731", "732", "921"
            }},
            { Allergens.FISH, new HashSet<string>{
                "198", "202", "204", "212", "213", "214", "219", "225", "226", "227", "228", "242",
                "265", "921"
            }},
            { Allergens.SHELLFISH, new HashSet<string>{
                "203", "218", "227", "228", "727", "728", "729", "730", "732", "733"
            }},
            { Allergens.TREE_NUTS, new HashSet<string>{
                "239", "607"
            }},
            { Allergens.DAIRY, new HashSet<string>{
                "195", "197", "199", "201", "206", "215", "232", "233", "236", "240", "243", "605",
                "608", "727", "730", "904"
            }}
        };

        private static readonly Dictionary<Allergens, string> ENUM_TO_CONTEXT_TAG = new()
        {
            { Allergens.EGG, "bzp_allergies_egg" },
            { Allergens.WHEAT, "bzp_allergies_wheat" },
            { Allergens.FISH, "bzp_allergies_fish" },
            { Allergens.SHELLFISH, "bzp_allergies_shellfish" },
            { Allergens.TREE_NUTS, "bzp_allergies_treenuts" },
            { Allergens.DAIRY, "bzp_allergies_dairy" }
        };

        private static readonly Dictionary<Allergens, string> ENUM_TO_STRING = new()
        {
            { Allergens.EGG, "Eggs" },
            { Allergens.WHEAT, "Wheat" },
            { Allergens.FISH, "Fish" },
            { Allergens.SHELLFISH, "Shellfish" },
            { Allergens.TREE_NUTS, "Tree Nuts" },
            { Allergens.DAIRY, "Dairy" }
        };

        public static string GetAllergenContextTag(Allergens allergen)
        {
            string result = ENUM_TO_CONTEXT_TAG.GetValueOrDefault(allergen, "");
            if (result.Equals(""))
            {
                throw new Exception("No context tags were defined for the allergen " + allergen.ToString());
            }
            return result;
        }

        public static string GetAllergenReadableString(Allergens allergen)
        {
            string result = ENUM_TO_STRING.GetValueOrDefault(allergen, "");
            if (result.Equals(""))
            {
                throw new Exception("No readable string was defined for the allergen " + allergen.ToString());
            }
            return result;
        }

        public static ISet<string> GetObjectsWithAllergen(Allergens allergen)
        {
            ISet<string> result = ENUM_TO_ALLERGEN_OBJECTS.GetValueOrDefault(allergen, new HashSet<string>());
            if (result.Count() == 0)
            {
                throw new Exception("No objects have been assigned the allergen " + allergen.ToString());
            }
            return result;
        }
        public static bool FarmerIsAllergic(Allergens allergen, ModConfig config)
        {
            switch (allergen)
            {
                case Allergens.EGG:
                    return config.Farmer.EggAllergy;
                case Allergens.WHEAT:
                    return config.Farmer.WheatAllergy;
                case Allergens.FISH:
                    return config.Farmer.FishAllergy;
                case Allergens.SHELLFISH:
                    return config.Farmer.ShellfishAllergy;
                case Allergens.TREE_NUTS:
                    return config.Farmer.TreenutAllergy;
                case Allergens.DAIRY:
                    return config.Farmer.DairyAllergy;
                default:
                    return false;
            }
        }

        public static bool FarmerIsAllergic (StardewValley.Object @object, ModConfig config)
        {
            // check each of the allergens
            foreach (Allergens a in Enum.GetValues<Allergens>())
            {
                if (@object.HasContextTag(GetAllergenContextTag(a)) && FarmerIsAllergic(a, config))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
