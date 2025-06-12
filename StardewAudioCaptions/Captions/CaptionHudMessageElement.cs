using Microsoft.Xna.Framework;
using StardewValley;

namespace StardewAudioCaptions.Captions;

/// <summary>
/// A <c>CaptionHudMessageElement</c> is an entry in a <see cref="CaptionHudMessage"/>.
/// </summary>
public class CaptionHudMessageElement
{
    private int _ticksElapsed;
    private readonly ICue _cue;
    private readonly int _maxDurationTicks;
    
    public float Transparency { get; private set; }
    public Color Color { get; }
    
    public readonly Caption Caption;

    public string Text => Caption.Text;

    public CaptionHudMessageElement(ICue cue, Caption caption, Color color)
    {
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
        var cuePlaying = _cue.IsPlaying && !_cue.IsPaused;
        if (VisibleLongEnough() && (!cuePlaying || ExceededMaxDuration()))
        {
            Transparency -= 0.1f;
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