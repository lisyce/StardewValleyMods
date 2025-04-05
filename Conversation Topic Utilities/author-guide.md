# Author Guide

This guide describes how to use Conversation Topic Utilities (CTU). In this guide, "Conversation Topic" is abbreviated as "CT".

## The Asset

Other mods can interface with CTU by editing the asset it exposes (normally, this would be done through Content Patcher
or by using the C# asset editing events). This asset has the name `BarleyZP.CTU/TopicRules`. It's a mapping of string keys
to object values, the format of which is described below.

### Fields

| Field | Description | Default Value |
| ----- | ----------- | ------------- |
| `KeyIsPrefix` | Whether the key for this value is a prefix that may match many CTs. For example, "somePrefixKey_" is a prefix that matches the CTs "somePrefixKey_" and "somePrefixKey_Abigail" but *not* "Abigail_somePrefixKey_". | `false` |
| `RepeatableOnExpire` | Whether to immediately mark all matching CTs as repeatable (by clearing the associated mail flags) on the night they expire. | `false` |
| `MemoriesRepeatableOnExpire` | Whether to immediately mark all matching CTs' *memories* as repeatable on the night they expire. (e.g. "someKey_memory_oneweek" is a memory for the key "someKey"). | `false` |
| `DefaultDialogueRules` | A mapping of string keys to lists of string values, describing default dialogue lines for matching CTs and the conditions under which they appear. More details can be found in [[#Default Dialogue Rules]] | `{}` |

### Default Dialogue Rules
