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
            if (split.Length < 2) continue;
            
            var toCheck = string.Join('_', split[1..]);
            
            // is the CT associated actually expiring tonight?
            if (Game1.player.activeDialogueEvents.TryGetValue(toCheck, out int daysLeft) && daysLeft >= 0) continue;  // CTs are removed when daysLeft < 0
            
            // find the matching key in data, if any
            foreach (var (k, v) in data)
            {
                if ((v.KeyIsPrefix && Util.PrefixKeyApplies(toCheck, k)) || toCheck == k)
                {
                    if (Util.ShouldRepeat(v, toCheck))
                    {
                        Monitor.Log($"Removing CT {topicKey} from mail flags since it is repeatable.", LogLevel.Debug);
                        toRemove.Add(topicKey);
                        break;
                    }
                }
            }
        }

        Game1.player.mailReceived.ExceptWith(toRemove);
    }
}