using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using EnemyOfTheValley.Util;
using static StardewValley.Menus.SocialPage;
using System.Reflection.Emit;

namespace EnemyOfTheValley.Patches
{
    internal class SocialPagePatches
    {
        public static void Patch(Harmony harmony)
        {
            
            harmony.Patch(
                original: AccessTools.Method(typeof(SocialPage), "drawNPCSlotHeart"),
                prefix: new HarmonyMethod(typeof(SocialPagePatches), nameof(drawNPCSlotHeart_Prefix))
                );

            harmony.Patch(
                original: AccessTools.Method(typeof(SocialPage), "drawNPCSlot"),
                transpiler: new HarmonyMethod(typeof(SocialPagePatches), nameof(drawNPCSlot_Transpiler)),
                postfix: new HarmonyMethod(typeof(SocialPagePatches), nameof(drawNPCSlot_Postfix))
                );
        }

        public static bool drawNPCSlotHeart_Prefix(ref SocialPage __instance, SpriteBatch b, int npcIndex, SocialPage.SocialEntry entry, int hearts)
        {
            if (entry is null || entry.Friendship is null || entry.Friendship.Points >= 0) return true;

            bool isLockedHeart = !Relationships.IsRelationship(entry, Relationships.Enemy, Game1.player) && !Relationships.IsRelationship(entry, Relationships.Archenemy, Game1.player) && hearts >= 8;
            Color heartTint = ((hearts < 10 && isLockedHeart) ? (Color.Black * 0.35f) : Color.White);

            Texture2D spriteSheet = hearts < Math.Abs(entry.HeartLevel) || isLockedHeart ? ModEntry.MiscSprites : Game1.mouseCursors;
            Rectangle sourceRect = hearts < Math.Abs(entry.HeartLevel) || isLockedHeart ? new Rectangle(0, 0, 7, 6) : new Rectangle(218, 428, 7, 6);

            int yOffset = hearts < 10 ? 28 : 0;
            hearts %= 10;
            
            b.Draw(spriteSheet, new Vector2(__instance.xPositionOnScreen + 320 - 4 + hearts * 32, __instance.sprites[npcIndex].bounds.Y + 64 - yOffset), sourceRect, heartTint, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            

            return false;
        }

        public static void drawNPCSlot_Postfix(ref SocialPage __instance, SpriteBatch b, int i)
        {
            SocialEntry entry = __instance.GetSocialEntry(i);
            if (entry == null) return;

            string? text = Relationships.IsRelationship(entry, Relationships.Enemy, Game1.player) ? ModEntry.Translation.Get("Enemy") :
                Relationships.IsRelationship(entry, Relationships.Archenemy, Game1.player) ? ModEntry.Translation.Get("Archenemy") :
                Relationships.IsRelationship(entry, Relationships.ExArchenemy, Game1.player) ? ModEntry.Translation.Get("ExArchenemy") : null;
            if (text is null && entry.IsDatable && !entry.IsRoommateForCurrentPlayer() && !entry.IsMarriedToAnyone() && !entry.IsDivorcedFromCurrentPlayer() && !(!Game1.player.isMarriedOrRoommates() && entry.IsDatingCurrentPlayer()))
            {
                text = (entry.Gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Male") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Female");
            }

            if (text is null) return;

            int width = (IClickableMenu.borderWidth * 3 + 128 - 40 + 192) / 2;
            text = Game1.parseText(text, Game1.smallFont, width);
            Vector2 textSize = Game1.smallFont.MeasureString(text);
            float lineHeight = Game1.smallFont.MeasureString("W").Y;

            b.DrawString(Game1.smallFont, text, new Vector2(__instance.xPositionOnScreen + 192 + 8 - textSize.X / 2f, __instance.sprites[i].bounds.Bottom - (textSize.Y - lineHeight)), Game1.textColor);

            // draw the little icons for the cake and amulet
            if (Relationships.IsRelationship(entry, Relationships.Enemy, Game1.player))
            {
                b.Draw(ModEntry.StandardSprites, new Vector2(__instance.xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, __instance.sprites[i].bounds.Y), new Rectangle(0, 0, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
            }
            else if (Relationships.IsRelationship(entry, Relationships.Archenemy, Game1.player))
            {
                b.Draw(ModEntry.StandardSprites, new Vector2(__instance.xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, __instance.sprites[i].bounds.Y), new Rectangle(16, 0, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
            }
        }
        public static IEnumerable<CodeInstruction> drawNPCSlot_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // goal is to look at where we first assign to "text" variable and make it empty if it's "single" since the postfix will rewrite it
            CodeMatcher matcher = new(instructions);
            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldsfld),
                new CodeMatch(OpCodes.Ldstr, "Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Male"),
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Br_S),
                new CodeMatch(OpCodes.Ldsfld),
                new CodeMatch(OpCodes.Ldstr, "Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Female"),
                new CodeMatch(OpCodes.Callvirt))
                .ThrowIfNotMatch("could not find place to remove \"single\" text")
                .Advance(2)
                .Insert(new(OpCodes.Ldstr, ""), new(OpCodes.Stloc_S, (byte)10));

            return matcher.InstructionEnumeration();
        }

    }
}
