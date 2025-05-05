namespace StardewAudioCaptions;

public sealed class ModConfig
{
    public bool CaptionsOn { get; set; } = true;
    public float FontScaling { get; set; } = 1f;
    public int MaxVisibleCaptions { get; set; } = 6;
    public int MinDurationTicks { get; set; } = 120;
    public string SubtitlePosition { get; set; } = "Top Left";
    public int SubtitleOffsetX { get; set; } = 0;
    public int SubtitleOffsetY { get; set; } = 0;
    public Dictionary<string, bool> CaptionToggles { get; set; } = new();
    public Dictionary<string, string> CategoryColors { get; set; } = new();
}