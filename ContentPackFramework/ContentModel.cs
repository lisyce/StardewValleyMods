

namespace BZP_Allergies.ContentPackFramework
{
    internal class ContentModel
    {
        public string? Format { get; set; }
        public List<CustomAllergen> CustomAllergens { get; set; } = new();
        public List<AllergenAssignments> AllergenAssignments { get; set; } = new();
    }

    internal class CustomAllergen
    {
        // display name
        public string? Name { get; set; }
    }

    internal class AllergenAssignments
    {
        public string? AllergenName { get; set; }

        // any object with this Id has the allergen
        public List<string> ObjectIds { get; set; } = new();

        // any object with this context tag has the allergen
        public List<string> ContextTags { get; set; } = new();
    }
}
