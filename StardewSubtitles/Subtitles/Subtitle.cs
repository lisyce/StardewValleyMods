namespace StardewSubtitles.Subtitles;

public class Subtitle
{
    public readonly string CueId;
    public readonly string SubtitleId;
    private readonly int? _duration;
    public int Duration => _duration ?? SubtitleManager.DefaultDurationTicks;

    public Subtitle(string cueId, string subtitleId, int? duration = null)
    {
        CueId = cueId;
        SubtitleId = subtitleId;
        _duration = duration;
    }
}