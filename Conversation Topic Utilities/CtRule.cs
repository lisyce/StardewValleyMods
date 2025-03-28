namespace Conversation_Topic_Utilities;

public class CtRule
{
    public bool KeyIsPrefix { get; set; } = false;  // whether the key in the dictionary is a prefix for many possible matching CTs, e.g. "dating_" is a prefix for "dating_Abigail".
    public Dictionary<string, List<string>> Defaults { get; set; } = new();
    public bool Repeatable { get; set; } = false;
    public bool MemoriesRepeatable { get; set; } = false;
}