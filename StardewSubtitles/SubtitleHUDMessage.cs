using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace StardewSubtitles;

public class SubtitleHUDMessage
{
    private const float DefaultTime = 3500f;
    private List<SubtitleHUDMessageElement> _subtitles;
    private SpriteFont _font;

    private static SubtitleHUDMessage? _instance = null;

    public static SubtitleHUDMessage Instance
    {
        get
        {
            _instance ??= new SubtitleHUDMessage(Game1.smallFont);
            return _instance;
        }
    }

    private SubtitleHUDMessage(SpriteFont font)
    {
        _subtitles = new List<SubtitleHUDMessageElement>();
        _font = font;
        
        AddSubtitle("Test Subtitle");
    }

    public void AddSubtitle(string message)
    {
        var el = new SubtitleHUDMessageElement(message, DefaultTime);
        _subtitles.Add(el);
    }
    
    public void Update(GameTime time)
    {
        _subtitles.RemoveWhere(s => s.Update(time));
    }

    public void Draw(SpriteBatch b)
    {
        var elHeight = (int) _font.MeasureString("Ing!").Y;
        var elPadding = 8;
        var mainPadding = 16;
        var height = (elHeight * _subtitles.Count) + (2 * mainPadding) + (elPadding * (_subtitles.Count - 1));
        var minHeight = (2 * mainPadding) + elHeight;

        var safeArea = Utility.getSafeArea();
        var x = safeArea.Left + 16;
        var y = safeArea.Top + 16;

        var boxSourceRect = new Rectangle(0, 256, 60, 60);
        IClickableMenu.drawTextureBox(b, Game1.menuTexture, boxSourceRect, x, y, 300, Math.Max(height, minHeight), Color.White);
        
        // draw the actual subtitles
        y += mainPadding;
        foreach (var sub in _subtitles)
        {
            var pos = new Vector2(x + mainPadding, y);
            b.DrawString(_font, sub.Message, pos, Game1.textColor * sub.Transparency);
            y += elHeight + elPadding;
        }
    }
}