using HarmonyLib;
using StardewValley;
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
        }

        public static IEnumerable<CodeInstruction> changeFriendship_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new(instructions);

            MethodInfo getPoints = AccessTools.PropertyGetter(typeof(Friendship), nameof(Friendship.Points));

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
