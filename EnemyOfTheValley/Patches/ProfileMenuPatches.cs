using EnemyOfTheValley.Common;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace EnemyOfTheValley.Patches
{
    internal class ProfileMenuPatches
    {
        public static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(ProfileMenu), "drawNPCSlotHeart"),
                prefix: new HarmonyMethod(typeof(ProfileMenuPatches), nameof(drawNPCSlotHeart_Prefix))
                );
        }

        public static bool drawNPCSlotHeart_Prefix(ref ProfileMenu __instance, SpriteBatch b, float heartDrawStartX, float heartDrawStartY, SocialPage.SocialEntry entry, int hearts)
        {
            if (entry is null || entry.Friendship is null || entry.Friendship.Points >= 0) return true;

            bool isLockedHeart = !Relationships.IsEnemy(entry) && !Relationships.IsArchEnemy(entry) && hearts >= 8;
            Color heartTint = ((hearts < 10 && isLockedHeart) ? (Color.Black * 0.35f) : Color.White);

            Texture2D spriteSheet = hearts < Math.Abs(entry.HeartLevel) || isLockedHeart ? ModEntry.sprites : Game1.mouseCursors;
            Rectangle sourceRect = hearts < Math.Abs(entry.HeartLevel) || isLockedHeart ? new Rectangle(0, 0, 7, 6) : new Rectangle(218, 428, 7, 6);

            Vector2 heartDisplayPos = Traverse.Create(__instance).Field("_heartDisplayPosition").GetValue<Vector2>();

            hearts %= 10;
            b.Draw(spriteSheet, new Vector2(heartDrawStartX + hearts * 32, heartDisplayPos.Y + heartDrawStartY), sourceRect, heartTint, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);

            return false;
        }
    }
}
