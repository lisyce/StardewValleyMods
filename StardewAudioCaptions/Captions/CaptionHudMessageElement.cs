using Microsoft.Xna.Framework.Audio;

namespace StardewAudioCaptions.Captions;

public class CaptionHudMessageElement
{
    private int _ticksElapsed;
    private readonly Cue _cue;
    private readonly int _maxDurationTicks;
    
    public string Message { get; }
    public float Transparency { get; private set; }

    public CaptionHudMessageElement(Cue cue, string message, int maxDurationTicks)
    {
        Message = message;
        _cue = cue;
        Transparency = 1f;
        _ticksElapsed = 0;
        _maxDurationTicks = maxDurationTicks;
    }

    public bool Update()
    {
        _ticksElapsed = Math.Min(_ticksElapsed + 1, CaptionManager.InfiniteDuration);
        if (VisibleLongEnough() && (!_cue.IsPlaying || _ticksElapsed >= _maxDurationTicks))
        {
            Transparency -= 0.02f;
            if (Transparency < 0f)
            {
                return true;
            }
        }
        else if (Transparency < 1f)
        {
            Transparency = Math.Min(Transparency + 0.02f, 1f);
        }
        return false;
    }

    public void Reset()
    {
        Transparency = 1f;
        _ticksElapsed = 0;
    }

    private bool VisibleLongEnough()
    {
        return _ticksElapsed >= CaptionManager.MinDurationTicks;
    }
}