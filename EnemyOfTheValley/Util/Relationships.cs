using StardewValley;
using static StardewValley.Menus.SocialPage;

namespace EnemyOfTheValley.Util
{
    internal class Relationships
    {
        public static FriendshipStatus Enemy = (FriendshipStatus)(-1);
        public static FriendshipStatus Archenemy = (FriendshipStatus)(-2);
        public static FriendshipStatus ExArchenemy = (FriendshipStatus)(-3);

        public static void SetRelationship(string npcName, Farmer who, FriendshipStatus status, bool printValidation = false)
        {
            if (printValidation)
            {
                if (!who.friendshipData.ContainsKey(npcName))
                {   
                    Console.WriteLine("No NPC with the name " +  npcName + " found. Remember this is case-sensitive!");
                    return;
                }
            }
            var npc = Game1.getCharacterFromName(npcName);
            if (npc == null) return;
            
            if (status == Enemy || status == Archenemy || status == ExArchenemy)
            {
                who.modData["BarleyZP.EnemyOfTheValley.FriendshipStatus_" + npcName] = ((int)status).ToString();
                switch (status)
                {
                    case (FriendshipStatus)(-1):  // a bit hacky, but need a constant value here
                        who.mailReceived.Add("BarleyZP.EnemyOfTheValley.BeenEnemies_" + npcName);
                        break;
                    case (FriendshipStatus)(-2):
                        who.mailReceived.Add("BarleyZP.EnemyOfTheValley.BeenArchenemies_" + npcName);
                        break;
                    case (FriendshipStatus)(-3):
                        who.mailReceived.Add("BarleyZP.EnemyOfTheValley.BeenExArchenemies_" + npcName);
                        break;
                }
            }
            else
            {
                who.modData["BarleyZP.EnemyOfTheValley.FriendshipStatus_" + npcName] = "";
                who.friendshipData[npcName].Status = status;
            }
        }

        public static bool IsRelationship(SocialEntry? entry, FriendshipStatus status, Farmer who)
        {
            return entry?.Friendship is not null && IsRelationship(entry.InternalName, status, who);
        }
        
        public static bool TryConvertToFriendshipStatus(string str, out FriendshipStatus status)
        {
            status = str.ToLower() switch
            {
                "enemy" => Enemy,
                "archenemy" => Archenemy,
                "exarchenemy" => ExArchenemy,
                "friendly" => FriendshipStatus.Friendly,
                "dating" => FriendshipStatus.Dating,
                "engaged" => FriendshipStatus.Engaged,
                "divorced" => FriendshipStatus.Divorced,
                "married" => FriendshipStatus.Married,
                _ => (FriendshipStatus) int.MinValue // default value since we cannot use null
            };
            
            return status != (FriendshipStatus) int.MinValue;
        }

        public static bool IsRelationship(string npcName, FriendshipStatus status, Farmer who)
        {
            var npc = Game1.getCharacterFromName(npcName);
            if (npc == null) return false;
            var savedStatus = who.modData.GetValueOrDefault("BarleyZP.EnemyOfTheValley.FriendshipStatus_" + npcName);
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
        
        public static bool HasAnExArchenemy(Farmer who)
        {
            foreach (var name in who.friendshipData.Keys)
            {
                if (IsRelationship(name, ExArchenemy, who))
                {
                    return true;
                }
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
        
        public static void WipeExArchenemyMemories()
        {
            Farmer who = Game1.player;
            
            foreach (string npcName in who.friendshipData.Keys)
            {
                var friendship = who.friendshipData[npcName];
                if (IsRelationship(npcName, ExArchenemy, who))
                {
                    friendship.Clear();
                    var i = Game1.getCharacterFromName(npcName);
                    if (i == null) continue;
                    
                    who.modData["BarleyZP.EnemyOfTheValley.FriendshipStatus_" + npcName] = "";
                    i.CurrentDialogue.Clear();
                    i.CurrentDialogue.Push(i.TryGetDialogue("WipedMemory") ?? new Dialogue(i, "Strings\\Characters:WipedMemory"));
                    Game1.stats.Increment("exArchenemyMemoriesWiped");
                }
            }
        }
    }
}
