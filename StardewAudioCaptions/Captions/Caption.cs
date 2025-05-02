namespace StardewAudioCaptions.Captions;

public class Caption
{
    public readonly string CueId;
    public readonly string CaptionId;
    public readonly bool ShouldLog;
    private readonly int? _duration;
    public int MaxDuration => _duration ?? CaptionManager.InfiniteDuration;
    public string CategoryId => CaptionId.Split(".")[0];

    public Caption(string cueId, string captionId, int? duration = null, bool shouldLog = true)
    {
        CueId = cueId;
        CaptionId = captionId;
        ShouldLog = shouldLog;
        _duration = duration;

        ModEntry.CaptionManager.ValidateCaption(this);
    }
}