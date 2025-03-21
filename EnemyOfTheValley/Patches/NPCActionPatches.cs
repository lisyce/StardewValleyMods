using EnemyOfTheValley.Util;
using HarmonyLib;
using StardewValley;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Xna.Framework;

namespace EnemyOfTheValley.Patches
{
    internal class NPCActionPatches
    {
        public static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToReceiveActiveObject)),
                prefix: new HarmonyMethod(typeof(NPCActionPatches), nameof(tryToReceiveActiveObject_Prefix)),
                transpiler: new HarmonyMethod(typeof(NPCActionPatches), nameof(tryToReceiveActiveObject_Transpiler))
                );       
        }

        public static bool tryToReceiveActiveObject_Prefix(ref NPC __instance, Farmer who, bool probe, ref bool __result)
        {
            if (__instance.IsInvisible || __instance.isSleeping.Value || !who.CanMove) return true;  // run original

            if (NpcReceiveObjectApi.Instance.HandleItem(who.ActiveObject?.QualifiedItemId, __instance, who, probe))
            {
                __result = true;
                return false;  // skip original
            }

            return true;
        }
        
        public static IEnumerable<CodeInstruction> tryToReceiveActiveObject_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new(instructions, generator);
            // part 1: more dialogue options for bouquet and pendant
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

        public static void HandleReconciliationDust(NPC npc, Farmer who)
        {
            who.friendshipData.TryGetValue(npc.Name, out var friendship);

            friendship ??= (who.friendshipData[npc.Name] = new Friendship());

            if (!Relationships.IsRelationship(npc.Name, Relationships.Archenemy, who))
            {
                Game1.drawObjectDialogue(ModEntry.Translation.Get("ReconciliationDustInvalid", new { npc = npc.displayName }));
            }
            else
            {
                who.reduceActiveItemByOne();

                var loc = npc.currentLocation;
                
                friendship.Points = 0;
                Relationships.SetRelationship(npc.Name, Relationships.ExArchenemy);
                
                Game1.playSound("fireball");
                var sparkleRect = npc.GetBoundingBox();
                sparkleRect.Inflate(0, 32);
                sparkleRect.Y -= 64;
                sparkleRect.X -= 32;
                loc.temporarySprites.AddRange(Utility.sparkleWithinArea(sparkleRect, 6, Color.Gold));

                DelayedAction.functionAfterDelay(() =>
                {
                    npc.CurrentDialogue.Push(npc.TryGetDialogue("ReconciliationDustUsed") ?? new Dialogue(npc, "ReconciliationDustUsed", ModEntry.Translation.Get("ReconciliationDustUsed")));
                    Game1.drawDialogue(npc);
                }, 1000);
                
                npc.faceTowardFarmerForPeriod(3000, 4, faceAway: false, who);
                npc.doEmote(16);  // exclamation emote
                npc.facePlayer(who);
                
                Traverse traverse = Traverse.Create(typeof(Game1)).Field("multiplayer");
                traverse.GetValue<Multiplayer>().globalChatInfoMessage("Chat_Exarchenemies", Game1.player.Name, npc.GetTokenizedDisplayName());
                
            }
        }

        public static void HandleShatteredAmulet(NPC npc, Farmer who)
        {
            who.friendshipData.TryGetValue(npc.Name, out var friendship);

            friendship ??= (who.friendshipData[npc.Name] = new Friendship());

            if (Relationships.IsRelationship(npc.Name, Relationships.Archenemy, who))
            {
                Game1.drawObjectDialogue(ModEntry.Translation.Get("RejectShatteredAmulet_AlreadyArchenemies", new { name = npc.displayName }));
            }
            else if (Relationships.IsRelationship(npc.Name, Relationships.ExArchenemy, who))
            {
                npc.CurrentDialogue.Push(npc.TryGetDialogue("RejectShatteredAmulet_ExArchenemies") ?? new Dialogue(npc, "RejectShatteredAmulet_ExArchenemies", ModEntry.Translation.Get("RejectShatteredAmulet_ExArchenemies")));
                Game1.drawDialogue(npc);
            }
            else if (friendship.Points > -250)  // don't even have 1 negative heart yet
            {
                npc.CurrentDialogue.Push(npc.TryGetDialogue("RejectShatteredAmulet_NoNegativeHearts") ?? new Dialogue(npc, "RejectShatteredAmulet_NoNegativeHearts", ModEntry.Translation.Get("RejectShatteredAmulet_NoNegativeHearts")));
                Game1.drawDialogue(npc);
            }
            else if (friendship.Points > -2000) // > -8 hearts
            {
                npc.CurrentDialogue.Push(npc.TryGetDialogue("RejectShatteredAmulet_VeryLowNegativeHearts") ?? new Dialogue(npc, "RejectShatteredAmulet_VeryLowNegativeHearts", ModEntry.Translation.Get("RejectShatteredAmulet_VeryLowNegativeHearts")));
                Game1.drawDialogue(npc);
            }
            else if (friendship.Points > -2500)  // > -10 hearts
            {
                npc.CurrentDialogue.Push(npc.TryGetDialogue("RejectShatteredAmulet_LowNegativeHearts") ?? new Dialogue(npc, "RejectShatteredAmulet_LowNegativeHearts", ModEntry.Translation.Get("RejectShatteredAmulet_LowNegativeHearts")));
                Game1.drawDialogue(npc);
            }
            else  // we become archenemies!
            {
                Traverse traverse = Traverse.Create(typeof(Game1)).Field("multiplayer");
                traverse.GetValue<Multiplayer>().globalChatInfoMessage("Archenemies", Game1.player.Name, npc.GetTokenizedDisplayName());

                npc.CurrentDialogue.Push(npc.TryGetDialogue("AcceptShatteredAmulet") ?? new Dialogue(npc, "AcceptShatteredAmulet", ModEntry.Translation.Get("AcceptShatteredAmulet")));
                Relationships.SetRelationship(npc.Name, Relationships.Archenemy);

                // Next two lines are in the original bouquet accept code, but not in use for enemies for now
                //who.autoGenerateActiveDialogueEvent("enemies_" + npc.Name);
                //who.autoGenerateActiveDialogueEvent("enemies");

                who.changeFriendship(-50, npc);
                who.reduceActiveItemByOne();
                who.completelyStopAnimatingOrDoingAction();
                npc.facePlayer(who);
                npc.doEmote(12);  // angryEmote
                Game1.drawDialogue(npc);
            }
        }

        public static void HandleCake(NPC npc, Farmer who)
        {
            who.friendshipData.TryGetValue(npc.Name, out var friendship);

            friendship ??= (who.friendshipData[npc.Name] = new Friendship());

            if (Relationships.IsRelationship(npc.Name, Relationships.Enemy, who))
            {
                Game1.drawObjectDialogue(ModEntry.Translation.Get("RejectEnemyCake_AlreadyEnemies", new { name = npc.displayName }));
            }
            else if (Relationships.IsRelationship(npc.Name, Relationships.Archenemy, who))
            {
                npc.CurrentDialogue.Push(npc.TryGetDialogue("RejectEnemyCake_Archenemies") ?? new Dialogue(npc, "RejectEnemyCake_Archenemies", ModEntry.Translation.Get("RejectEnemyCake_Archenemies")));
                Game1.drawDialogue(npc);
            }
            else if (Relationships.IsRelationship(npc.Name, Relationships.ExArchenemy, who))
            {
                npc.CurrentDialogue.Push(npc.TryGetDialogue("RejectEnemyCake_ExArchenemies") ?? new Dialogue(npc, "RejectEnemyCake_ExArchenemies", ModEntry.Translation.Get("RejectEnemyCake_ExArchenemies")));
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
                    
                npc.CurrentDialogue.Push(npc.TryGetDialogue("AcceptEnemyCake") ?? new Dialogue(npc, "AcceptEnemyCake", ModEntry.Translation.Get("AcceptEnemyCake")));
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
    }
}
