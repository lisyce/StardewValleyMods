using StardewModdingAPI;
using StardewValley;
using DefaultDialogueRule = Conversation_Topic_Utilities.TopicRule.DefaultDialogueRule;

namespace Conversation_Topic_Utilities;

public class Util
{
    public static Dialogue? TryGetDefaultCtDialogue(string conversationTopic, NPC npc)
    {
        if (npc.CurrentDialogue.Any(x => x.TranslationKey == conversationTopic))
        {
            return null;  // don't put duplicates on the stack
        }
        
        var data = Game1.content.Load<List<TopicRule>>(ModEntry.ASSET_NAME);
        
        if (TryGetTopicRule(data, conversationTopic, out var value)
            && TryGetNpcDefaultDialogue(value, npc, conversationTopic, out var dialogue))
        {
            return new Dialogue(npc, conversationTopic, dialogue);
        }

        return null;
    }
    
    public static bool TryGetTopicRule(List<TopicRule> data, string conversationTopic, out TopicRule value)
    {
        value = null;
        
        foreach (var topicRule in data)
        {
            if (conversationTopic == topicRule.Id ||
                (conversationTopic.StartsWith(topicRule.Id) && topicRule.IdIsPrefix))
            {
                value = topicRule;
                return true;
            }
        }

        return false;
    }

    public static bool ShouldRepeat(TopicRule rule, string conversationTopic)
    {
        return true; // TODO
    }
    
    private static bool TryGetNpcDefaultDialogue(TopicRule topicRule, NPC npc, string conversationTopic, out string dialogue)
    {
        foreach (var defaultDialogueRule in topicRule.DefaultDialogueRules)
        {
            if (DefaultDialogueRuleApplies(defaultDialogueRule, npc, conversationTopic))
            {
                dialogue = defaultDialogueRule.Id;
                return true;
            }
        }

        dialogue = null;
        return false;
    }

    private static bool DefaultDialogueRuleApplies(DefaultDialogueRule defaultDialogueRule, NPC npc, string conversationTopic)
    {
        foreach (var rule in defaultDialogueRule.Rules)
        {
            if (!TryParseDefaultDialogueRule(rule, npc, conversationTopic, out bool result)) {
                ModEntry.StaticMonitor.Log($"Could not parse default dialogue rule {rule} for default dialogue line {defaultDialogueRule.Id}", LogLevel.Warn);
                return false;
            }

            if (!result) return false;
        }
        return true;
    }

    private static bool TryParseDefaultDialogueRule(string rule, NPC npc, string conversationTopic, out bool result)
    {
        result = false;
        
        if (!rule.Contains(':')) return false;
        var query = rule.Split(":")[1].Trim();
        var type = rule.Split(":")[0].Trim();

        var validTypes = new HashSet<string>{ "GSQ", "TopicContains", "NpcIs" };
        if (!validTypes.Contains(type)) return false;

        var isCurrentNpc = false;
        if (conversationTopic.Contains("_memory_"))
        {
            if (conversationTopic.Split("_memory_")[0].EndsWith(npc.Name)) isCurrentNpc = true;
        }
        else
        {
            if (conversationTopic.EndsWith(npc.Name)) isCurrentNpc = true;
        }
        
        result = type switch
        {
            "GSQ" => GameStateQuery.CheckConditions(query),
            "TopicContains" => conversationTopic.Contains(query),
            "NpcIs" => query == "Current" ? isCurrentNpc : query.Split(",").ToList().Select(x => x.Trim()).Contains(npc.Name),
            _ => false
        };

        return true;
    }
}