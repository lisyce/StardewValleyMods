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
                    string? target = GetQuestTarget(q);
                    if (target != null)
                    {
                        Game1.player.activeDialogueEvents.TryAdd($"{UniqueID}_questFailed_{target}", 4);
                        Monitor.Log($"{UniqueID}_questFailed_{target}", LogLevel.Debug);
                    }
                }
            }

            foreach (SpecialOrder so in Game1.player.team.specialOrders)
            {
                if (so.questState.Value != SpecialOrderStatus.Complete && so.GetDaysLeft() <= 1) {
                    failed++;
                    Game1.player.activeDialogueEvents.TryAdd($"{UniqueID}_questFailed_{so.requester.Value}", 4);
                    Monitor.Log($"{UniqueID}_questFailed_{so.requester.Value}", LogLevel.Debug);
                }
            }

            AddFailedQuests(Game1.player, failed);
            RemoveConvoTopicsMail();
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
