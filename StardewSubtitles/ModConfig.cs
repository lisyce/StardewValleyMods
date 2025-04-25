namespace StardewSubtitles;

public sealed class ModConfig
{
    public bool SubtitlesOn { get; set; } = true;
    public float FontScaling { get; set; } = 1f;
    public int MaxVisibleSubtitles { get; set; } = 6;
    public int DefaultDurationTicks { get; set; } = 80;
}