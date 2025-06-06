namespace StardewAudioCaptions.Captions;

public class StardewAudioCaptionsApiProvider : IStardewAudioCaptionsApi
{
    private readonly CaptionManager _manager;
    
    public StardewAudioCaptionsApiProvider(CaptionManager manager)
    {
        _manager = manager;
    }
    
    public void RegisterCaptionForNextCue(string cueId, string captionId, int? maxDuration = null, object? tokens = null)
    {
        var capt = new Caption(cueId, captionId, maxDuration, tokens);
        _manager.RegisterCaptionForNextCue(capt);
    }

    public void RegisterDefaultCaption(string cueId, string captionId, int? maxDuration = null, object? tokens = null)
    {
        var capt = new Caption(cueId, captionId, maxDuration, tokens);
        _manager.RegisterDefaultCaption(capt);
    }

    public void UnregisterCaptionForNextCue(string cueId, string captionId)
    {
        var capt = new Caption(cueId, captionId);
        _manager.UnregisterCaptionForNextCue(capt);
    }
}