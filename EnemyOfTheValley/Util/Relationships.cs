using StardewValley;
using static StardewValley.Menus.SocialPage;

namespace EnemyOfTheValley.Util
{
    internal class Relationships
    {
        public static FriendshipStatus Enemy = (FriendshipStatus)(-1);
        public static FriendshipStatus Archenemy = (FriendshipStatus)(-2);
        public static FriendshipStatus ExArchenemy = (FriendshipStatus)(-3);

        public static void SetRelationship(string name, FriendshipStatus status, bool printValidation = false)
        {
            if (printValidation)
            {
                if (!Game1.player.friendshipData.ContainsKey(name))
                {
                    Console.WriteLine("No NPC with the name " +  name + " found. Remember this is case-sensitive!");
                    return;
                }
            }
            var npc = Game1.getCharacterFromName(name);
            if (npc == null) return;
            
            if (status == Enemy || status == Archenemy || status == ExArchenemy)
            {
                npc.modData["BarleyZP.EnemyOfTheValley.FriendshipStatus"] = ((int)status).ToString();
            }
            else
            {
                npc.modData["BarleyZP.EnemyOfTheValley.FriendshipStatus"] = "";
                Game1.player.friendshipData[name].Status = status;
            }
        }

        public static bool IsRelationship(SocialEntry? entry, FriendshipStatus status, Farmer who)
        {
            return entry?.Friendship is not null && IsRelationship(entry.InternalName, status, who);
        }

        public static bool IsRelationship(string npcName, FriendshipStatus status, Farmer who)
        {
            var npc = Game1.getCharacterFromName(npcName);
            if (npc == null) return false;
            var savedStatus = npc.modData.GetValueOrDefault("BarleyZP.EnemyOfTheValley.FriendshipStatus");
            if (savedStatus != null)
            {
                return savedStatus == ((int)status).ToString();
            }

            return who.friendshipData.GetValueOrDefault(npcName)?.Status == status;
        }

        /// <summary>
        /// Returns whether the specified farmer has an enemy with at least <c>heartLevel</c> negative hearts.
        /// </summary>
        public static bool HasAnEnemyWithHeartLevel(Farmer who, int heartLevel)
        {
            foreach (string name in who.friendshipData.Keys)
            {
                var friendship = who.friendshipData[name];
                if (IsRelationship(name, Enemy, who) && friendship.Points <= 250 * heartLevel) return true;
            }
            return false;
        }
        
        /// <param name="who">The farmer to check relationships on</param>
        /// <returns>Tuples of (display name, internal name) for each NPC the player is enemies with.</returns>
        public static List<(string, string)> Enemies(Farmer who)
        {
            var result = new List<(string, string)>();
            foreach (var name in who.friendshipData.Keys)
            {
                if (IsRelationship(name, Enemy, who))
                {
                    var npc = Game1.getCharacterFromName(name);
                    if (npc != null)
                    {
                        result.Add((NPC.GetDisplayName(npc.Name), npc.Name));
                    }
                }
            }
            
            return result;
        }
    }
}
