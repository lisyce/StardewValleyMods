using StardewModdingAPI;
using HarmonyLib;
using EnemyOfTheValley.Patches;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using EnemyOfTheValley.Common;

namespace EnemyOfTheValley
{
    public class ModEntry : Mod
    {
        public static IMonitor Monitor;
        public static ITranslationHelper Translation;
        public static Texture2D? MiscSprites;  // do not reference directly in transpilers
        public static Texture2D? StandardSprites;
        public override void Entry(IModHelper helper)
        {
            Monitor = base.Monitor;
            Translation = helper.Translation;
            Harmony.DEBUG = true;

            Harmony harmony = new(ModManifest.UniqueID);
            FarmerPatches.Patch(harmony);
            SocialPagePatches.Patch(harmony);
            DialogueBoxPatches.Patch(harmony);
            NPCActionPatches.Patch(harmony);
            NPCDialoguePatches.Patch(harmony);
            ProfileMenuPatches.Patch(harmony);

            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;  // TODO remove for release
            helper.Events.GameLoop.DayEnding += OnDayEnding;
            LoadMiscSprites();

            helper.ConsoleCommands.Add("enemy", "Sets the specified NPC to be the player's enemy", SetEnemy);
            helper.ConsoleCommands.Add("archenemy", "Sets the specified NPC to be the player's archenemy", SetArchenemy);
            helper.ConsoleCommands.Add("exarchenemy", "Sets the specified NPC to be the player's ex-archenemy", SetExArchenemy);

            Event.RegisterPrecondition("NegativeFriendship", Preconditions.NegativeFriendship);
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("BarleyZP.EnemyOfTheValley/MiscSprites"))
            {
                e.LoadFromModFile<Texture2D>("assets/MiscSprites.png", AssetLoadPriority.Medium);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("BarleyZP.EnemyOfTheValley/StandardSprites"))
            {
                e.LoadFromModFile<Texture2D>("assets/StandardSprites.png", AssetLoadPriority.Medium);
            }
        }

        // FOR DEBUG ONLY
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            Game1.player.mailReceived.Remove("enemyCake");
            Game1.player.cookingRecipes.Remove("BarleyZP.EnemyOfTheValley.AvoidMeCake");
        }

        private void OnDayEnding(object? sender, DayEndingEventArgs e)
        {
            if (Game1.player.mailReceived.Contains("enemyCake")) return;

            foreach (var item in Game1.player.friendshipData)
            {
                foreach (var friendship in item.Values)
                {
                    if (friendship.Points <= -2000)
                    {
                        Game1.player.mailForTomorrow.Add("enemyCake");
                        break;
                    }
                }
            }
        }

        public static Texture2D LoadMiscSprites()
        {
            MiscSprites ??= Game1.content.Load<Texture2D>("BarleyZP.EnemyOfTheValley/MiscSprites");
            return MiscSprites;
        }

        public static void SetEnemy(string command, string[] args) {
            Relationships.SetRelationship(args[0], Relationships.Enemy, printValidation: true);
        }

        public static void SetArchenemy(string command, string[] args)
        {
            Relationships.SetRelationship(args[0], Relationships.Archenemy, printValidation: true);
        }

        public static void SetExArchenemy(string command, string[] args)
        {
            Relationships.SetRelationship(args[0], Relationships.ExArchenemy, printValidation: true);
        }
    }
}
