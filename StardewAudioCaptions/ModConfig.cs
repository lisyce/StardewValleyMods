namespace StardewAudioCaptions;

public sealed class ModConfig
{
    public bool CaptionsOn { get; set; } = true;
    public float FontScaling { get; set; } = 1f;
    public int MaxVisibleCaptions { get; set; } = 6;
    public int MinDurationTicks { get; set; } = 80;
    public Dictionary<string, bool> CaptionToggles { get; set; } = new();
}