using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

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

            if (hearts >= Math.Abs(entry.HeartLevel))
            {
                b.Draw(Game1.mouseCursors, new Vector2(__instance.xPositionOnScreen + 320 - 4 + hearts * 32, __instance.sprites[npcIndex].bounds.Y + 64 - 28), new Rectangle(218, 428, 7, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            }
            else
            {
                b.Draw(ModEntry.sprites, new Vector2(__instance.xPositionOnScreen + 320 - 4 + hearts * 32, __instance.sprites[npcIndex].bounds.Y + 64 - 28), new Rectangle(0, 0, 7, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            }


            return false;
        }
    }
}
