using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Reflection.Emit;


namespace EnemyOfTheValley.Patches
{
    internal class DialogueBoxPatches
    {
        public static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(DialogueBox), nameof(DialogueBox.drawPortrait)),
                postfix: new HarmonyMethod(typeof(DialogueBoxPatches), nameof(drawPortrait_Postfix))
                );
            //harmony.Patch(
            //    original: AccessTools.Method(typeof(DialogueBox), nameof(DialogueBox.performHoverAction)),
            //    transpiler: new HarmonyMethod(typeof(DialogueBoxPatches), nameof(performHoverAction_Transpiler))
            //    );
        }

        public static void drawPortrait_Postfix(ref DialogueBox __instance, SpriteBatch b)
        {
            NPC speaker = __instance.characterDialogue.speaker;
            if (!Game1.IsMasterGame && !speaker.EventActor && !(speaker.currentLocation?.IsActiveLocation() ?? false))
            {
                NPC actualSpeaker = Game1.getCharacterFromName(speaker.Name);
                if (actualSpeaker != null && actualSpeaker.currentLocation.IsActiveLocation())
                {
                    speaker = actualSpeaker;
                }
            }

            int friendshipHearts = Game1.player.getFriendshipHeartLevelForNPC(speaker.Name);
          

            if (friendshipHearts >= 0) return;
            friendshipHearts = Math.Abs(friendshipHearts);

            if (__instance.width >= 642 && __instance.shouldDrawFriendshipJewel())
            {
                b.Draw(
                    ModEntry.MiscSprites,
                    new Vector2(__instance.friendshipJewel.X, __instance.friendshipJewel.Y),
                    (friendshipHearts >= 10) ? new Rectangle(0, 62, 11, 11) : new Rectangle(Math.Max(0, (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 / 250.0) * 11), Math.Max(7, 7 + friendshipHearts / 2 * 11), 11, 11),
                    Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            }
        }

        //public static int GetHoverMaxHearts(int currMax, DialogueBox __instance)
        //{
        //    NPC npc = __instance.characterDialogue.speaker;
        //    if (Game1.player.getFriendshipHeartLevelForNPC(npc.Name) < 0)
        //    {
        //        return -1 * currMax;
        //    }

        //    return currMax;
        //}

        //public static IEnumerable<CodeInstruction> performHoverAction_Transpiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    CodeMatcher matcher = new(instructions);

        //    MethodInfo getMaxHearts = AccessTools.Method(typeof(Utility), nameof(Utility.GetMaximumHeartsForCharacter));
        //    MethodInfo ours = AccessTools.Method(typeof(DialogueBoxPatches), nameof(DialogueBoxPatches.GetHoverMaxHearts));

        //    matcher.MatchStartForward(
        //        new CodeMatch(OpCodes.Call, getMaxHearts))
        //        .ThrowIfNotMatch("could not find call to GetMaximumHeartsForCharacter")
        //        .Advance(1)
        //        .Insert(
        //            new(OpCodes.Ldarg_0),
        //            new(OpCodes.Call, ours)
        //            );


        //    return matcher.InstructionEnumeration();
        //}
    }
}
