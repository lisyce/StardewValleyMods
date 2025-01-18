using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnemyOfTheValley
{
    internal class Preconditions
    {
        // Player has at least as many negative friendship points with an NPC
        public static bool NegativeFriendship(GameLocation location, string eventId, string[] args)
        {
            for (int i = 1; i < args.Length; i += 2)
            {
                if (!ArgUtility.TryGet(args, i, out var npcName, out var error, allowBlank: false) || !ArgUtility.TryGetInt(args, i + 1, out var minPoints, out error))
                {
                    return Event.LogPreconditionError(location, eventId, args, error);
                }
                if (!Game1.player.friendshipData.TryGetValue(npcName, out var friendship) || friendship.Points > minPoints)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
