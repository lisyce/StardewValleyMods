using System.Reflection;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace StardewAudioCaptions.Captions;

public class EventCaptionManager
{
    private static readonly MethodInfo PlaySoundHandler =
        AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.PlaySound));
    private static readonly MethodInfo PlayMusicHandler =
        AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.PlayMusic));
    
    private readonly Dictionary<string, List<EventCaption>> _eventCaptions;
    private readonly IMonitor _monitor;
    private readonly IModHelper _helper;
    private readonly CaptionManager _captionManager;

    private Event? _currentEvent;
    
    private List<Caption> _currentCaptions = new();
    private int _currentIdx = 0;
    
    public EventCaptionManager(IModHelper helper, IMonitor monitor, CaptionManager captionManager)
    {
        _eventCaptions = helper.ModContent.Load<Dictionary<string, List<EventCaption>>>("assets/event-captions.json");
        _monitor = monitor;
        _helper = helper;
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
        _monitor.Log("preparing for " + @event.id, LogLevel.Debug);
        _currentEvent = @event;
        _currentCaptions = new List<Caption>();
        _currentIdx = 0;

        var timesPlayed = new Dictionary<string, int>();
        if (!_eventCaptions.TryGetValue(@event.id, out var captionsForThisEvent)) return;
        
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

            if (cueId == "samBand") cueId = ConvertSamBand();
            
            // count times this sound has played
            timesPlayed.TryAdd(cueId, 0);
            timesPlayed[cueId]++;
            
            _monitor.Log(cueId, LogLevel.Debug);

            // find the appropriate caption
            var total = 0;
            foreach (var ec in captionsForThisEvent.Where(x => x.CueId == cueId))
            {
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

    public void CleanupAfterEvent()
    {
        _monitor.Log("cleanup", LogLevel.Debug);
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
}