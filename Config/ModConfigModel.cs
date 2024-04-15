namespace BZP_Allergies.Config
{
    internal sealed class ModConfigModel
    {
        public GenericAllergenConfig Farmer { get; set; } = new();
        public bool RandomizeAllergies { get; set; } = false;
        public int RandomAllergenCount { get; set; } = -1;  // -1 implies the game picks a random number
        public bool HintBeforeEating { get; set; } = true;
    }
}
