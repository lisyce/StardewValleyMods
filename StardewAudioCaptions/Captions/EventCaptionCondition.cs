namespace StardewAudioCaptions.Captions;

/// <summary>
/// <c>EventCaptionCondition</c> describes when a caption should display in an event.
/// </summary>
public enum EventCaptionCondition
{
    /// The caption should display every time the cue with the appropriate ID plays.
    Always,
    
    /// The caption should be displayed the first N times the cue with the appropriate ID plays.
    FirstN,
    
    /// The caption should be displayed every time the cue plays after the cue has played N times. 
    AfterN
}