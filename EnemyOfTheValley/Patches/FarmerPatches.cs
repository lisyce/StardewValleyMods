using EnemyOfTheValley.Common;
using Force.DeepCloner;
using HarmonyLib;
using Netcode;
using StardewValley;
using StardewValley.Network;
using System.Reflection;
using System.Reflection.Emit;

namespace EnemyOfTheValley.Patches
{
    internal class FarmerPatches
    {
        public static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), "changeFriendship"),
                transpiler: new HarmonyMethod(typeof(FarmerPatches), nameof(changeFriendship_Transpiler))
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

                    if (Relationships.IsRelationship(__instance.friendshipData[name], Relationships.Archenemy))
                    {
                        __instance.changeFriendship(20, npc);
                    }
                    else if (Relationships.IsRelationship(__instance.friendshipData[name], Relationships.Enemy) && __instance.friendshipData[name].Points > -2500)
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

        public static IEnumerable<CodeInstruction> changeFriendship_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new(instructions);

            MethodInfo getPoints = AccessTools.PropertyGetter(typeof(Friendship), nameof(Friendship.Points));
            MethodInfo min = AccessTools.Method(typeof(Math), nameof(Math.Min), new Type[] { typeof(int), typeof(int) });
            MethodInfo max = AccessTools.Method(typeof(Math), nameof(Math.Max), new Type[] { typeof(int), typeof(int) });

            matcher.MatchStartForward(
                new CodeMatch(OpCodes.Call, min),
                new CodeMatch(OpCodes.Call, max))
                .ThrowIfNotMatch("failed to find min and max calls")
                .Advance(-13)
                .Set(OpCodes.Ldc_I4, -2500);

            matcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Callvirt, getPoints),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Bge_S))
                .ThrowIfNotMatch("failed to find where it's checked that friendship is < 0")
                .Advance(2)
                .Set(OpCodes.Ldc_I4, -2500)
                .MatchStartForward(
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Ldc_I4_0))
                .Advance(1)
                .Set(OpCodes.Ldc_I4, -2500);

            return matcher.InstructionEnumeration();
        }
    }
}
