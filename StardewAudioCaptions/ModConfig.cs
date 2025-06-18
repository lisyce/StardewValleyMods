namespace StardewAudioCaptions;

public sealed class ModConfig
{
    public bool CaptionsOn { get; set; } = true;
    public float FontScaling { get; set; } = 1f;
    public int MaxVisibleCaptions { get; set; } = 4;
    public int MinDurationTicks { get; set; } = 100;
    public string CaptionPosition { get; set; } = "Top Left";
    public bool HideInMenu { get; set; } = true;
    public int CaptionOffsetX { get; set; } = 0;
    public int CaptionOffsetY { get; set; } = 0;
    public Dictionary<string, bool> CaptionToggles { get; set; } = new();
    public Dictionary<string, string> CategoryColors { get; set; } = new();
}