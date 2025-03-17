using StardewValley;
using StardewValley.Delegates;

namespace EnemyOfTheValley.Util;

public class GameStateQueries
{
    public static bool PlayerNpcEnemy(string[] query, GameStateQueryContext context)
    {
        return RelationshipQueryHelper(query, context, Relationships.Enemy);
    }

    public static bool PlayerNpcArchenemy(string[] query, GameStateQueryContext context)
    {
        return RelationshipQueryHelper(query, context, Relationships.Archenemy);
    }

    public static bool PlayerNpcExArchenemy(string[] query, GameStateQueryContext context)
    {
        return RelationshipQueryHelper(query, context, Relationships.ExArchenemy);
    }

    private static bool RelationshipQueryHelper(string[] query, GameStateQueryContext context, FriendshipStatus status)
    {
        if (!ArgUtility.TryGet(query, 1, out var playerKey, out var error, allowBlank: false) || !ArgUtility.TryGet(query, 2, out var npcName, out error, allowBlank: false))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        
        bool anyNpc = string.Equals(npcName, "Any", StringComparison.OrdinalIgnoreCase);
        return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
        {
            if (anyNpc)
            {
                foreach (string name in target.friendshipData.Keys)
                {
                    if (Relationships.IsRelationship(name, status, target))
                    {
                        return true;
                    }
                }
            }
            else if (target.friendshipData.ContainsKey(npcName) && Relationships.IsRelationship(npcName, status, target))
            {
                return true;
            }
            return false;
        });
    }
}