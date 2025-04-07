using StardewModdingAPI;
using StardewValley;

namespace Conversation_Topic_Utilities;

public class Util
{
    public static Dialogue? TryGetDefaultCtDialogue(string topicKey, NPC npc)
    {
        if (npc.CurrentDialogue.Any(x => x.TranslationKey == topicKey))
        {
            return null;  // don't put duplicates on the stack
        }
        
        var data = Game1.content.Load<Dictionary<string, CtRule>>(ModEntry.ASSET_NAME);
        
        if (TryGetRule(data, topicKey, out var value) && TryGetNpcDefaultDialogue(topicKey, value, npc, out var dialogue))
        {
            return new Dialogue(npc, topicKey, dialogue);
        }

        return null;
    }

    public static bool PrefixKeyApplies(string topicKey, string prefix)
    {
        var memoryCheck = !topicKey.Contains("_memory_") || prefix.Contains("_memory_");
        return topicKey.StartsWith(prefix) && memoryCheck;
    }

    public static bool ShouldRepeat(CtRule rule, string topic)
    {
        return topic.Contains("_memory_") ? rule.MemoriesRepeatable : rule.Repeatable;
    }

    private static bool TryGetRule(Dictionary<string, CtRule> data, string topicKey, out CtRule value)
    {
        foreach (var (k, v) in data)
        {
            if (v.KeyIsPrefix && PrefixKeyApplies(topicKey, k))
            {
                value = v;
                return true;
            }
        }

        foreach (var (k, v) in data)
        {
            if (k == topicKey)
            {
                value = v;
                return true;
            }
        }

        value = null;
        return false;
    }

    // uses the first matching rule
    private static bool TryGetNpcDefaultDialogue(string topicKey, CtRule rule, NPC npc, out string dialogue)
    {
        foreach (var (defaultResponse, conditions) in rule.Defaults)
        {
            if (DefaultApplies(topicKey, conditions, npc))
            {
                dialogue = defaultResponse;
                return true;
            }
        }

        dialogue = null;
        return false;
    }

    private static bool DefaultApplies(string topicKey, List<string> conditions, NPC npc)
    {
        foreach (var condition in conditions)
        {
            if (!TryParseCondition(condition, npc, topicKey, out bool result)) {
                ModEntry.StaticMonitor.Log($"Could not parse condition {condition}", LogLevel.Warn);
                return false;
            }

            if (!result) return false;
        }
        return true;
    }

    private static bool TryParseCondition(string condition, NPC npc, string topicKey, out bool result)
    {
        result = false;
        
        if (!condition.Contains(':')) return false;
        var query = condition.Split(":")[1].Trim();
        var type = condition.Split(":")[0].Trim();

        var validTypes = new HashSet<string>{ "GSQ", "TopicContains", "NpcIs" };
        if (!validTypes.Contains(type)) return false;

        var isCurrentNpc = false;
        if (topicKey.Contains("_memory_"))
        {
            if (topicKey.Split("_memory_")[0].EndsWith(npc.Name)) isCurrentNpc = true;
        }
        else
        {
            if (topicKey.EndsWith(npc.Name)) isCurrentNpc = true;
        }
        
        result = type switch
        {
            "GSQ" => GameStateQuery.CheckConditions(query),
            "TopicContains" => topicKey.Contains(query),
            "NpcIs" => query == "Current" ? isCurrentNpc : query.Split(",").ToList().Select(x => x.Trim()).Contains(npc.Name)
        };

        return true;
    }
}