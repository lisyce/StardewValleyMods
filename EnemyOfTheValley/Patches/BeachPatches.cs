using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Reflection;
using System.Reflection.Emit;
using SObject = StardewValley.Object;

namespace EnemyOfTheValley.Patches
{
    internal class BeachPatches
    {
        public static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Beach), nameof(Beach.checkAction)),
                transpiler: new HarmonyMethod(typeof(BeachPatches), nameof(checkAction_Transpiler))
                );
        }

        public static bool checkShardsAction(Beach beach, xTile.Dimensions.Location tileLocation, Farmer who)
        {
            Vector2 tilePos = new(tileLocation.X, tileLocation.Y);
            if (beach.objects.TryGetValue(tilePos, out SObject obj) && obj.IsSpawnedObject && who.couldInventoryAcceptThisItem(obj))
            {
                if (who.IsLocalPlayer)
                {
                    beach.localSound("pickUpItem");
                }
                who.animateOnce(279 + who.FacingDirection);

                who.faceDirection(2);
                who.forceCanMove();
                who.completelyStopAnimatingOrDoingAction();
                who.CanMove = false;
                Game1.changeMusicTrack("none");
              
                who.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
                {
                    new FarmerSprite.AnimationFrame(57, 2500, secondaryArm: false, flip: false, delegate(Farmer who)
                    {
                        Farmer.showHoldingItem(who, obj);
                    })
                });
                DelayedAction.showDialogueAfterDelay(ModEntry.Translation.Get("PickupShards"), 2500);


                Game1.playSound("breakingGlass");

                who.addItemToInventoryBool(obj.getOne());
                beach.objects.Remove(tilePos);
                return true;
            }

            return false;
        }

        public static IEnumerable<CodeInstruction> checkAction_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new(instructions, generator);

            MethodInfo baseCheckAction = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction));
            MethodInfo ours = AccessTools.Method(typeof(BeachPatches), nameof(checkShardsAction));

            matcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldarg_2),
                new CodeMatch(OpCodes.Ldarg_3),
                new CodeMatch(OpCodes.Call, baseCheckAction))
                .ThrowIfNotMatch("could not find base GameLocation.checkAction")
                .Advance(2)  // we will use args 0 and 1 already on the stack to call our method (the labels from prev branches jump here; don't want to change them all)
                .Insert(
                new(OpCodes.Ldarg_3),
                new(OpCodes.Call, ours),
                new(OpCodes.Brfalse, null),  // we'll add a label here later, this is just a placeholder
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Ret),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldarg_1))
                .Advance(5)
                .CreateLabel(out Label label)  // label on the new ldarg_0 op where the base method args are loaded so it can get called
                .Advance(-3)
                .Set(OpCodes.Brfalse, label);  // add the label instead of a null one


            return matcher.InstructionEnumeration();
        }
    }
}
