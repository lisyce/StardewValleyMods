namespace StardewAudioCaptions.Captions;

/// <summary>
/// <c>CaptionPriority</c> defines how important it is that users see a particular <see cref="Caption"/>.
/// Captions with higher priorities may be displayed instead of captions with lower priorities.
/// </summary>
public enum CaptionPriority
{
    Background = -10,
    Low = -5,
    Default = 0,
    Medium = 5,
    High = 10
}