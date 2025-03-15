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

            harmony.Patch(
                original: AccessTools.Method(typeof(ProfileMenu), "_SetCharacter"),
                postfix: new HarmonyMethod(typeof(ProfileMenuPatches), nameof(_SetCharacter_Postfix))
                );
            harmony.Patch(
                original: AccessTools.Method(typeof(ProfileMenu), nameof(ProfileMenu.SetupLayout)),
                prefix: new HarmonyMethod(typeof(ProfileMenuPatches), nameof(SetupLayout_Prefix))
                );
        }

        public static bool drawNPCSlotHeart_Prefix(ref ProfileMenu __instance, SpriteBatch b, float heartDrawStartX, float heartDrawStartY, SocialPage.SocialEntry entry, int hearts)
        {
            if (entry is null || entry.Friendship is null || entry.Friendship.Points >= 0) return true;

            bool isLockedHeart = !Relationships.IsRelationship(entry, Relationships.Enemy, Game1.player) && !Relationships.IsRelationship(entry, Relationships.Archenemy, Game1.player) && hearts >= 8;
            Color heartTint = ((hearts < 10 && isLockedHeart) ? (Color.Black * 0.35f) : Color.White);

            Texture2D spriteSheet = hearts < Math.Abs(entry.HeartLevel) || isLockedHeart ? ModEntry.MiscSprites : Game1.mouseCursors;
            Rectangle sourceRect = hearts < Math.Abs(entry.HeartLevel) || isLockedHeart ? new Rectangle(0, 0, 7, 6) : new Rectangle(218, 428, 7, 6);

            Vector2 heartDisplayPos = Traverse.Create(__instance).Field("_heartDisplayPosition").GetValue<Vector2>();

            int yOffset = hearts < 10 ? 0 : 32;
            hearts %= 10;
            b.Draw(spriteSheet, new Vector2(heartDrawStartX + hearts * 32, heartDisplayPos.Y + heartDrawStartY + yOffset), sourceRect, heartTint, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);

            return false;
        }

        public static void _SetCharacter_Postfix(ref ProfileMenu __instance, SocialPage.SocialEntry entry)
        {
            Traverse traverse = Traverse.Create(__instance);
            if (Relationships.IsRelationship(entry, Relationships.Enemy, Game1.player)) traverse.Field("_status").SetValue((string)ModEntry.Translation.Get("EnemyNoParen"));
            else if (Relationships.IsRelationship(entry, Relationships.Archenemy, Game1.player)) traverse.Field("_status").SetValue((string)ModEntry.Translation.Get("ArchenemyNoParen"));
            else if (Relationships.IsRelationship(__instance.Current, Relationships.ExArchenemy, Game1.player)) traverse.Field("_status").SetValue((string)ModEntry.Translation.Get("ExArchenemyNoParen"));
        }

        public static void SetupLayout_Prefix(ref ProfileMenu __instance)
        {
            Traverse traverse = Traverse.Create(__instance);
            if (Relationships.IsRelationship(__instance.Current, Relationships.Enemy, Game1.player)) traverse.Field("_status").SetValue((string)ModEntry.Translation.Get("EnemyNoParen"));
            else if (Relationships.IsRelationship(__instance.Current, Relationships.Archenemy, Game1.player)) traverse.Field("_status").SetValue((string)ModEntry.Translation.Get("ArchenemyNoParen"));
            else if (Relationships.IsRelationship(__instance.Current, Relationships.ExArchenemy, Game1.player)) traverse.Field("_status").SetValue((string)ModEntry.Translation.Get("ExArchenemyNoParen"));
        }
    }
}
