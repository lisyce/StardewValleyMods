using StardewModdingAPI;
using HarmonyLib;
using EnemyOfTheValley.Patches;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;

namespace EnemyOfTheValley
{
    public class ModEntry : Mod
    {
        public static IMonitor MonitorRef;
        public static Texture2D? sprites;  // do not reference directly in transpilers
        public override void Entry(IModHelper helper)
        {
            MonitorRef = Monitor;
            Harmony.DEBUG = true;

            Harmony harmony = new(ModManifest.UniqueID);
            FarmerPatches.Patch(harmony);
            SocialPagePatches.Patch(harmony);
            DialogueBoxPatches.Patch(harmony);

            helper.Events.Content.AssetRequested += OnAssetRequested;
            LoadSprites();
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("BarleyZP.EnemyOfTheValley/Sprites"))
            {
                e.LoadFromModFile<Texture2D>("assets/Sprites.png", AssetLoadPriority.Medium);
            }
        }

        public static Texture2D LoadSprites()
        {
            sprites ??= Game1.content.Load<Texture2D>("BarleyZP.EnemyOfTheValley/Sprites");
            return sprites;
        }
    }
}
