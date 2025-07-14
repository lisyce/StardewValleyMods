using StardewValley.GameData;

namespace MushroomLogFramework;

public class MushroomLogData
{
    public enum TreeType
    {
        Wild,
        Fruit
    }

    public class TreeOutput : GenericSpawnItemDataWithCondition
    {
        public int Precedence { get; set; } = 0;
        public float Chance { get; set; } = 1;
        public List<QuantityModifier> ChanceModifiers { get; set; } = new();
        public QuantityModifier.QuantityModifierMode ChanceModifierMode { get; set; } = QuantityModifier.QuantityModifierMode.Stack;
        public bool AllowQualityModifications { get; set; } = true;
        public bool AllowQuantityModifications { get; set; } = true;
        public bool DisableDefaultOutputPossibilities { get; set; } = false;
    }
    
    public class TreeRule
    {
        public string Id { get; set; }
        public TreeType Type { get; set; } = TreeType.Wild;
        public string TreeId { get; set; }
        public List<TreeOutput> Outputs { get; set; } = new();
        public string Condition { get; set; }
    }

    public List<TreeOutput> DefaultTreeOutputs { get; set; } = new();
    public List<TreeRule> SpecificTreeOutputs { get; set; } = new();
}