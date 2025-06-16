using StardewValley;

namespace MultiplayerPerfectionCollections;

public class Util
{
    public int GetTotalShipped(string shippedId)
    {
        return Game1.getAllFarmers().Sum(farmer => farmer.basicShipped.GetValueOrDefault(shippedId, 0));
    }

    public bool ShouldDrawInShippingCollection(string shippedId)
    {
        return GetTotalShipped(shippedId) > 0;
    }
}