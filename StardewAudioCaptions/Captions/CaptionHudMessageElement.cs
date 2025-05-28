using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace StardewAudioCaptions.Captions;

public class CaptionHudMessageElement
{
    private int _ticksElapsed;
    private readonly Cue _cue;
    private readonly int _maxDurationTicks;
    
    public string Message { get; }
    public float Transparency { get; private set; }
    public Color Color { get; }
    
    public readonly Caption Caption;

    public CaptionHudMessageElement(Cue cue, string message, Caption caption, Color color)
    {
        Message = message;
        _cue = cue;
        Transparency = 1f;
        _ticksElapsed = 0;
        _maxDurationTicks = caption.MaxDuration;
        Caption = caption;
        Color = color;
    }

    public bool Update()
    {
        _ticksElapsed = Math.Min(_ticksElapsed + 1, CaptionManager.InfiniteDuration);
        var cuePlaying = _cue.IsPlaying && !_cue.IsPaused && _cue.Volume >= 10;
        if (VisibleLongEnough() && (!cuePlaying || ExceededMaxDuration()))
        {
            Transparency -= 0.03f;
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
        return _ticksElapsed >= ModEntry.ModCaptionManager.Config.MinDurationTicks;
    }

    private bool ExceededMaxDuration()
    {
        if (_maxDurationTicks == CaptionManager.InfiniteDuration) return false;
        return _ticksElapsed >= _maxDurationTicks;
    }
}