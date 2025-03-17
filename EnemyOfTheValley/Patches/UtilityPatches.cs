using System.Reflection.Emit;
using EnemyOfTheValley.Common;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace EnemyOfTheValley.Patches;

public class UtilityPatches
{
    public static void Patch(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.Method(typeof(Utility), nameof(Utility.GetMaximumHeartsForCharacter)),
            prefix: new HarmonyMethod(typeof(UtilityPatches), nameof(GetMaximumHeartsForCharacter_Prefix))
        );
        
        harmony.Patch(
            original: AccessTools.Method(typeof(Utility), nameof(Utility.getMaxedFriendshipPercent)),
            transpiler: new HarmonyMethod(typeof(UtilityPatches), nameof(getMaxedFriendshipPercent_transpiler))
        );
    }
    
    public static bool GetMaximumHeartsForCharacter_Prefix(Character? character, ref int __result)
    {
        if (character == null || !Game1.player.friendshipData.TryGetValue(character.Name, out var friendship) || friendship.Points >= 0) return true;

        if (Relationships.IsRelationship(character.Name, Relationships.Archenemy, Game1.player)) {
            __result = 14;
            return false;
        }
        else if (Relationships.IsRelationship(character.Name, Relationships.Enemy, Game1.player))
        {
            __result = 10;
            return false;
        }
        else
        {
            __result = 8;
            return false;
        }
    }

    public static IEnumerable<CodeInstruction> getMaxedFriendshipPercent_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        CodeMatcher matcher = new(instructions);
        
        var ours = AccessTools.Method(typeof(UtilityPatches), nameof(MaxedFriendshipHelper));
        var canBeRomanced = AccessTools.Field(typeof(StardewValley.GameData.Characters.CharacterData), "CanBeRomanced");

        matcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldfld, canBeRomanced),
                new CodeMatch(OpCodes.Brtrue_S))
            .ThrowIfNotMatch("Could not find entrypoint for " + nameof(getMaxedFriendshipPercent_transpiler))
            .Insert(
                new CodeInstruction(OpCodes.Ldloc, 6),
                new CodeInstruction(OpCodes.Call, ours),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Add),
                new CodeInstruction(OpCodes.Stloc_0));
        
        return matcher.InstructionEnumeration();
    }
    
    /// <returns>1 if maxed friend with negative hearts, 0 otherwise</returns>
    public static int MaxedFriendshipHelper(Friendship? friendship)
    {
        if (friendship != null && friendship.Points <= -8 * 250)
        {
            return 1;
        }

        return 0;
    }
}