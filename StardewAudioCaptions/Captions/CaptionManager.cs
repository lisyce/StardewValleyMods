using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;

namespace StardewAudioCaptions.Captions;

/// <summary>
/// The <c>CaptionManager</c> is responsible for registering captions to be displayed in the HUD when sounds play.
/// </summary>
public class CaptionManager
{
    public const int InfiniteDuration = int.MaxValue;
    public const string AnyCue = "";

    public static readonly Dictionary<string, Color> AllowedColors = new()
    {
        { "White", Color.White },
        { "Gray", Color.DarkGray },
        { "Yellow", Color.Yellow },
        { "Cyan", Color.DarkTurquoise },
        { "Pink", Color.HotPink },
        { "Green", Color.LimeGreen }
    };
    
    private readonly CaptionHudMessage _hudMessage;
    private readonly IMonitor _monitor;
    private readonly Dictionary<string, List<Caption>> _captionsOnNextCue;
    private readonly Dictionary<string, Caption> _defaultCueCaptions;
    
    public ModConfig Config { get; set; }
    
    public CaptionManager(CaptionHudMessage hudMessage, IMonitor monitor, ModConfig config)
    {
        _hudMessage = hudMessage;
        _monitor = monitor;
        Config = config;
        _captionsOnNextCue = new Dictionary<string, List<Caption>>();
        _defaultCueCaptions = new Dictionary<string, Caption>();
    }

    /// <summary>
    /// Adds captions to the HUD for the sound cue if any are registered.
    /// </summary>
    /// <param name="cue">The sound cue</param>
    public void OnSoundPlayed(ICue cue)
    {
        var cueId = cue.Name;
        //_monitor.Log(cueId, LogLevel.Debug);
        
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
        
        // captions raised for the next time ANY sound plays
        if (_captionsOnNextCue.TryGetValue(AnyCue, out captions))
        {
            foreach (var caption2 in captions)
            {
                AddCaption(cue, caption2);
            }

            var captionsCopy = new List<Caption>(captions);
            foreach (var caption2 in captionsCopy)
            {
                UnregisterCaptionForNextCue(caption2);
            }
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
        if (!ValidateCaption(caption)) return;
        if (!_captionsOnNextCue.ContainsKey(caption.CueId)) _captionsOnNextCue.Add(caption.CueId, new List<Caption>());

        _captionsOnNextCue[caption.CueId].Add(caption);
    }
    
    /// <summary>
    /// Removes a caption for the next sound cue that was added by <see cref="RegisterCaptionForNextCue"/>.
    /// Usually used with a prefix/postfix pair to avoid a transpiler.
    /// </summary>
    /// <param name="caption">The caption to unregister</param>
    public void UnregisterCaptionForNextCue(Caption caption)
    {
        if (_captionsOnNextCue.TryGetValue(caption.CueId, out var captions))
        {
            captions.RemoveWhere(x => x.CaptionId == caption.CaptionId);
            if (captions.Count == 0) _captionsOnNextCue.Remove(caption.CueId);
        }
    }

    /// <summary>
    /// Registers a persistent default caption for a sound cue. Will only be used if a more specific caption is not found.
    /// </summary>
    /// <param name="caption">The caption to show</param>
    public void RegisterDefaultCaption(Caption caption)
    {
        if (!ValidateCaption(caption)) return;
        if (!_defaultCueCaptions.TryAdd(caption.CueId, caption))
        {
            _monitor.Log($"Failed to register default caption {caption.CaptionId} for sound cue {caption.CueId} because a default caption {_defaultCueCaptions[caption.CueId]} already exists.", LogLevel.Warn);
        }
    }
    
    /// <returns>A mapping of category Ids to captions Ids in each category.</returns>
    public Dictionary<string, HashSet<string>> CaptionsByCategory()
    {
        var result = new Dictionary<string, HashSet<string>>();
        foreach (var caption in ModEntry.Definitions.Keys)
        {
            var category = caption.Split(".")[0];
            if (!result.ContainsKey(category)) result.Add(category, new HashSet<string>());
            result[category].Add(caption);
        }
        
        return result;
    }

    public CaptionPriority GetPriority(string captionId)
    {
        if (!ModEntry.Definitions.ContainsKey(captionId)) return CaptionPriority.Default;
        return ModEntry.Definitions[captionId].Priority;
    }

    public bool ValidateCaption(Caption caption)
    {
        var captionId = caption.CaptionId;
        if (!ModEntry.Definitions.ContainsKey(captionId))
        {
            _monitor.Log($"Invalid caption id: {captionId}", LogLevel.Warn);
            return false;
        }
        
        if (caption.CueId != AnyCue && !Game1.soundBank.Exists(caption.CueId))
        {
            _monitor.Log($"Invalid sound cue id: {caption.CueId} for caption {captionId}", LogLevel.Warn);
            return false;
        }

        return true;
    }
    
    private void AddCaption(ICue cue, Caption caption)
    {
        var captionId = caption.CaptionId;
        if (!Config.CaptionToggles.GetValueOrDefault(captionId, true) && !ModEntry.EventCaptionManager.Value.EventInProgress()) return;
        
        _hudMessage.AddCaption(cue, caption, CaptionColor(caption));
    }

    private Color CaptionColor(Caption caption)
    {
        var colorStr = Config.CategoryColors.GetValueOrDefault(caption.CategoryId, "White");
        return AllowedColors[colorStr];
    }
}