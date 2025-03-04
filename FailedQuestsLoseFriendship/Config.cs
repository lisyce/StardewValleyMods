using StardewValley.Extensions;
using StardewValley.GameData.SpecialOrders;

namespace FailedQuestsLoseFriendship;

public class Config
{
    public int FriendshipLost { get; set; } = 20;
    
    public bool SpecialOrdersEnabled { get; set; } = true;
    public HashSet<string> DisabledSpecialOrders { get; init; } = new();
    
    public bool ResourceCollectionQuestsEnabled { get; set; } = true;
    public bool SlayMonsterQuestsEnabled { get; set; } = true;
    public bool FishingQuestsEnabled { get; set; } = true;
    public bool ItemDeliveryQuestsEnabled { get; set; } = true;
    public bool SocializeQuestsEnabled { get; set; } = true;
}