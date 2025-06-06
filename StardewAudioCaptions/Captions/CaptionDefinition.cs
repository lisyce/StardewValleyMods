namespace StardewAudioCaptions.Captions;

/// <summary>
/// A <c>CaptionDefinition</c> is the value of a key/value pair in the <c>BarleyZP.Captions/Definitions</c> asset.
/// </summary>
public class CaptionDefinition
{
    public CaptionPriority Priority { get; set; } = CaptionPriority.Default;
    
    // only instantiated via JSON deserialization
    // not marked [JsonRequired] because this mod does not provide the Text field in the JSON but rather programmatically
    public string Text { get; set; } = null!;
}