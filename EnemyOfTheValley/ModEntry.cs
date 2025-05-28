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
using StardewValley.Delegates;
using StardewValley.GameData.Characters;
using SObject = StardewValley.Object;

namespace EnemyOfTheValley
{
    public class ModEntry : Mod
    {
        public static IMonitor StaticMonitor;
        public static ITranslationHelper Translation;
        public static Texture2D? MiscSprites;  // do not reference directly in transpilers
        public static Texture2D? StandardSprites;

        private static readonly HashSet<string> OptedInNpcs = new() { "Abigail", "Alex", "Clint", "Demetrius", "Elliott", "Haley", "Lewis", "Pierre" };
        public override void Entry(IModHelper helper)
        {
            StaticMonitor = Monitor;
            Translation = helper.Translation;
            
            StaticMonitor.Log("This mod patches the way dialogue keys are handled. If you are having issues with a dialogue key not showing, ensure that it happens without this mod installed before reporting it to the respective mod authors.", LogLevel.Debug);
            
            Harmony harmony = new(ModManifest.UniqueID);
            // Harmony.DEBUG = true;
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

            helper.ConsoleCommands.Add("EOTV_friendly", "Sets the specified NPC to have the 'friendly' relationship with the player", SetFriendly);
            helper.ConsoleCommands.Add("EOTV_enemy", "Sets the specified NPC to be the player's enemy", SetEnemy);
            helper.ConsoleCommands.Add("EOTV_archenemy", "Sets the specified NPC to be the player's archenemy", SetArchenemy);
            helper.ConsoleCommands.Add("EOTV_exarchenemy", "Sets the specified NPC to be the player's ex-archenemy", SetExArchenemy);
            helper.ConsoleCommands.Add("EOTV_changefriendship", "Changes the friendship of the NPC (first arg) by the amount given in the second arg", ChangeFriendship);
            helper.ConsoleCommands.Add("EOTV_maxedfriends", "Outputs Utility::getMaxedFriendshipPercent", MaxedFriendshipPercent);
            
            Event.RegisterPrecondition("EOTV_NegativeFriendship", EOTVPreconditions.NegativeFriendship);
            
            GameStateQuery.Register("EOTV_PLAYER_NPC_RELATIONSHIP", EOTVGameStateQueries.EotvPlayerNpcRelationship);
            GameStateQuery.Register("PLAYER_WEARING_HAT", EOTVGameStateQueries.PlayerWearingHat);
            
            NpcReceiveObjectApi.Instance.RegisterItemHandler(
                ModManifest,
                "(O)BarleyZP.EnemyOfTheValley.AvoidMeCake",
                NPCActionPatches.HandleCake);
            NpcReceiveObjectApi.Instance.RegisterItemHandler(
                ModManifest,
                "(O)BarleyZP.EnemyOfTheValley.ShatteredAmulet",
                NPCActionPatches.HandleShatteredAmulet);
            NpcReceiveObjectApi.Instance.RegisterItemHandler(
                ModManifest,
                "(O)BarleyZP.EnemyOfTheValley.ReconciliationDust",
                NPCActionPatches.HandleReconciliationDust);
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
            } else if (e.NameWithoutLocale.IsEquivalentTo("Data/Characters"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, CharacterData>().Data;
                    foreach (var (name, character) in data)
                    {
                        if (!OptedInNpcs.Contains(name)) continue;
                        character.CustomFields ??= new Dictionary<string, string>();
                        character.CustomFields.TryAdd("BarleyZP.EnemyOfTheValley.CanHaveNegativeFriendship", "");
                        character.CustomFields.TryAdd("BarleyZP.EnemyOfTheValley.CanBecomeEnemies", "");
                    }
                });
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
            foreach (var farmer in Game1.getAllFarmers())
            {
                List<string> toRemove = new();
                foreach (string key in farmer.mailReceived)
                {
                    if (key.StartsWith("apologyLetter_"))
                    {
                        var npcName = key.Replace("apologyLetter_", "");
                        Relationships.SetRelationship(npcName, farmer, FriendshipStatus.Friendly, true);
                        if (farmer.friendshipData.TryGetValue(npcName, out var friendship))
                        {
                            friendship.Points = Math.Max(-1250, friendship.Points);
                        }
                        var npc = Game1.getCharacterFromName(npcName);
                        var traverse = Traverse.Create(typeof(Game1)).Field("multiplayer");
                        traverse.GetValue<Multiplayer>().globalChatInfoMessage("Apologized", farmer.Name, npc?.GetTokenizedDisplayName() ?? "Unknown NPC");

                        farmer.activeDialogueEvents.TryAdd("BarleyZP.EnemyOfTheValley.apologized_" + npcName, 4);
                        toRemove.Add(key);
                    }
                }
            
                // make these repeatable
                farmer.mailReceived.ExceptWith(toRemove);
            }
        }

        private void OnDayEnding(object? sender, DayEndingEventArgs e)
        {
            // get rid of door unlock for NPCs that fell below 0 hearts
            foreach (var farmer in Game1.getAllFarmers())
            {
                foreach (string name in farmer.friendshipData.Keys)
                {
                    if (farmer.mailReceived.Contains("doorUnlock" + name) && farmer.friendshipData[name].Points < 0)
                    {
                        farmer.mailReceived.Remove("doorUnlock" + name);
                    }
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
            Relationships.SetRelationship(args[0], Game1.player, Relationships.Enemy, printValidation: true);
        }

        public static void SetArchenemy(string command, string[] args)
        {
            Relationships.SetRelationship(args[0], Game1.player, Relationships.Archenemy, printValidation: true);
        }

        public static void SetExArchenemy(string command, string[] args)
        {
            Relationships.SetRelationship(args[0], Game1.player, Relationships.ExArchenemy, printValidation: true);
        }

        public static void SetFriendly(string command, string[] args)
        {
            Relationships.SetRelationship(args[0], Game1.player, FriendshipStatus.Friendly, printValidation: true);
        }

        public static void ChangeFriendship(string command, string[] args)
        {
            if (!ArgUtility.TryGetInt(args, 1, out int amt, out string err))
            {
                StaticMonitor.Log(err, LogLevel.Error);
            }
            else
            {
                var npc = Game1.getCharacterFromName<NPC>(args[0]);
                if (!Game1.player.friendshipData.TryGetValue(args[0], out var friendship))
                {
                    StaticMonitor.Log("NPC not found in player's friendship data.", LogLevel.Error);
                }
                else
                {
                    int before = friendship.Points;
                    Game1.player.changeFriendship(amt, npc);
                    int after = friendship.Points;
                    
                    StaticMonitor.Log("Before: " + before + ". After: " + after, LogLevel.Info);
                }
            }
            
        }

        public static void MaxedFriendshipPercent(string command, string[] args)
        {
            StaticMonitor.Log(Utility.getMaxedFriendshipPercent().ToString(), LogLevel.Info);
        }
    }
}
