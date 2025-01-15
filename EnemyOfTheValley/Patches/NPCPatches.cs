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

        public static Dialogue? NegativeLocationDialogue(NPC npc, Dialogue? currDialogue, int heartLevel, string preface)
        {
            if (currDialogue is not null) return currDialogue;

            int hearts = -10;
            while (currDialogue is null && hearts <= -2 && heartLevel <= hearts)
            {
                currDialogue = npc.TryGetDialogue(preface + Game1.currentLocation.Name + hearts);
                hearts += 2;
            }

            return currDialogue;
        }

        public static Dialogue? NegativeDayDialogue(NPC npc, int heartLevel, string preface, string appendToEnd, string day_name, int year) {
            for (int hearts = -10; hearts <= -2; hearts += 2)
            {
                if (heartLevel <= hearts)
                {
                    Dialogue? d = npc.TryGetDialogue(preface + day_name + hearts + "_" + year + appendToEnd) ?? npc.TryGetDialogue(preface + day_name + hearts + appendToEnd);
                    if (d != null) return d;
                }
            }
            return null;
        }

        public static IEnumerable<CodeInstruction> tryToRetrieveDialogue_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new(instructions, generator);
            MethodInfo negativeDayDialogue = AccessTools.Method(typeof(NPCPatches), nameof(NegativeDayDialogue));

            // we want to insert after the for-loop
            matcher.MatchEndForward(
               new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_2),
                new CodeMatch(OpCodes.Sub),
                new CodeMatch(OpCodes.Stloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_2),
                new CodeMatch(OpCodes.Bge)
                )
                .ThrowIfNotMatch("could not find end of for loop to insert after")
                .Advance(1)
                .CreateLabel(out Label jmpLabel)
                .Insert(
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldarg_2),
                    new(OpCodes.Ldarg_1),
                    new(OpCodes.Ldarg_3),
                    new(OpCodes.Ldloc_S, (byte)1),
                    new(OpCodes.Ldloc_S, (byte)0),
                    new(OpCodes.Call, negativeDayDialogue),
                    new(OpCodes.Stloc_S, (byte)8),
                    new(OpCodes.Ldloc_S, (byte)8),
                    new(OpCodes.Brfalse, jmpLabel),
                    new(OpCodes.Ldloc_S, (byte)8),
                    new(OpCodes.Ret));


            return matcher.InstructionEnumeration();
        }

        public static IEnumerable<CodeInstruction> checkForNewCurrentDialogue_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo negativeLocDialogue = AccessTools.Method(typeof(NPCPatches), nameof(NegativeLocationDialogue));
            CodeMatcher matcher = new(instructions);

            // we want to insert under the while loop
            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_2),
                new CodeMatch(OpCodes.Sub),
                new CodeMatch(OpCodes.Stloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Brtrue_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_2),
                new CodeMatch(OpCodes.Bge_S)
                )
                .ThrowIfNotMatch("could not find the end of the while loop to insert after")
                .Advance(1)
                .Insert(
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldloc_S, (byte)7),
                    new(OpCodes.Ldarg_1),
                    new(OpCodes.Ldloc_S, (byte)6),
                    new(OpCodes.Call, negativeLocDialogue),
                    new(OpCodes.Stloc_S, (byte)7)
                );

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
                    new(OpCodes.Ldstr, "(O)BarleyZP.EnemyOfTheValley.AvoidMeCake"),
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
                    npc.CurrentDialogue.Push(npc.TryGetDialogue("RejectEnemyCake_Archenemies") ?? new Dialogue(npc, "RejectEnemyCake_Archenemies", ModEntry.Translation.Get("RejectEnemyCake_Archenemies")));
                    Game1.drawDialogue(npc);
                }
                else if (friendship.Points > -250)  // don't even have 1 negative heart yet
                {
                    npc.CurrentDialogue.Push(npc.TryGetDialogue("RejectEnemyCake_NoNegativeHearts") ?? new Dialogue(npc, "RejectEnemyCake_NoNegativeHearts", ModEntry.Translation.Get("RejectEnemyCake_NoNegativeHearts")));
                    Game1.drawDialogue(npc);
                }
                else if (friendship.Points > -1000) // > -4 hearts
                {
                    npc.CurrentDialogue.Push(npc.TryGetDialogue("RejectEnemyCake_VeryLowNegativeHearts") ?? new Dialogue(npc, "RejectEnemyCake_VeryLowNegativeHearts", ModEntry.Translation.Get("RejectEnemyCake_VeryLowNegativeHearts")));
                    Game1.drawDialogue(npc);
                }
                else if (friendship.Points > -2000)  // > -8 hearts
                {
                    npc.CurrentDialogue.Push(npc.TryGetDialogue("RejectEnemyCake_LowNegativeHearts") ?? new Dialogue(npc, "RejectEnemyCake_LowNegativeHearts", ModEntry.Translation.Get("RejectEnemyCake_LowNegativeHearts")));
                    Game1.drawDialogue(npc);
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
