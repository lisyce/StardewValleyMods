using EnemyOfTheValley.Common;
using HarmonyLib;
using StardewValley;
using StardewValley.Extensions;
using System.Reflection;
using System.Reflection.Emit;
using SObject = StardewValley.Object;

namespace EnemyOfTheValley.Patches
{
    internal class NPCPatches
    {
        public static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.checkForNewCurrentDialogue)),
                transpiler: new HarmonyMethod(typeof(NPCPatches), nameof(checkForNewCurrentDialogue_Transpiler))
                );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToRetrieveDialogue)),
                transpiler: new HarmonyMethod(typeof(NPCPatches), nameof(tryToRetrieveDialogue_Transpiler))
                );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToReceiveActiveObject)),
                transpiler: new HarmonyMethod(typeof(NPCPatches), nameof(tryToReceiveActiveObject_Transpiler))
                );
        }

        public static IEnumerable<CodeInstruction> tryToRetrieveDialogue_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // match the hearts >= -2 condition in the for loop to make it a -10
            CodeMatcher matcher = new(instructions);

            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ret),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_2),
                new CodeMatch(OpCodes.Sub),
                new CodeMatch(OpCodes.Stloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_2))
                .ThrowIfNotMatch("could not find place to change for loop bounds")
                .Set(OpCodes.Ldc_I4, -10);

            return matcher.InstructionEnumeration();
        }

        public static IEnumerable<CodeInstruction> checkForNewCurrentDialogue_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // match the num -= 2, then the dialogue2 == null so we can replace the num (hearts) >= 2 with num >= -10
            // (NPC::checkForNewCurrentDialogue on line 3945) which is a while loop condition
            CodeMatcher matcher = new(instructions);

            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_2),
                new CodeMatch(OpCodes.Sub),
                new CodeMatch(OpCodes.Stloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Brtrue_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_2)
                )
                .ThrowIfNotMatch("could not find place to change while loop bounds")
                .Set(OpCodes.Ldc_I4, -10);

            return matcher.InstructionEnumeration();
        }

        public static IEnumerable<CodeInstruction> tryToReceiveActiveObject_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // we insert switch cases (eek)
            CodeMatcher matcher = new(instructions, generator);

            // our handlers
            MethodInfo cakeMethod = AccessTools.Method(typeof(NPCPatches), nameof(HandleCake));

            // add the new case bodies
            int cakePos = matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldstr, "(O)460"),
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Brtrue),
                new CodeMatch(OpCodes.Br))
                .ThrowIfNotMatch("could not find place to add new switch case bodies")
                .Pos;
            matcher.Advance(1)
                .Insert(
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldarg_1),
                    new(OpCodes.Ldarg_2),
                    new(OpCodes.Ldloc_2),
                    new(OpCodes.Call, cakeMethod),
                    new(OpCodes.Ret)
                )
                .CreateLabel(out Label cakeLabel);

            // add the new case control flow
            MethodInfo strEquality = AccessTools.Method(typeof(string), "op_Equality");
            matcher.Start().Advance(cakePos)
                .Insert(
                    new(OpCodes.Ldloc_S, (byte)11),
                    new(OpCodes.Ldstr, "(O)582"),
                    new(OpCodes.Call, strEquality),
                    new(OpCodes.Brtrue, cakeLabel)
                );

            return matcher.InstructionEnumeration();
        }

        public static bool HandleCake(NPC npc, Farmer who, bool probe, bool canReceiveGifts)
        {
            if (!canReceiveGifts) return false;

            if (!probe)
            {
                who.friendshipData.TryGetValue(npc.Name, out var friendship);

                friendship ??= (who.friendshipData[npc.Name] = new Friendship());

                if (Relationships.IsRelationship(friendship, Relationships.Enemy))
                {
                    Game1.drawObjectDialogue(ModEntry.Translation.Get("RejectEnemyCake_AlreadyEnemies", new { name = npc.displayName }));
                }
                else if (Relationships.IsRelationship(friendship, Relationships.Archenemy))
                {

                }
                else if (friendship.Points > -250)  // don't even have 1 negative heart yet
                {

                }
                else if (friendship.Points > -1000) // > -4 hearts
                {

                }
                else if (friendship.Points > -2000)  // > -8 hearts
                {

                }
                else  // we become enemies!
                {
                    Traverse traverse = Traverse.Create(typeof(Game1)).Field("multiplayer");
                    traverse.GetValue<Multiplayer>().globalChatInfoMessage("Enemies", Game1.player.Name, npc.GetTokenizedDisplayName());

                    if (Relationships.IsRelationship(friendship, Relationships.ExArchenemy))
                    {
                        npc.CurrentDialogue.Push(npc.TryGetDialogue("AcceptEnemyCake_ExArchenemies") ?? new Dialogue(npc, "AcceptEnemyCake_ExArchenemies", ModEntry.Translation.Get("AcceptEnemyCake_ExArchenemies")));
                    } else
                    {
                        npc.CurrentDialogue.Push(npc.TryGetDialogue("AcceptEnemyCake") ?? new Dialogue(npc, "AcceptEnemyCake", ModEntry.Translation.Get("AcceptEnemyCake")));
                    }

                    Relationships.SetRelationship(npc.Name, Relationships.Enemy);

                    // Next two lines are in the original bouquet accept code, but not in use for enemies for now
                    //who.autoGenerateActiveDialogueEvent("enemies_" + npc.Name);
                    //who.autoGenerateActiveDialogueEvent("enemies");

                    who.changeFriendship(-25, npc);
                    who.reduceActiveItemByOne();
                    who.completelyStopAnimatingOrDoingAction();
                    npc.facePlayer(who);
                    npc.doEmote(12);  // angryEmote
                    Game1.drawDialogue(npc);
                }

            }

            return true;
        }
    }
}
