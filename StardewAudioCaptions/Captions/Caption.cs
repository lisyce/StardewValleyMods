namespace StardewAudioCaptions.Captions;

public class Caption
{
    public readonly string CueId;
    public readonly string CaptionId;
    public readonly bool ShouldLog;
    private readonly int? _maxDuration;
    public int MaxDuration => _maxDuration ?? CaptionManager.InfiniteDuration;
    public string CategoryId => CaptionId.Split(".")[0];
    public object? Tokens { get; private set; }

    public Caption(string cueId, string captionId, int? maxDuration = null, bool shouldLog = true, object? tokens = null)
    {
        CueId = cueId;
        CaptionId = captionId;
        ShouldLog = shouldLog;
        _maxDuration = maxDuration;
        Tokens = tokens;

        ModEntry.CaptionManager.ValidateCaption(this);
    }
}