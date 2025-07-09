using StardewValley.GameData;

namespace MushroomLogFramework;

public class MushroomLogData
{
    public enum TreeType
    {
        Wild,
        Fruit
    }

    public class TreeOutputItem : GenericSpawnItemDataWithCondition
    {
        public int Precedence { get; set; } = 0;
        public float Chance { get; set; } = 1;
        public List<QuantityModifier> ChanceModifiers { get; set; } = new();
    }
    
    public class TreeProduce
    {
        public string Id { get; set; }
        public TreeType Type { get; set; } = TreeType.Wild;
        public string TreeId { get; set; }
        public List<TreeOutputItem> Outputs { get; set; }
        public string Condition { get; set; }
    }
    
    public List<TreeOutputItem> DefaultTreeOutputs { get; set; }
    public List<TreeProduce> SpecificTreeOutputs { get; set; }
}