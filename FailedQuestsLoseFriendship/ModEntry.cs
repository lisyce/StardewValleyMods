using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.SpecialOrders;
using StardewValley.Quests;
using StardewValley.SpecialOrders;

namespace FailedQuestsLoseFriendship
{
    public class ModEntry : Mod
    {
        public static string UniqueID;
        public Config Config;
        public bool SaveLoaded;
        public override void Entry(IModHelper helper)
        {
            UniqueID = ModManifest.UniqueID;
            Config = Helper.ReadConfig<Config>();
            SaveLoaded = false;

            helper.Events.GameLoop.DayEnding += OnDayEnding;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            helper.Events.Content.AssetsInvalidated += OnAssetsInvalidated;
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            SaveLoaded = true;
            UpdateGMCM();
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            UpdateGMCM();
        }

        private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            SaveLoaded = false;
            UpdateGMCM();
        }

        private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            foreach (var name in e.NamesWithoutLocale)
            {
                if (name.IsEquivalentTo("Data/SpecialOrders"))
                {
                    UpdateGMCM();
                    break;
                }
            }
        }

        private void OnDayEnding(object? sender, DayEndingEventArgs e)
        {
            int failed = 0;
            foreach (Quest q in Game1.player.questLog)
            {
                if (q.IsTimedQuest() && q.daysLeft.Value <= 1 && !q.completed.Value && QuestEnabled(q))
                {
                    failed++;
                    string? topic = GetQuestConvoTopic(q);
                    if (topic != null)
                    {
                        ChangeFriendshipFailedQuest(GetQuestTarget(q));
                        Game1.player.activeDialogueEvents.TryAdd($"{UniqueID}_questFailed_{topic}", 4);
                    }
                }
            }

            foreach (SpecialOrder so in Game1.player.team.specialOrders)
            {
                if (so.questState.Value != SpecialOrderStatus.Complete && so.GetDaysLeft() <= 1 && Config.SpecialOrdersEnabled && !Config.DisabledSpecialOrders.Contains(so.questKey.Value)) {
                    failed++;
                    ChangeFriendshipFailedQuest(so.requester.Value);
                    Game1.player.activeDialogueEvents.TryAdd($"{UniqueID}_questFailed_so_{so.questKey}", 4);
                }
            }

            AddFailedQuests(Game1.player, failed);

            // check if we need to send a letter from Lewis
            List<int> mailAmounts = new() { 3, 10, 20, 50 };
            int failedQuests = GetFailedQuests(Game1.player);
            foreach (int amount in mailAmounts)
            {
                if (failedQuests >= amount && !Game1.player.hasOrWillReceiveMail($"{UniqueID}{amount}"))
                {
                    Game1.player.mailForTomorrow.Add($"{UniqueID}{amount}");
                }
            }
        }
        
        private void ChangeFriendshipFailedQuest(string name)
        {
            NPC npc = Game1.getCharacterFromName(name);
            if (npc == null) return;
            
            Game1.player.changeFriendship(-1 * Config.FriendshipLost, npc);
        }
        
        private bool QuestEnabled(Quest quest)
        {
            return quest switch
            {
                ResourceCollectionQuest => Config.ResourceCollectionQuestsEnabled,
                SlayMonsterQuest => Config.SlayMonsterQuestsEnabled,
                FishingQuest => Config.FishingQuestsEnabled,
                ItemDeliveryQuest => Config.ItemDeliveryQuestsEnabled,
                SocializeQuest => Config.SocializeQuestsEnabled,
                _ => false
            };
        }

        private static string? GetQuestConvoTopic(Quest q)
        {
            return q switch
            {
                ResourceCollectionQuest rcq => $"rc_{rcq.target.Value}",
                SlayMonsterQuest smq => $"sm_{smq.target.Value}",
                FishingQuest fq => $"f_{fq.target.Value}",
                ItemDeliveryQuest idq => $"id_{idq.target.Value}",
                SocializeQuest => "socialize",
                _ => null
            };
        }
        
        private static string? GetQuestTarget(Quest q)
        {
            return q switch
            {
                ResourceCollectionQuest rcq => rcq.target.Value,
                SlayMonsterQuest smq => smq.target.Value,
                FishingQuest fq => fq.target.Value,
                ItemDeliveryQuest idq => idq.target.Value,
                SocializeQuest => "Emily",
                _ => null
            };
        }

        private int GetFailedQuests(Farmer who)
        {
            return int.Parse(who.modData.GetValueOrDefault($"{UniqueID}.failedQuests", "0"));
        }

        private void AddFailedQuests(Farmer who, int failed)
        {
            who.modData[$"{UniqueID}.failedQuests"] = (GetFailedQuests(who) + failed).ToString();
        }
        

        private void UpdateGMCM(bool specialOrders = false)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
            {
                Monitor.Log($"Failed to get GMCM API.", LogLevel.Warn);
                return;
            }

            try
            {
                configMenu.Unregister(ModManifest);
            }
            catch (Exception e)
            {
                Monitor.Log($"Failed to unregister this mod from GMCM: {e}", LogLevel.Debug);
            }
            
            configMenu.Register(
                mod: ModManifest,
                reset: () =>
                {
                    Config = new Config();
                },
                save: () => Helper.WriteConfig(Config)
            );
            
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("Config.GeneralSettings"));
            
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.Penalty"),
                getValue: () => Config.FriendshipLost,
                setValue: value => Config.FriendshipLost = value);
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.Collection"),
                getValue: () => Config.ResourceCollectionQuestsEnabled,
                setValue: value => Config.ResourceCollectionQuestsEnabled = value);
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.Slay"),
                getValue: () => Config.SlayMonsterQuestsEnabled,
                setValue: value => Config.SlayMonsterQuestsEnabled = value);
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.Fish"),
                getValue: () => Config.FishingQuestsEnabled,
                setValue: value => Config.FishingQuestsEnabled = value);
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.Delivery"),
                getValue: () => Config.ItemDeliveryQuestsEnabled,
                setValue: value => Config.ItemDeliveryQuestsEnabled = value);
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.Socialize"),
                getValue: () => Config.SocializeQuestsEnabled,
                setValue: value => Config.SocializeQuestsEnabled = value);
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.SO"),
                getValue: () => Config.SpecialOrdersEnabled,
                setValue: value => Config.SpecialOrdersEnabled = value);
            
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("Config.SOToggle"));

            if (SaveLoaded)
            {
                foreach (var orderData in Game1.content.Load<Dictionary<string, SpecialOrderData>>("Data/SpecialOrders"))
                {
                    SpecialOrder order = SpecialOrder.GetSpecialOrder(orderData.Key, null);
                
                    // filter out invalid special orders
                    // for mods like RSV, some of the patches to load to Strings/SpecialOrderStrings only happen when a save
                    // is loaded. Until then, their names/descs are just "Strings/SpecialOrderStrings/.....".
                    if (order == null ||
                        (orderData.Value.RequiredTags != null && orderData.Value.RequiredTags.Contains("NOT_IMPLEMENTED")) ||
                        order.GetName().StartsWith("Strings") || order.GetDescription().StartsWith("Strings")) continue;
                
                    configMenu.AddBoolOption(
                        mod: ModManifest,
                        name: () => order.GetName(),
                        tooltip: () => order.GetDescription(),
                        getValue: () => !Config.DisabledSpecialOrders.Contains(orderData.Key),
                        setValue: value =>
                        {
                            if (value) Config.DisabledSpecialOrders.Remove(orderData.Key);
                            else Config.DisabledSpecialOrders.Add(orderData.Key);
                        });
                }
            }
            else
            {
                configMenu.AddParagraph(
                    mod: ModManifest,
                    text: () => Helper.Translation.Get("Config.LoadSave"));
            }
            
        }
    }
}
