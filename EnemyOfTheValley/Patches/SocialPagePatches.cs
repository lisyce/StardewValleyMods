using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using EnemyOfTheValley.Common;

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
        }

        public static bool drawNPCSlotHeart_Prefix(ref SocialPage __instance, SpriteBatch b, int npcIndex, SocialPage.SocialEntry entry, int hearts)
        {
            if (entry is null || entry.Friendship is null || entry.Friendship.Points >= 0) return true;

            bool isLockedHeart = !Relationships.IsEnemy(entry) && !Relationships.IsArchEnemy(entry) && hearts >= 8;
            Color heartTint = ((hearts < 10 && isLockedHeart) ? (Color.Black * 0.35f) : Color.White);

            Texture2D spriteSheet = hearts < Math.Abs(entry.HeartLevel) || isLockedHeart ? ModEntry.sprites : Game1.mouseCursors;
            Rectangle sourceRect = hearts < Math.Abs(entry.HeartLevel) || isLockedHeart ? new Rectangle(0, 0, 7, 6) : new Rectangle(218, 428, 7, 6);

            hearts %= 10;
            b.Draw(spriteSheet, new Vector2(__instance.xPositionOnScreen + 320 - 4 + hearts * 32, __instance.sprites[npcIndex].bounds.Y + 64 - 28), sourceRect, heartTint, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            

            return false;
        }
    }
}
