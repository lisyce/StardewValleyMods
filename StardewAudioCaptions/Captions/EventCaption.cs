namespace StardewAudioCaptions.Captions;

/// <summary>
/// An <c>EventCaption</c> is a list entry in the value of a key/value pair in the <c>BarleyZP.Captions/Events</c> asset.
/// </summary>
public class EventCaption
{
    public string CueId { get; set; }
    public string CaptionId { get; set; }
    public EventCaptionCondition When { get; set; } = EventCaptionCondition.Always;
    public int WhenCount { get; set; } = -1;
}