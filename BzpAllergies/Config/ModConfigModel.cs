namespace BZP_Allergies.Config
{
    internal sealed class ModConfigModel
    {
        public bool HintBeforeEating { get; set; } = true;
        public int NumberRandomAllergies { get; set; } = -1;
        public bool AllergenCountHint { get; set; } = true;

        public bool HoldingReaction { get; set; } = false;
        public bool CookingReaction { get; set; } = false;
        
        public bool EnableNausea { get; set; } = true;
        public int EatingDebuffLengthSeconds { get; set; } = 120;
        public float DebuffSeverityMultiplier { get; set; } = 1;
    }
}
