namespace StardewSubtitles.Subtitles;

public class Subtitle
{
    public readonly string CueId;
    public readonly string SubtitleId;
    private readonly int? _duration;
    public int MaxDuration => _duration ?? SubtitleManager.InfiniteDuration;

    public Subtitle(string cueId, string subtitleId, int? duration = null)
    {
        CueId = cueId;
        SubtitleId = subtitleId;
        _duration = duration;
    }
}