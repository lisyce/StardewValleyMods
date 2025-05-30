namespace StardewAudioCaptions.Captions;

public class EventCaption
{
    public string CueId { get; set; }
    public string CaptionId { get; set; }
    public EventCaptionCondition When { get; set; } = EventCaptionCondition.Always;
    public int WhenCount { get; set; } = -1;
}