using StardewModdingAPI;
using HarmonyLib;
using EnemyOfTheValley.Patches;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

namespace EnemyOfTheValley
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            Harmony.DEBUG = true;

            Harmony harmony = new(ModManifest.UniqueID);
            FarmerPatches.Patch(harmony);
            SocialPagePatches.Patch(harmony);

            helper.Events.Content.AssetRequested += OnAssetRequested;
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("BarleyZP.EnemyOfTheValley/Sprites"))
            {
                e.LoadFromModFile<Texture2D>(PathUtilities.NormalizePath(@"assets/Sprites.png"), AssetLoadPriority.Medium);
            }
        }
    }
}
