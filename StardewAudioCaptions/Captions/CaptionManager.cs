using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace StardewAudioCaptions.Captions;

public class CaptionManager
{
    public static int MinDurationTicks { get; set; }
    public static readonly int InfiniteDuration = int.MaxValue;
    
    private readonly HashSet<string> _captionIds;
    private readonly CaptionHudMessage _hudMessage;
    private readonly IMonitor _monitor;
    private readonly IModHelper _helper;
    private readonly Dictionary<string, List<Caption>> _captionsOnNextCue;
    private readonly Dictionary<string, Caption> _defaultCueCaptions;
    
    public ModConfig Config { get; set; }
    
    public CaptionManager(IModHelper helper, CaptionHudMessage hudMessage, IMonitor monitor, ModConfig config)
    {
        _captionIds = helper.ModContent.Load<List<string>>("assets/captions.json").ToHashSet();
        _hudMessage = hudMessage;
        _monitor = monitor;
        _helper = helper;
        Config = config;
        _captionsOnNextCue = new Dictionary<string, List<Caption>>();
        _defaultCueCaptions = new Dictionary<string, Caption>();
        MinDurationTicks = config.MinDurationTicks;
    }

    /// <summary>
    /// Adds captions to the HUD for the sound cue if any are registered.
    /// </summary>
    /// <param name="cue">The sound cue</param>
    public void OnSoundPlayed(Cue cue)
    {
        var cueId = cue.Name;
        _monitor.Log(cueId, LogLevel.Debug);
        
        // do we have any overrides?
        if (_captionsOnNextCue.TryGetValue(cueId, out var captions))
        {
            foreach (var caption in captions)
            {
                AddCaption(cue, caption);
            }

            var captionsCopy = new List<Caption>(captions);
            foreach (var caption in captionsCopy)
            {
                UnregisterCaptionForNextCue(caption);
            }

        }
        else if (_defaultCueCaptions.TryGetValue(cueId, out var caption))
        {
            // default caption
            AddCaption(cue, caption);
            UnregisterCaptionForNextCue(caption);
        }
    }
    
    /// <summary>
    /// The next time the appropriate cue plays, adds the caption to the HUD.
    /// Does not persist beyond the very next time the cue is played. This caption will override any default
    /// captions for the sound cue.
    /// </summary>
    /// <param name="caption">The caption to show</param>
    public void RegisterCaptionForNextCue(Caption caption)
    {
        if (!ValidateCaption(caption.CaptionId)) return;
        if (!_captionsOnNextCue.ContainsKey(caption.CueId)) _captionsOnNextCue.Add(caption.CueId, new List<Caption>());

        _captionsOnNextCue[caption.CueId].Add(caption);

        if (caption.ShouldLog)
        {
            _monitor.Log($"Registered caption {caption.CaptionId} for next cue {caption.CueId}", LogLevel.Debug);
        }
    }
    
    /// <summary>
    /// Removes a caption for the next sound cue that was added by <see cref="RegisterCaptionForNextCue"/>.
    /// Usually used with a prefix/postfix pair to avoid a transpiler.
    /// </summary>
    /// <param name="cueId"></param>
    /// <param name="captionId"></param>
    public void UnregisterCaptionForNextCue(Caption caption)
    {
        if (_captionsOnNextCue.TryGetValue(caption.CueId, out var captions))
        {
            captions.RemoveWhere(x => x.CaptionId == caption.CaptionId);
            if (captions.Count == 0) _captionsOnNextCue.Remove(caption.CueId);
            if (caption.ShouldLog)
            {
                _monitor.Log($"Unregistered caption {caption.CaptionId} for cue {caption.CueId}", LogLevel.Debug);
            }
        }
    }

    /// <summary>
    /// Registers a persistent default caption for a sound cue.
    /// </summary>
    /// <param name="caption">The caption to show</param>
    public void RegisterDefaultCaption(Caption caption)
    {
        if (!ValidateCaption(caption.CaptionId)) return;
        if (!_defaultCueCaptions.TryAdd(caption.CueId, caption))
        {
            _monitor.Log($"Failed to register default caption {caption.CaptionId} for sound cue {caption.CueId} because a default caption {_defaultCueCaptions[caption.CueId]} already exists.", LogLevel.Warn);
        }
    }

    public Dictionary<string, HashSet<string>> CaptionsByCategory()
    {
        var result = new Dictionary<string, HashSet<string>>();
        foreach (var caption in _captionIds)
        {
            var category = caption.Split(".")[0];
            if (!result.ContainsKey(category)) result.Add(category, new HashSet<string>());
            result[category].Add(caption);
        }
        
        return result;
    }

    private void AddCaption(Cue cue, Caption caption)
    {
        var captionId = caption.CaptionId;
        if (!Config.CaptionToggles.GetValueOrDefault(captionId, true)) return;
        
        var captionTranslationKey = captionId + ".caption";
        var translatedCaption = _helper.Translation.Get(captionTranslationKey);
        _hudMessage.AddCaption(cue, translatedCaption, caption.MaxDuration);
    }

    private bool ValidateCaption(string captionId)
    {
        if (!_captionIds.Contains(captionId))
        {
            _monitor.Log($"Invalid caption id: {captionId}", LogLevel.Warn);
            return false;
        }

        var captionTranslationKey = captionId + ".caption";
        if (!_helper.Translation.ContainsKey(captionTranslationKey))
        {
            _monitor.Log($"No translation found for caption id: {captionId}. Translation key: {captionTranslationKey}");
            return false;
        }

        return true;
    }
}