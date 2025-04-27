using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace StardewSubtitles.Subtitles;

public class SubtitleManager
{
    public static int MinDurationTicks { get; set; }
    public static readonly int InfiniteDuration = int.MaxValue;
    
    private readonly HashSet<string> _subtitleIds;
    private readonly SubtitleHUDMessage _hudMessage;
    private readonly IMonitor _monitor;
    private readonly IModHelper _helper;
    private readonly Dictionary<string, List<Subtitle>> _subtitlesOnNextCue;
    private readonly Dictionary<string, Subtitle> _defaultCueSubtitles;
    
    public ModConfig Config { get; set; }
    
    public SubtitleManager(IModHelper helper, SubtitleHUDMessage hudMessage, IMonitor monitor, ModConfig config)
    {
        _subtitleIds = helper.ModContent.Load<List<string>>("assets/subtitles.json").ToHashSet();
        _hudMessage = hudMessage;
        _monitor = monitor;
        _helper = helper;
        Config = config;
        _subtitlesOnNextCue = new Dictionary<string, List<Subtitle>>();
        _defaultCueSubtitles = new Dictionary<string, Subtitle>();
        MinDurationTicks = config.MinDurationTicks;
    }

    /// <summary>
    /// Adds subtitles to the HUD for the sound cue if any are registered.
    /// </summary>
    /// <param name="cue">The sound cue</param>
    public void OnSoundPlayed(Cue cue)
    {
        var cueId = cue.Name;
        _monitor.Log(cueId, LogLevel.Debug);
        
        // do we have any overrides?
        if (_subtitlesOnNextCue.TryGetValue(cueId, out var subtitles))
        {
            foreach (var subtitle in subtitles)
            {
                AddSubtitle(cue, subtitle);
            }

            _subtitlesOnNextCue.Remove(cueId);
        }
        else if (_defaultCueSubtitles.TryGetValue(cueId, out var subtitle))
        {
            // default subtitle
            AddSubtitle(cue, subtitle);
        }
    }
    
    /// <summary>
    /// The next time the appropriate cue plays, adds the subtitle to the HUD.
    /// Does not persist beyond the very next time the cue is played. This subtitle will override any default
    /// subtitles for the sound cue.
    /// </summary>
    /// <param name="subtitle">The subtitle to show</param>
    public void RegisterSubtitleForNextCue(Subtitle subtitle)
    {
        if (!_subtitlesOnNextCue.ContainsKey(subtitle.CueId)) _subtitlesOnNextCue.Add(subtitle.CueId, new List<Subtitle>());

        _subtitlesOnNextCue[subtitle.CueId].Add(subtitle);
        // _monitor.Log($"Registered subtitle {subtitleId} for next cue {cueId}", LogLevel.Debug);
    }
    
    /// <summary>
    /// Removes a subtitle for the next sound cue that was added by <see cref="RegisterSubtitleForNextCue"/>.
    /// Usually used with a prefix/postfix pair to avoid a transpiler.
    /// </summary>
    /// <param name="cueId"></param>
    /// <param name="subtitleId"></param>
    public void UnRegisterSubtitleForNextCue(string cueId, string subtitleId)
    {
        if (_subtitlesOnNextCue.TryGetValue(cueId, out var subtitles))
        {
            subtitles.RemoveWhere(x => x.SubtitleId == subtitleId);
            if (subtitles.Count == 0) _subtitlesOnNextCue.Remove(cueId);
            // _monitor.Log($"Unregistered subtitle {subtitleId} for cue {cueId}", LogLevel.Debug);
        }
    }

    /// <summary>
    /// Registers a persistent default subtitle for a sound cue.
    /// </summary>
    /// <param name="subtitle">The subtitle to show</param>
    public void RegisterDefaultSubtitle(Subtitle subtitle)
    {
        if (!_defaultCueSubtitles.TryAdd(subtitle.CueId, subtitle))
        {
            _monitor.Log($"Failed to register default subtitle {subtitle.SubtitleId} for sound cue {subtitle.CueId} because a default subtitle {_defaultCueSubtitles[subtitle.CueId]} already exists.", LogLevel.Warn);
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

    private void AddSubtitle(Cue cue, Subtitle subtitle)
    {
        var subtitleId = subtitle.SubtitleId;
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
        _hudMessage.AddSubtitle(cue, translatedSubtitle, subtitle.MaxDuration);
    }
}