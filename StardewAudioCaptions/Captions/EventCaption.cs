using Newtonsoft.Json;

namespace StardewAudioCaptions.Captions;

/// <summary>
/// An <c>EventCaption</c> is a list entry in the value of a key/value pair in the <c>BarleyZP.Captions/Events</c> asset.
/// </summary>
public class EventCaption
{
    [JsonRequired]
    public string CueId { get; set; } = null!;  // only instantiated via JSON deserialization
    [JsonRequired]
    public string CaptionId { get; set; } = null!;  // only instantiated via JSON deserialization
    public EventCaptionCondition When { get; set; } = EventCaptionCondition.Always;
    public int WhenCount { get; set; } = -1;
}