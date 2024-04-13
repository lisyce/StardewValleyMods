namespace BZP_Allergies.Config
{
    internal sealed class ModConfig
    {
        public GenericAllergenConfig Farmer { get; set; } = new();
        public int? RandomAllergenCount { get; set; } = null;
        public bool HintBeforeEating { get; set; } = true;
    }
}
