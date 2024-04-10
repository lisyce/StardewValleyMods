﻿

namespace BZP_Allergies.ContentPackFramework
{
    internal class ContentModel
    {
        public string? Format { get; set; }
        public Dictionary<string, CustomAllergen> CustomAllergens { get; set; } = new();
        public Dictionary<string, AllergenAssignments> AllergenAssignments { get; set; } = new();
    }

    internal class CustomAllergen
    {
        // display name
        public string? Name { get; set; }
    }

    internal class AllergenAssignments
    {
        // any object with this unqualified Id has the allergen
        public List<string> ObjectIds { get; set; } = new();

        // any object with this context tag has the allergen
        public List<string> ContextTags { get; set; } = new();
    }
}