using StardewValley;

namespace EnemyOfTheValley.Util;

public class CustomFields
{
    public static bool CanHaveNegativeFriendship(NPC npc)
    {
        return npc.GetData()?.CustomFields
            ?.ContainsKey("BarleyZP.EnemyOfTheValley.CanHaveNegativeFriendship") ?? false;
    }
    
    public static bool CanBecomeEnemies(NPC npc)
    {
        return npc.GetData()?.CustomFields
            ?.ContainsKey("BarleyZP.EnemyOfTheValley.CanBecomeEnemies") ?? false;
    }
}