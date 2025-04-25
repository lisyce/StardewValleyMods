using Microsoft.Xna.Framework;

namespace StardewSubtitles;

public class SubtitleHUDMessageElement
{
    private float _timeLeft;
    
    public string Message { get; }
    public float Transparency { get; private set; }

    public SubtitleHUDMessageElement(string message, float timeLeft)
    {
        Message = message;
        _timeLeft = timeLeft;
        Transparency = 1f;
    }

    public bool Update(GameTime time)
    {
        _timeLeft -= time.ElapsedGameTime.Milliseconds;
        if (_timeLeft < 0f)
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
}