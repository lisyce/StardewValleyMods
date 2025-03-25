using StardewValley;
using StardewValley.Delegates;
using StardewValley.Objects;

namespace EnemyOfTheValley.Util;

public class GameStateQueries
{
    public static bool PlayerWearingHat(string[] query, GameStateQueryContext context)
    {
        if (!ArgUtility.TryGet(query, 1, out var playerKey, out var error, allowBlank: false) ||
            !ArgUtility.TryGet(query, 2, out var hatId, out error, allowBlank: false))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        
        return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, target => target.hat?.Value?.ItemId == hatId);
    }
    
    public static bool EotvPlayerNpcRelationship(string[] query, GameStateQueryContext context)
    {
        if (!ArgUtility.TryGet(query, 1, out var playerKey, out var error, allowBlank: false) || !ArgUtility.TryGet(query, 2, out var npcName, out error, allowBlank: false))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        
        var relationships = new string[query.Length - 3];
        var validRelationships = new HashSet<string>() { "enemy", "archenemy", "exarchenemy", "friendly", "roommate", "dating", "engaged", "married", "divorced" };
        for (int i = 3; i < query.Length && ArgUtility.TryGet(query, i, out var type, out error, allowBlank: false); i++)
        {
            relationships[i-3] = type.ToLower();
            if (!validRelationships.Contains(type.ToLower()))
            {
                return GameStateQuery.Helpers.ErrorResult(query, "unknown relationship type '" + type + "'; expected one of Friendly, Roommate, Dating, Engaged, Married, Divorced, Enemy, Archenemy, or ExArchenemy");
            }
        }
        
        if (relationships.Length == 0)
        {
            return GameStateQuery.Helpers.ErrorResult(query, "missing required relationship status argument");
        }
 
        
        bool anyNpc = string.Equals(npcName, "Any", StringComparison.OrdinalIgnoreCase);
        return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
        {
            if (anyNpc)
            {
                foreach (var entry in target.friendshipData.Pairs)
                {
                    if (IsMatch(entry.Value, target, entry.Key, relationships)) return true;
                }
            }
            else if (target.friendshipData.TryGetValue(npcName, out var friendship))
            {
                if (IsMatch(friendship, target, npcName, relationships)) return true;
            }
            return false;
        });

        bool IsMatch(Friendship friendship, Farmer target, string npcName, string[] relationships)
        {
            foreach (var relationship in relationships)
            {
                if (relationship == "married")
                {
                    if (Relationships.IsRelationship(npcName, FriendshipStatus.Married, target) &&
                        !friendship.RoommateMarriage) return true;
                }
                else if (relationship == "divorced")
                {
                    if (Relationships.IsRelationship(npcName, FriendshipStatus.Divorced, target) &&
                        !friendship.RoommateMarriage) return true;
                }
                else if (relationship == "roommate")
                {
                    if (Relationships.IsRelationship(npcName, FriendshipStatus.Married, target) &&
                        friendship.RoommateMarriage) return true;
                }
                else if (Relationships.TryConvertToFriendshipStatus(relationship, out var status))
                {
                    if (Relationships.IsRelationship(npcName, status, target)) return true;
                }
                else
                {
                    return GameStateQuery.Helpers.ErrorResult(query, "unhandled relationship type '" + relationship + "'");
                }
            }

            return false;
        }
    }
}