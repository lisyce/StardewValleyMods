using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Quests;
using StardewValley.SpecialOrders;

namespace FailedQuestsLoseFriendship
{
    public class ModEntry : Mod
    {
        static IMonitor Monitor;
        static string UniqueID;
        public override void Entry(IModHelper helper)
        {
            Monitor = base.Monitor;
            UniqueID = ModManifest.UniqueID;

            helper.Events.GameLoop.DayEnding += OnDayEnding;
        }

        private void OnDayEnding(object? sender, DayEndingEventArgs e)
        {
            int failed = 0;
            foreach (Quest q in Game1.player.questLog)
            {
                if (q.IsTimedQuest() && q.daysLeft.Value <= 1 && !q.completed.Value)
                {
                    failed++;
                    string? topic = GetQuestConvoTopic(q);
                    if (topic != null)
                    {
                        ChangeFriendshipFailedQuest(GetQuestTarget(q));
                        Game1.player.activeDialogueEvents.TryAdd($"{UniqueID}_questFailed_{topic}", 1);
                    }
                }
            }

            foreach (SpecialOrder so in Game1.player.team.specialOrders)
            {
                if (so.questState.Value != SpecialOrderStatus.Complete && so.GetDaysLeft() <= 1) {
                    failed++;
                    ChangeFriendshipFailedQuest(so.requester.Value);
                    Game1.player.activeDialogueEvents.TryAdd($"{UniqueID}_questFailed_so_{so.questKey}", 1);
                }
            }

            AddFailedQuests(Game1.player, failed);
            RemoveConvoTopicsMail();

            List<int> mailAmounts = new() { 3, 10, 20, 50 };
            int failedQuests = GetFailedQuests(Game1.player);
            foreach (int amount in mailAmounts)
            {
                if (failedQuests >= amount && !Game1.player.mailReceived.Contains($"{UniqueID}{amount}"))
                {
                    Game1.player.mailForTomorrow.Add($"{UniqueID}{amount}");
                }
            }
        }

        private static void ChangeFriendshipFailedQuest(string name)
        {
            NPC npc = Game1.getCharacterFromName(name);
            if (npc == null) return;
            
            Game1.player.changeFriendship(-50, npc);
        }

        private static void RemoveConvoTopicsMail()
        {
            HashSet<string> toRemove = new();
            foreach (string mail in Game1.player.mailReceived)
            {
                if (mail.Contains($"{UniqueID}_questFailed_")) toRemove.Add(mail);
            }

            Game1.player.mailReceived.ExceptWith(toRemove);
        }

        private static string? GetQuestConvoTopic(Quest q)
        {
            if (q is ResourceCollectionQuest rcq)
            {
                return $"rc_{rcq.target.Value}";
            }
            else if (q is SlayMonsterQuest smq)
            {
                return $"sm_{smq.target.Value}";
            }
            else if (q is FishingQuest fq)
            {
                return $"f_{fq.target.Value}";
            }
            else if (q is ItemDeliveryQuest idq)
            {
                return $"id_{idq.target.Value}";
            }
            else if (q is SocializeQuest sq)
            {
                return "socialize";
            }

            return null;
        }
        
        private static string? GetQuestTarget(Quest q)
        {
            if (q is ResourceCollectionQuest rcq)
            {
                return rcq.target.Value;
            }
            else if (q is SlayMonsterQuest smq)
            {
                return smq.target.Value;
            }
            else if (q is FishingQuest fq)
            {
                return fq.target.Value;
            }
            else if (q is ItemDeliveryQuest idq)
            {
                return idq.target.Value;
            }
            else if (q is SocializeQuest sq)
            {
                return "Emily";
            }

            return null;
        }

        private static int GetFailedQuests(Farmer who)
        {
            return int.Parse(who.modData.GetValueOrDefault($"{UniqueID}.failedQuests", "0"));
        }

        private static void AddFailedQuests(Farmer who, int failed)
        {
            who.modData[$"{UniqueID}.failedQuests"] = (GetFailedQuests(who) + failed).ToString();
        }
    }
}
