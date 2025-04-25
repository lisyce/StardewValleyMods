namespace StardewSubtitles.Subtitles;

public class SubtitleHUDMessageElement
{
    private int _ticksLeft;
    private readonly int _originalDuration;
    
    public string Message { get; }
    public float Transparency { get; private set; }

    public SubtitleHUDMessageElement(string message, int ticksLeft)
    {
        Message = message;
        _ticksLeft = ticksLeft;
        _originalDuration = ticksLeft;
        Transparency = 1f;
    }

    public bool Update()
    {
        _ticksLeft--;
        if (_ticksLeft < 0)
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
        _ticksLeft = _originalDuration;
        Transparency = 1f;
    }
}