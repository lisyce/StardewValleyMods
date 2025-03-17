using System.Reflection;
using System.Reflection.Emit;
using EnemyOfTheValley.Util;
using HarmonyLib;
using StardewValley;

namespace EnemyOfTheValley.Patches;

public class GameLocationPatches
{
    public static void Patch(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.mailbox)),
            transpiler: new HarmonyMethod(typeof(GameLocationPatches), nameof(mailbox_Transpiler))
        );
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