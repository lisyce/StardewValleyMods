using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StardewValley.Menus.SocialPage;

namespace EnemyOfTheValley.Common
{
    internal class Relationships
    {
        public static FriendshipStatus Enemy = (FriendshipStatus)100;
        public static FriendshipStatus ArchEnemy = (FriendshipStatus)101;
        public static bool IsEnemy(SocialEntry entry)
        {
            if (entry is null || entry.Friendship is null) return false;
            return entry.Friendship.Status == Enemy;
        }

        public static bool IsArchEnemy(SocialEntry entry)
        {
            if (entry is null || entry.Friendship is null) return false;
            return entry.Friendship.Status == ArchEnemy;
        }
    }
}
