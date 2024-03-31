namespace BZP_Allergies.Config
{
    internal class GenericAllergenConfig
    {
        public Dictionary<string, bool> Allergies;

        public GenericAllergenConfig()
        {
            Allergies = new();

            // get content pack configurations
            foreach (string id in AllergenManager.ALLERGEN_TO_DISPLAY_NAME.Keys)
            {
                Allergies.Add(id, false);
            }
        }
    }
}
