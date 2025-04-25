using StardewModdingAPI;

namespace StardewSubtitles.Subtitles;

public class SubtitleManager
{
    private readonly HashSet<string> _subtitleIds;
    private readonly SubtitleHUDMessage _hudMessage;
    private readonly IMonitor _monitor;
    private readonly IModHelper _helper;
    private readonly Dictionary<string, List<string>> _subtitlesOnNextCue;
    private readonly Dictionary<string, string> _defaultCueSubtitles;
    
    public ModConfig Config { get; set; }
    
    public SubtitleManager(IModHelper helper, SubtitleHUDMessage hudMessage, IMonitor monitor, ModConfig config)
    {
        _subtitleIds = helper.ModContent.Load<List<string>>("assets/subtitles.json").ToHashSet();
        _hudMessage = hudMessage;
        _monitor = monitor;
        _helper = helper;
        Config = config;
        _subtitlesOnNextCue = new Dictionary<string, List<string>>();
        _defaultCueSubtitles = new Dictionary<string, string>();
    }

    /// <summary>
    /// Adds subtitles to the HUD for the sound cue if any are registered.
    /// </summary>
    /// <param name="cueId">The sound cue from the game's sound bank</param>
    public void OnSoundPlayed(string cueId)
    {
        // do we have any overrides?
        if (_subtitlesOnNextCue.TryGetValue(cueId, out var subtitles))
        {
            foreach (var subtitle in subtitles)
            {
                AddSubtitle(subtitle);
            }

            _subtitlesOnNextCue.Remove(cueId);
        }
        else if (_defaultCueSubtitles.TryGetValue(cueId, out var subtitle))
        {
            // default subtitle
            AddSubtitle(subtitle);
        }
    }
    
    /// <summary>
    /// The next time the sound cue with Id cueId plays, adds the subtitle with Id subtitleId to the HUD.
    /// Does not persist beyond the very next time the cue is played. This subtitle will override any default
    /// subtitles for the sound cue.
    /// </summary>
    /// <param name="cueId">The sound cue from the game's sound bank</param>
    /// <param name="subtitleId">The Id of the subtitle to show</param>
    public void RegisterSubtitleForNextCue(string cueId, string subtitleId)
    {
        if (!_subtitlesOnNextCue.ContainsKey(cueId)) _subtitlesOnNextCue.Add(cueId, new List<string>());
        
        _subtitlesOnNextCue[cueId].Add(subtitleId);
    }

    /// <summary>
    /// Registers a persistent default subtitle for a sound cue.
    /// </summary>
    /// <param name="cueId">The sound cue from the game's sound bank</param>
    /// <param name="subtitleId">The Id of the subtitle to show</param>
    public void RegisterDefaultSubtitle(string cueId, string subtitleId)
    {
        if (!_defaultCueSubtitles.TryAdd(cueId, subtitleId))
        {
            _monitor.Log($"Failed to register default subtitle {subtitleId} for sound cue {cueId} because a default subtitle {_defaultCueSubtitles[cueId]} already exists.", LogLevel.Warn);
        }
    }

    public Dictionary<string, HashSet<string>> SubtitlesByCategory()
    {
        var result = new Dictionary<string, HashSet<string>>();
        foreach (var subtitle in _subtitleIds)
        {
            var category = subtitle.Split(".")[0];
            if (!result.ContainsKey(category)) result.Add(category, new HashSet<string>());
            result[category].Add(subtitle);
        }
        
        return result;
    }

    private void AddSubtitle(string subtitleId)
    {
        if (!Config.SubtitleToggles.GetValueOrDefault(subtitleId, true)) return;
        
        if (!_subtitleIds.Contains(subtitleId))
        {
            _monitor.Log($"Invalid subtitle id: {subtitleId}", LogLevel.Warn);
            return;
        }

        var subtitleTranslationKey = subtitleId + ".subtitle";
        if (!_helper.Translation.ContainsKey(subtitleTranslationKey))
        {
            _monitor.Log($"No translation found for subtitle id: {subtitleId}. Translation key: {subtitleTranslationKey}");
            return;
        }
        
        var translatedSubtitle = _helper.Translation.Get(subtitleTranslationKey);
        _hudMessage.AddSubtitle(translatedSubtitle, _hudMessage.DefaultDurationTicks);
    }

    
}