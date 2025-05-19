using Newtonsoft.Json;

namespace Conversation_Topic_Utilities;


public class TopicRule
{
    public class DefaultDialogueRule {
        [JsonRequired]
        public string Id { get; set; }
        [JsonRequired]
        public string Dialogue { get; set; }
        public List<string> Rules = new();
    }

    [JsonRequired]
    public string Id { get; set; }  // required field
    public bool IdIsPrefix { get; set; } = false;
    public bool RepeatableOnExpire { get; set; } = false;
    public bool MemoriesRepeatableOnExpire { get; set; } = false;
    public List<DefaultDialogueRule> DefaultDialogueRules { get; set; } = new();
}