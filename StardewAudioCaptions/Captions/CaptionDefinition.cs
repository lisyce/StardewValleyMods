namespace StardewAudioCaptions.Captions;

/// <summary>
/// A <c>CaptionDefinition</c> is the value of a key/value pair in the <c>BarleyZP.Captions/Definitions</c> asset.
/// </summary>
public class CaptionDefinition
{
    public CaptionPriority Priority { get; set; } = CaptionPriority.Default;
    public string Text { get; set; }
}