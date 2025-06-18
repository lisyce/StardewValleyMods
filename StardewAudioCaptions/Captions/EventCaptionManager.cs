using System.Reflection;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace StardewAudioCaptions.Captions;

/// <summary>
/// The <c>EventCaptionManager</c> is responsible for registering captions to be displayed in the HUD when sounds and music play during cutscenes.
/// </summary>
public class EventCaptionManager
{
    private static readonly MethodInfo PlaySoundHandler =
        AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.PlaySound));

    private static readonly MethodInfo PlayMusicHandler =
        AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.PlayMusic));

    private readonly IMonitor _monitor;
    private readonly CaptionManager _captionManager;

    private Event? _currentEvent;

    private List<Caption> _currentCaptions = new();
    private int _currentIdx = 0;

    public EventCaptionManager(IMonitor monitor, CaptionManager captionManager)
    {
        _monitor = monitor;
        _captionManager = captionManager;
    }

    public void Update()
    {
        if (Game1.CurrentEvent == null && _currentEvent != null)
        {
            CleanupAfterEvent();
        }
        else if (Game1.CurrentEvent != null && Game1.CurrentEvent.id != _currentEvent?.id)
        {
            PrepareForEvent(Game1.CurrentEvent);
        }
    }

    public void BeforePlaySound()
    {
        if (_currentEvent == null || _currentIdx >= _currentCaptions.Count) return;

        var caption = _currentCaptions[_currentIdx];
        _captionManager.RegisterCaptionForNextCue(caption);
        _currentIdx++;
    }

    public void PrepareForEvent(Event @event)
    {
#if DEBUG
        _monitor.Log("preparing for event " + @event.id, LogLevel.Debug);
#endif
        _currentEvent = @event;
        _currentCaptions = new List<Caption>();
        _currentIdx = 0;

        var timesPlayed = new Dictionary<string, int>();
        if (!ModEntry.EventCaptions.TryGetValue(@event.id, out var captionsForThisEvent)) return;

        if (TryGetStartingMusic(out var startingMusicId))
        {
            TryAddCaptionForCue(startingMusicId, timesPlayed, captionsForThisEvent);
        }
        
        foreach (var cmd in @event.eventCommands)
        {
            var args = ArgUtility.SplitBySpaceQuoteAware(cmd);
            var cmdName = ArgUtility.Get(args, 0);

            if (!Event.TryGetEventCommandHandler(cmdName, out var handler)) continue;

            string cueId;
            if (handler.Method.Equals(PlaySoundHandler)) cueId = ArgUtility.Get(args, 1);
            else if (handler.Method.Equals(PlayMusicHandler))
            {
                ArgUtility.TryGetRemainder(args, 1, out cueId, out var _, ' ', "string musicId");
            }
            else continue;

            TryAddCaptionForCue(cueId, timesPlayed, captionsForThisEvent);
        }
    }

    public void CleanupAfterEvent()
    {
#if DEBUG
        _monitor.Log("cleanup after event", LogLevel.Debug);
#endif
        _currentEvent = null;
    }

    public bool EventInProgress()
    {
        return _currentEvent != null;
    }

    private string ConvertSamBand()
    {
        if (Game1.player.DialogueQuestionsAnswered.Contains("78"))
        {
            return "shimmeringbastion";
        }
        else if (Game1.player.DialogueQuestionsAnswered.Contains("79"))
        {
            return "honkytonky";
        }
        else if (Game1.player.DialogueQuestionsAnswered.Contains("77"))
        {
            return "heavy";
        }
        else
        {
            return "poppy";
        }
    }

    private bool TryGetStartingMusic(out string music)
    {
        music = "";
        if (_currentEvent == null) return false;

        return ArgUtility.TryGet(_currentEvent.eventCommands, 0, out music, out var error, allowBlank: true,
            "string musicId");
    }

    private void TryAddCaptionForCue(string cueId, Dictionary<string, int> timesPlayed, List<EventCaption> captionsForThisEvent)
    {
        if (cueId == "samBand") cueId = ConvertSamBand();

        // count times this sound has played
        timesPlayed.TryAdd(cueId, 0);
        timesPlayed[cueId]++;

        _monitor.Log(cueId, LogLevel.Debug);

        // find the appropriate caption
        var total = 0;
        foreach (var ec in captionsForThisEvent.Where(x => x.CueId == cueId))
        {
#if DEBUG
            _monitor.Log($"Adding caption with id {ec.CaptionId} for cue {cueId}", LogLevel.Debug);
#endif
            total += ec.WhenCount;
            var firstNApplies = ec.When == EventCaptionCondition.FirstN && timesPlayed[cueId] <= total;
            var afterNApplies = ec.When == EventCaptionCondition.AfterN && timesPlayed[cueId] > total;
            if (ec.When == EventCaptionCondition.Always || firstNApplies || afterNApplies)
            {
                _currentCaptions.Add(new Caption(cueId, ec.CaptionId));
                break;
            }
        }
    }
}