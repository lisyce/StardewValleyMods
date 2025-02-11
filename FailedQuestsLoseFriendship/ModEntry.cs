using HarmonyLib;
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
                if (q.IsTimedQuest() && q.daysLeft.Value <= 1 && !q.completed.Value) failed++;
            }

            foreach (SpecialOrder so in Game1.player.team.specialOrders)
            {
                if (so.questState.Value != SpecialOrderStatus.Complete && so.GetDaysLeft() <= 1) failed++;
            }


            AddFailedQuests(Game1.player, failed);
            // Monitor.Log(failed.ToString(), LogLevel.Debug);
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
