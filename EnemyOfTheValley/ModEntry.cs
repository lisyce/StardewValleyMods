using StardewModdingAPI;
using HarmonyLib;
using EnemyOfTheValley.Patches;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using EnemyOfTheValley.Util;
using EOTVPreconditions = EnemyOfTheValley.Util.Preconditions;
using EOTVGameStateQueries = EnemyOfTheValley.Util.GameStateQueries;
using StardewValley.Locations;
using Microsoft.Xna.Framework;
using SObject = StardewValley.Object;

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
            
            Monitor.Log("This mod patches the way dialogue keys are handled. If you are having issues with a dialogue key not showing, ensure that it happens without this mod installed before reporting it to the respective mod authors.", LogLevel.Debug);
            
            Harmony harmony = new(ModManifest.UniqueID);
            FarmerPatches.Patch(harmony);
            SocialPagePatches.Patch(harmony);
            DialogueBoxPatches.Patch(harmony);
            NPCActionPatches.Patch(harmony);
            NPCDialoguePatches.Patch(harmony);
            ProfileMenuPatches.Patch(harmony);
            BeachPatches.Patch(harmony);
            GameLocationPatches.Patch(harmony);
            UtilityPatches.Patch(harmony);

            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.DayEnding += OnDayEnding;

            LoadMiscSprites();
            LoadStandardSprites();

            helper.ConsoleCommands.Add("EOTV_enemy", "Sets the specified NPC to be the player's enemy", SetEnemy);
            helper.ConsoleCommands.Add("EOTV_archenemy", "Sets the specified NPC to be the player's archenemy", SetArchenemy);
            helper.ConsoleCommands.Add("EOTV_exarchenemy", "Sets the specified NPC to be the player's ex-archenemy", SetExArchenemy);
            helper.ConsoleCommands.Add("EOTV_change_friendship", "Changes the friendship of the NPC (first arg) by the amount given in the second arg", ChangeFriendship);
            helper.ConsoleCommands.Add("EOTV_maxed_friends", "Outputs Utility::getMaxedFriendshipPercent", MaxedFriendshipPercent);
            
            Event.RegisterPrecondition("EOTV_NegativeFriendship", EOTVPreconditions.NegativeFriendship);
            
            GameStateQuery.Register("EOTV_PLAYER_NPC_ENEMY", EOTVGameStateQueries.PlayerNpcEnemy);
            GameStateQuery.Register("EOTV_PLAYER_NPC_ARCHENEMY", EOTVGameStateQueries.PlayerNpcArchenemy);
            GameStateQuery.Register("EOTV_PLAYER_NPC_EXARCHENEMY", EOTVGameStateQueries.PlayerNpcExArchenemy);
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

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            // do the beach shards
            Beach beach = (Beach)Game1.getLocationFromName("Beach");
            if (Game1.wasRainingYesterday && Relationships.HasAnEnemyWithHeartLevel(Game1.player, -10) && !beach.IsRainingHere())
            {
                var oldMariner = Traverse.Create(beach).Field<NPC>("oldMariner").Value;
                if (oldMariner != null) return;  // somehow the mariner is here even though it isn't raining; we can't place the shards

                Vector2 marinerPos = new(80, 5);
                beach.overlayObjects.Remove(marinerPos);
                var shards = ItemRegistry.Create<SObject>("BarleyZP.EnemyOfTheValley.ShatteredAmulet");
                shards.TileLocation = marinerPos;
                shards.IsSpawnedObject = true;
                beach.overlayObjects.Add(marinerPos, shards);
            }
            
            // do apology letters
            List<string> toRemove = new();
            foreach (string key in Game1.player.mailReceived)
            {
                if (key.StartsWith("apologyLetter_"))
                {
                    var npcName = key.Replace("apologyLetter_", "");
                    Relationships.SetRelationship(npcName, FriendshipStatus.Friendly, true);
                    if (Game1.player.friendshipData.TryGetValue(npcName, out var friendship))
                    {
                        friendship.Points = Math.Max(-1250, friendship.Points);
                    }
                    var npc = Game1.getCharacterFromName(npcName);
                    var traverse = Traverse.Create(typeof(Game1)).Field("multiplayer");
                    traverse.GetValue<Multiplayer>().globalChatInfoMessage("Apologized", Game1.player.Name, npc?.GetTokenizedDisplayName() ?? "Unknown NPC");

                    Game1.player.activeDialogueEvents.TryAdd("BarleyZP.EnemyOfTheValley.apologized_" + npcName, 4);
                    toRemove.Add(key);
                }
            }
            
            // make these repeatable
            Game1.player.mailReceived.ExceptWith(toRemove);
        }

        private void OnDayEnding(object? sender, DayEndingEventArgs e)
        {
            // get rid of door unlock for NPCs that fell below 0 hearts
            foreach (string name in Game1.player.friendshipData.Keys)
            {
                if (Game1.player.mailReceived.Contains("doorUnlock" + name) && Game1.player.friendshipData[name].Points < 0)
                {
                    Game1.player.mailReceived.Remove("doorUnlock" + name);
                }
            }

            // remove beach shards
            Beach beach = (Beach)Game1.getLocationFromName("Beach");
            Vector2 marinerPos = new(80, 5);
            beach.overlayObjects.Remove(marinerPos);
        }

        public static Texture2D LoadMiscSprites()
        {
            MiscSprites ??= Game1.content.Load<Texture2D>("BarleyZP.EnemyOfTheValley/MiscSprites");
            return MiscSprites;
        }

        public static Texture2D LoadStandardSprites()
        {
            StandardSprites ??= Game1.content.Load<Texture2D>("BarleyZP.EnemyOfTheValley/StandardSprites");
            return StandardSprites;
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

        public static void ChangeFriendship(string command, string[] args)
        {
            if (!ArgUtility.TryGetInt(args, 1, out int amt, out string err))
            {
                Monitor.Log(err, LogLevel.Error);
            }
            else
            {
                var npc = Game1.getCharacterFromName<NPC>(args[0]);
                if (!Game1.player.friendshipData.TryGetValue(args[0], out var friendship))
                {
                    Monitor.Log("NPC not found in player's friendship data.", LogLevel.Error);
                }
                else
                {
                    int before = friendship.Points;
                    Game1.player.changeFriendship(amt, npc);
                    int after = friendship.Points;
                    
                    Monitor.Log("Before: " + before + ". After: " + after, LogLevel.Info);
                }
            }
            
        }

        public static void MaxedFriendshipPercent(string command, string[] args)
        {
            Monitor.Log(Utility.getMaxedFriendshipPercent().ToString(), LogLevel.Info);
        }
    }
}
