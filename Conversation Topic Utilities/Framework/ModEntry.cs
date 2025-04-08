using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Conversation_Topic_Utilities;

public class ModEntry : Mod
{
    public static readonly string ASSET_NAME = "BarleyZP.CTU/TopicRules";
    public static IMonitor StaticMonitor;
    
    public override void Entry(IModHelper helper)
    {
        StaticMonitor = Monitor;
        helper.Events.Content.AssetRequested += OnAssetRequested;
        helper.Events.GameLoop.DayEnding += OnDayEnding;
        
        var harmony = new Harmony(ModManifest.UniqueID);
        Patches.Patch(harmony);

        helper.ConsoleCommands.Add("CTU_ListActive", "Lists the current player's active CTs.", ListActiveTopics);
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(ASSET_NAME))
        {
            e.LoadFrom(() => new List<TopicRule>(), AssetLoadPriority.Medium);
        }
    }

    private void OnDayEnding(object? sender, DayEndingEventArgs e)
    {
        var data = Game1.content.Load<List<TopicRule>>(ASSET_NAME);
        
        HashSet<string> toRemove = new();
        
        foreach (string topicKey in Game1.player.mailReceived)
        {
            // CT mail keys start with NPC names. skip others
            var split = topicKey.Split("_");
            if (split.Length < 2 || Game1.getCharacterFromName(split[0]) == null) continue;
            
            var toCheck = string.Join('_', split[1..]);
            var memoryStripped = Util.StripMemory(toCheck);
            
            // is the associated CT actually expiring tonight?
            if (Game1.player.activeDialogueEvents.TryGetValue(toCheck, out int daysLeft) && daysLeft > 0) continue;  // CTs are removed when daysLeft < 0. this event fires BEFORE the counter is decremented by the game.
            
            // find the matching key in data, if any
            if ((Util.TryGetTopicRule(data, toCheck, out TopicRule rule) && Util.ShouldRepeat(rule, toCheck)) ||
                (Util.TryGetTopicRule(data, memoryStripped, out rule) && Util.ShouldRepeat(rule, memoryStripped)))
            {
                Monitor.Log($"Removing CT \"{topicKey}\" from mail flags since it is repeatable. (Rule: \"{rule.Id}\")", LogLevel.Info);
                toRemove.Add(topicKey);
                Game1.player.previousActiveDialogueEvents.Remove(toCheck);
            }
            else
            {
                Monitor.Log($"No matching topic rule for mail flag {topicKey} (CT: {toCheck})", LogLevel.Debug);
            }
        }

        Game1.player.mailReceived.ExceptWith(toRemove);
    }

    private void ListActiveTopics(string command, string[] args)
    {
        foreach (var pair in Game1.player.activeDialogueEvents.Pairs.OrderBy(x => x.Key))
        {
            StaticMonitor.Log($"{pair.Key}: {pair.Value}", LogLevel.Debug);
        }
    }
}