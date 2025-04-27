namespace StardewAudioCaptions.Captions;

public class Caption
{
    public readonly string CueId;
    public readonly string CaptionId;
    private readonly int? _duration;
    public int MaxDuration => _duration ?? CaptionManager.InfiniteDuration;

    public Caption(string cueId, string captionId, int? duration = null)
    {
        CueId = cueId;
        CaptionId = captionId;
        _duration = duration;
    }
}