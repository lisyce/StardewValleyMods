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

    private void PrepareForEvent(Event @event)
    {
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
            
            // count times this sound has played
            timesPlayed.TryAdd(cueId, 0);
            timesPlayed[cueId]++;

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

    private void CleanupAfterEvent()
    {
        _currentEvent = null;
    }
}