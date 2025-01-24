using EnemyOfTheValley.Common;
using HarmonyLib;
using StardewValley;
using System.Reflection;
using System.Reflection.Emit;

namespace EnemyOfTheValley.Patches
{
    internal class NPCActionPatches
    {
        public static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToReceiveActiveObject)),
                transpiler: new HarmonyMethod(typeof(NPCActionPatches), nameof(tryToReceiveActiveObject_Transpiler))
                );       
        }

        
        public static IEnumerable<CodeInstruction> tryToReceiveActiveObject_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new(instructions, generator);
            // part 1: more dialogue options for bouquet and pendant
            // todo: need the jump instruction to THIS branch from the previous one too, not just to the next one
            MethodInfo get_Points = AccessTools.PropertyGetter(typeof(Friendship), nameof(Friendship.Points));
            MethodInfo isMarriedOrRoommates = AccessTools.Method(typeof(Farmer), nameof(Farmer.isMarriedOrRoommates));
            MethodInfo isEngaged = AccessTools.Method(typeof(Farmer), nameof(Farmer.isEngaged));

            MethodInfo mermaidHandler = AccessTools.Method(typeof(NPCActionPatches), nameof(NegativeMermaidPendantDialogue));
            
            // Mermaid's Pendant
            matcher.Start().MatchStartForward(
                    new CodeMatch(OpCodes.Ldstr, "Strings\\StringsFromCSFiles:NPC.cs.3967"))
                .MatchEndForward(
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Ret))
                .ThrowIfNotMatch("could not find end of else-if statement to insert after")
                .Advance(1)
                .CreateLabel(out Label afterMermaidElseifLabel)
                .Insert(
                    // if friendship is null or we DON'T have negative friendship points, break to the label we just created (key name: RejectMermaidPendant_NegativeHearts)
                    // (since this else if branch will handle mermaid pendant when there are negative hearts
                    new(OpCodes.Ldloc_1),
                    new(OpCodes.Brfalse, afterMermaidElseifLabel),
                    new(OpCodes.Ldloc_1),
                    new(OpCodes.Call, get_Points),
                    new(OpCodes.Ldc_I4_0),
                    new(OpCodes.Bge, afterMermaidElseifLabel),
                    // our stuff
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldarg_2),
                    new(OpCodes.Ldloc_2),
                    new(OpCodes.Call, mermaidHandler),
                    new CodeMatch(OpCodes.Ret)
                    )
                .CreateLabel(out Label startMermaidElseif)
                // change the previous else-if's labels
                .Start().MatchEndForward(
                    new CodeMatch(OpCodes.Ldloc_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Callvirt, isMarriedOrRoommates),
                    new CodeMatch(OpCodes.Brtrue_S),
                    new CodeMatch(OpCodes.Ldloc_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Callvirt, isEngaged),
                    new CodeMatch(OpCodes.Brfalse))
                .ThrowIfNotMatch("failed to go back to change if statement's label to point to the mermaid handler")
                .Set(OpCodes.Brfalse, startMermaidElseif);

            // Bouquet (our branch goes under the isDivorced() branch)
            MethodInfo isDivorced = AccessTools.Method(typeof(Friendship), nameof(Friendship.IsDivorced));

            MethodInfo bouquetHandler = AccessTools.Method(typeof(NPCActionPatches), nameof(NegativeBouquetDialogue));
            matcher.Start().MatchStartForward(
                    new CodeMatch(OpCodes.Ldstr, "Strings\\Characters:Divorced_bouquet"))
                .MatchEndForward(
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Ret))
                .ThrowIfNotMatch("could not find end of else-if statement to insert after")
                .Advance(1)
                .CreateLabel(out Label afterBouquetElseIfLabel)
                .Insert(
                    // if we DON'T have negative friendship points, break to the label we just created
                    new(OpCodes.Ldloc_1),
                    new(OpCodes.Call, get_Points),
                    new(OpCodes.Ldc_I4_0),
                    new(OpCodes.Bge, afterBouquetElseIfLabel),
                    // our stuff
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldarg_2),
                    new(OpCodes.Call, bouquetHandler),
                    new(OpCodes.Ldc_I4_1),
                    new(OpCodes.Ret)  // so we don't have to jump down
                    )
                .CreateLabel(out Label startBouquetElseIf)
                // change the previous else-if's labels
                .Start().MatchEndForward(
                    new CodeMatch(OpCodes.Ldstr, "Strings\\UI:AlreadyDatingBouquet")
                    )
                .MatchEndForward(
                    new CodeMatch(OpCodes.Callvirt, isDivorced),
                    new CodeMatch(OpCodes.Brfalse_S))
                .ThrowIfNotMatch("failed to go back to change if statement's label to point to the bouquet handler")
                .Set(OpCodes.Brfalse_S, startBouquetElseIf);


            // we insert switch cases (eek)
            // our handler
            MethodInfo cakeMethod = AccessTools.Method(typeof(NPCActionPatches), nameof(HandleCake));

            // add the new case body
            int cakePos = matcher.Start().MatchEndForward(
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

        public static bool NegativeMermaidPendantDialogue(NPC npc, bool probe, bool canReceiveGifts)
        {
            if (!canReceiveGifts) return false;
            if (!probe)
            {
                npc.CurrentDialogue.Push(npc.TryGetDialogue("RejectMermaidPendant_NegativeHearts") ?? new Dialogue(npc, "RejectMermaidPendant_NegativeHearts", ModEntry.Translation.Get("RejectMermaidPendant_NegativeHearts")));
                Game1.drawDialogue(npc);
            }

            return true;
        }

        public static void NegativeBouquetDialogue(NPC npc, bool probe)
        {
            if (!probe)
            {
                npc.CurrentDialogue.Push(npc.TryGetDialogue("RejectBouquet_NegativeHearts") ?? new Dialogue(npc, "RejectBouquet_NegativeHearts", ModEntry.Translation.Get("RejectBouquet_NegativeHearts")));
                Game1.drawDialogue(npc);
            }
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
