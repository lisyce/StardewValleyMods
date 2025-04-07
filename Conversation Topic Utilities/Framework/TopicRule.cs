namespace Conversation_Topic_Utilities;

public class TopicRule
{
    public class DefaultDialogueRule {
        public string Id { get; set; }
        public List<string> Rules = new();
    }

    public string Id { get; set; }
    public bool IdIsPrefix { get; set; } = false;
    public bool RepeatableOnExpire { get; set; } = false;
    public bool MemoriesRepeatableOnExpire { get; set; } = false;
    public List<DefaultDialogueRule> DefaultDialogueRules { get; set; } = new();
}