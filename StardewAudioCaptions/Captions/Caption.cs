using System.Text.RegularExpressions;
using StardewModdingAPI;

namespace StardewAudioCaptions.Captions;

/// <summary>
/// A <c>Caption</c> is an audio description that can be displayed on the HUD.
/// </summary>
public class Caption
{
    public readonly string CueId;
    public readonly string CaptionId;
    public readonly CaptionPriority Priority;
    private readonly int? _maxDuration;
    public int MaxDuration => _maxDuration ?? CaptionManager.InfiniteDuration;
    public string CategoryId => CaptionId.Split(".")[0];
    private object? Tokens;

    public string Text
    {
        get
        {
            if (!ModEntry.Definitions.TryGetValue(CaptionId, out var capt)) return $"Caption {CaptionId} does not exist";
            return capt.TranslationKey != null
                ? ModEntry.ModHelper.Translation.Get(capt.TranslationKey, tokens: Tokens)
                : capt.Text;
        }
    }
    
    /// <param name="cueId">The ID of the sound cue</param>
    /// <param name="captionId">The ID of the caption to display</param>
    /// <param name="maxDuration">The maximum number of ticks the caption can display for, or <c>null</c> for default behavior</param>
    /// <param name="tokens">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.</param>
    public Caption(string cueId, string captionId, int? maxDuration = null, object? tokens = null)
    {
        CueId = cueId;
        CaptionId = captionId;
        _maxDuration = maxDuration;
        Tokens = tokens;
        Priority = ModEntry.ModCaptionManager.GetPriority(captionId);

        ModEntry.ModCaptionManager.ValidateCaption(this);
    }
}