using EnemyOfTheValley.Util;
using Force.DeepCloner;
using HarmonyLib;
using Netcode;
using StardewValley;
using StardewValley.Network;
using System.Reflection;
using System.Reflection.Emit;
using StardewModdingAPI;
using StardewValley.Characters;

namespace EnemyOfTheValley.Patches
{
    internal class FarmerPatches
    {
        public static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), "changeFriendship"),
                prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(changeFriendship_Prefix))
                );
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.resetFriendshipsForNewDay)),
                prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(resetFriendshipForNewDay_Prefix)),
                postfix: new HarmonyMethod(typeof(FarmerPatches), nameof(resetFriendshipForNewDay_Postfix))
                );
        }

        public static void resetFriendshipForNewDay_Prefix(Farmer __instance, out Dictionary<string, (int Points, bool TalkedToToday)> __state)
        {
            __state = new();
            foreach (string name in __instance.friendshipData.Keys)
            {
                __state[name] = (__instance.friendshipData[name].Points, __instance.friendshipData[name].TalkedToToday);
            }
        }

        public static void resetFriendshipForNewDay_Postfix(ref Farmer __instance, Dictionary<string, (int Points, bool TalkedToToday)> __state)
        {
            foreach (string name in __instance.friendshipData.Keys)
            {
                NPC npc = Game1.getCharacterFromName(name);
                if (npc == null) continue;

                int before = __state[name].Points;
                if (before > 0)
                {
                    __instance.friendshipData[name].Points = Math.Max(__instance.friendshipData[name].Points, 0);
                }
                else if (before == 0)
                {
                    __instance.friendshipData[name].Points = 0;
                }
                else
                {
                    __instance.friendshipData[name].Points = before;
                    if (__state[name].TalkedToToday) continue;

                    if (Relationships.IsRelationship(name, Relationships.Archenemy, __instance))
                    {
                        __instance.changeFriendship(20, npc);
                    }
                    else if (Relationships.IsRelationship(name, Relationships.Enemy, __instance) && __instance.friendshipData[name].Points > -2500)
                    {
                        __instance.changeFriendship(10, npc);
                    }
                    else if (__instance.friendshipData[name].Points > -2000)
                    {
                        __instance.changeFriendship(2, npc);
                    }

                    __instance.friendshipData[name].Points = Math.Min(__instance.friendshipData[name].Points, 0);
                }
                
            }
        }

        public static bool changeFriendship_Prefix(ref Farmer __instance, int amount, NPC? n)
        {
            if (n == null || (!(n is Child) && !n.IsVillager) || !CustomFields.CanHaveNegativeFriendship(n))
            {
                return true;  // run original
            }

            if (n.SpeaksDwarvish() && !__instance.canUnderstandDwarves)
            {
                return false;  // do nothing, skip original
            }
            
            if (!__instance.friendshipData.TryGetValue(n.Name, out var friendship) || friendship.Points + amount >= 0) return true; // run original
            
            var maxNegativePoints = -1 * ((Utility.GetMaximumHeartsForCharacter(n) + 1) * 250 - 1);
            friendship.Points = Math.Min(0, Math.Max(friendship.Points + amount, maxNegativePoints));
            
            if (friendship.Points <= -2000 && !__instance.hasOrWillReceiveMail("BarleyZP.EnemyOfTheValley.EnemyCake"))
            {
                Game1.addMailForTomorrow("BarleyZP.EnemyOfTheValley.EnemyCake");
            }

            if (friendship.Points <= -3125 && Relationships.IsRelationship(n.Name, Relationships.Archenemy, __instance) && !__instance.hasOrWillReceiveMail("CF_Spouse"))
            {
                // stardrop mail
                Game1.addMailForTomorrow("BarleyZP.EnemyOfTheValley.StardropMail");
            }

            return false;

        }
    }
}
