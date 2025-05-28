using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Characters;
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
                (topicRule.IdIsPrefix && !conversationTopic.Contains("_memory_") && conversationTopic.StartsWith(topicRule.Id)))
            {
                value = topicRule;
                return true;
            }
        }

        return false;
    }
    

    public static bool ShouldRepeat(TopicRule rule, string conversationTopic)
    {
        if (conversationTopic.Contains("_memory_"))
        {
            // if it's a _memory_ CT, it has to be an *exact* match for the rule's Id to use RepeatableOnExpire.
            return rule.Id == conversationTopic ? rule.RepeatableOnExpire : rule.MemoriesRepeatableOnExpire;
        }
        return rule.RepeatableOnExpire;
    }

    public static string StripMemory(string conversationTopic)
    {
        if (conversationTopic.Contains("_memory_"))  // vanilla checks this string, so we can hardcode it too.
        {
            var removeFrom = conversationTopic.IndexOf("_memory_", StringComparison.Ordinal);
            return conversationTopic.Remove(removeFrom);
        }

        return conversationTopic;
    }
    
    private static bool TryGetNpcDefaultDialogue(TopicRule topicRule, NPC npc, string conversationTopic, out string dialogue)
    {
        foreach (var defaultDialogueRule in topicRule.DefaultDialogueRules)
        {
            ModEntry.StaticMonitor.Log($"Testing if default dialogue line \"{defaultDialogueRule.Id}\" applies...");
            if (DefaultDialogueRuleApplies(defaultDialogueRule, npc, conversationTopic))
            {
                dialogue = defaultDialogueRule.Dialogue;
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
                ModEntry.StaticMonitor.Log($"Could not parse default dialogue rule \"{rule}\" for default dialogue line \"{defaultDialogueRule.Id}\"", LogLevel.Warn);
                return false;
            }

            if (!result)
            {
                ModEntry.StaticMonitor.Log($"Rule \"{rule}\" is false.");
                return false;
            }
        }
        return true;
    }

    private static bool TryParseDefaultDialogueRule(string rule, NPC npc, string conversationTopic, out bool result)
    {
        result = false;
        
        if (!rule.Contains(':')) return false;
        var query = rule.Split(":")[1].Trim().Replace("%CurrentNPC%", npc.Name);
        var type = rule.Split(":")[0].Trim();
        
        result = type switch
        {
            "GSQ" => GameStateQuery.CheckConditions(query),
            "TopicContains" => conversationTopic.Contains(query),
            "ForNPC" => query == "ANY" || query.Split(",").ToList().Select(x => x.Trim()).Contains(npc.Name),
            "CurrentNpcManner" => Utility.TryParseEnum<NpcManner>(query, out var manners) && npc.Manners == (int) manners,
            _ => false
        };

        return true;
    }
}