namespace StardewAudioCaptions;

public interface IStardewAudioCaptionsApi
{
    /// <summary>
    /// The next time the appropriate cue plays, adds the caption to the HUD.
    /// This caption will override any default captions for the sound cue. It will automatically be unregistered the next time the cue plays.
    /// </summary>
    /// <param name="cueId">The ID of the sound cue</param>
    /// <param name="captionId">The ID of the caption to display</param>
    /// <param name="maxDuration">The maximum number of ticks the caption can display for, or <c>null</c> for default behavior</param>
    /// <param name="tokens">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.</param>
    public void RegisterCaptionForNextCue(string cueId, string captionId, int? maxDuration = null, object? tokens = null);
    
    /// <summary>
    /// Registers a persistent default caption for a sound cue. Will only be used if a more specific caption is not found.
    /// </summary>
    /// <param name="cueId">The ID of the sound cue</param>
    /// <param name="captionId">The ID of the caption to display</param>
    /// <param name="maxDuration">The maximum number of ticks the caption can display for, or <c>null</c> for default behavior</param>
    /// <param name="tokens">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.</param>
    public void RegisterDefaultCaption(string cueId, string captionId, int? maxDuration = null, object? tokens = null);

    /// <summary>
    /// Removes a specific caption for the next sound cue that was added by <see cref="RegisterCaptionForNextCue"/>. Does not remove default captions.
    /// </summary>
    /// <param name="cueId">The ID of the sound cue</param>
    /// <param name="captionId">The ID of the caption to display</param>
    public void UnregisterCaptionForNextCue(string cueId, string captionId);
}