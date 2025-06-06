namespace StardewAudioCaptions.Captions;

public class CaptionDefinition
{
    public CaptionPriority Priority { get; set; } = CaptionPriority.Default;
    public string Text { get; set; }
}