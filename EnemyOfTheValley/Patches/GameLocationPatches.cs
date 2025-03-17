using System.Reflection;
using System.Reflection.Emit;
using EnemyOfTheValley.Util;
using HarmonyLib;
using StardewValley;
using xTile.Dimensions;

namespace EnemyOfTheValley.Patches;

public class GameLocationPatches
{
    public static void Patch(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.mailbox)),
            transpiler: new HarmonyMethod(typeof(GameLocationPatches), nameof(mailbox_Transpiler))
        );
        
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), new [] {typeof(string[]), typeof(Farmer), typeof(Location)}),
            transpiler: new HarmonyMethod(typeof(GameLocationPatches), nameof(performAction_Transpiler))
        );
        
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction), new [] {typeof(string), typeof(string[])}),
            transpiler: new HarmonyMethod(typeof(GameLocationPatches), nameof(answerDialogueAction_Transpiler))
        );
    }

    public static IEnumerable<CodeInstruction> answerDialogueAction_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        CodeMatcher matcher = new CodeMatcher(instructions);

        var wipeMemoriesVanilla = AccessTools.Method(typeof(Farmer), nameof(Farmer.wipeExMemories));
        var wipeMemoriesExArchenemies =
            AccessTools.Method(typeof(Relationships), nameof(Relationships.WipeExArchenemyMemories));

        matcher.MatchEndForward(
                new CodeMatch(OpCodes.Callvirt, wipeMemoriesVanilla))
            .ThrowIfNotMatch("Could not find entry point for " + nameof(answerDialogueAction_Transpiler))
            .Advance(1)
            .Insert(new CodeMatch(OpCodes.Call, wipeMemoriesExArchenemies));

        return matcher.InstructionEnumeration();
    }

    public static IEnumerable<CodeInstruction> performAction_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        CodeMatcher matcher = new (instructions);
        var isDivorced = AccessTools.Method(typeof(Farmer), nameof(Farmer.isDivorced));
        var helper = AccessTools.Method(typeof(GameLocationPatches), nameof(IsDivorcedOrExArchenemyHelper));

        matcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Callvirt, isDivorced),
                new CodeMatch(OpCodes.Brfalse_S),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldsfld),
                new CodeMatch(OpCodes.Ldstr, "Strings\\Locations:WitchHut_EvilShrineCenter"))
            .ThrowIfNotMatch("Could not find entry point for " + nameof(performAction_Transpiler))
            .Advance(3) // advance to the break statement to insert right above it
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_2), // Farmer arg (isDivorced is already on the stack!)
                new CodeInstruction(OpCodes.Call, helper));

        return matcher.InstructionEnumeration();
    }

    public static bool IsDivorcedOrExArchenemyHelper(bool isDivorced, Farmer who)
    {
        return isDivorced || Relationships.HasAnExArchenemy(who);
    }

    public static bool CheckApologyMail(GameLocation location)
    {
        var playerEnemies = Relationships.Enemies(Game1.player);
        if (playerEnemies.Count == 0) return false;
        
        Response[] responses = new Response[playerEnemies.Count + 1];
        for (var i = 0; i < responses.Length; i++)
        {
            if (i == responses.Length - 1)
            {
                responses[i] = new Response("cancel", ModEntry.Translation.Get("ApologyLetterPromptCancelOption"));
            }
            else
            {
                var (display, nonDisplay) = playerEnemies[i];
                responses[i] = new Response(display, nonDisplay);
            }
        }
        
        location.createQuestionDialogue(ModEntry.Translation.Get("ApologyLetterPrompt"), responses, AfterCheckApologyMailBehavior);
        return true;
    }

    public static void AfterCheckApologyMailBehavior(Farmer who, string whichAnswer)
    {
        var playerEnemies = Relationships.Enemies(who);
        foreach (var (display, nonDisplay) in playerEnemies)
        {
            if (whichAnswer == nonDisplay)
            {
                Game1.addMailForTomorrow("apologyLetter_" + nonDisplay, noLetter: true);
                Game1.drawObjectDialogue(ModEntry.Translation.Get("ApologyLetterSent", new { npc = display }));
            }
        }
    }

    public static IEnumerable<CodeInstruction> mailbox_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        CodeMatcher matcher = new (instructions, generator);
        
        var apologyMailMethod = AccessTools.Method(typeof(GameLocationPatches), nameof(GameLocationPatches.CheckApologyMail));

        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "Strings\\StringsFromCSFiles:GameLocation.cs.8429"))
            .ThrowIfNotMatch("Could not find entry point for " + nameof(mailbox_Transpiler))
            .Advance(-1)
            .CreateLabel(out Label label)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, apologyMailMethod),
                new CodeInstruction(OpCodes.Brfalse, label),
                new CodeInstruction(OpCodes.Ret));

        return matcher.InstructionEnumeration();
    }
}