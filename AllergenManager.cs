using Netcode;

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
            { Allergens.EGG, new HashSet<string>{ "194", "195" } },
            { Allergens.WHEAT, new HashSet<string>{} },
            { Allergens.FISH, new HashSet<string>{} },
            { Allergens.SHELLFISH, new HashSet<string>{} },
            { Allergens.TREE_NUTS, new HashSet<string>{} },
            { Allergens.DAIRY, new HashSet<string>{} }
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

        public static string GetAllergenContextTag(Allergens allergen)
        {
            string result = ENUM_TO_CONTEXT_TAG.GetValueOrDefault(allergen, "");
            if (result.Equals(""))
            {
                throw new Exception("No context tags were defined for the allergen " + allergen.ToString());
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
    }
}
