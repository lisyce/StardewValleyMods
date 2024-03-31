namespace BZP_Allergies.Config
{
    internal class GenericAllergenConfig
    {
        public Dictionary<string, bool> allergies;

        public GenericAllergenConfig()
        {
            allergies = new();

            // get content pack configurations
            foreach (string id in AllergenManager.ALLERGEN_TO_DISPLAY_NAME.Keys)
            {
                allergies.Add(id, false);
            }
        }
    }
}
