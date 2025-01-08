﻿using StardewModdingAPI;
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
        public static Texture2D? sprites;  // do not reference directly in transpilers
        public override void Entry(IModHelper helper)
        {
            Monitor = base.Monitor;
            Translation = helper.Translation;
            //Harmony.DEBUG = true;

            Harmony harmony = new(ModManifest.UniqueID);
            FarmerPatches.Patch(harmony);
            SocialPagePatches.Patch(harmony);
            DialogueBoxPatches.Patch(harmony);
            NPCPatches.Patch(harmony);
            ProfileMenuPatches.Patch(harmony);

            helper.Events.Content.AssetRequested += OnAssetRequested;
            LoadSprites();

            helper.ConsoleCommands.Add("enemy", "Sets the specified NPC to be the player's enemy", SetEnemy);
            helper.ConsoleCommands.Add("archenemy", "Sets the specified NPC to be the player's archenemy", SetArchenemy);
            helper.ConsoleCommands.Add("exarchenemy", "Sets the specified NPC to be the player's ex-archenemy", SetExArchenemy);
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
