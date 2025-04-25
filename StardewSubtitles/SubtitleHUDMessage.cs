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
    private float _fontScaling;

    private static SubtitleHUDMessage? _instance = null;

    public static SubtitleHUDMessage Instance
    {
        get
        {
            _instance ??= new SubtitleHUDMessage(Game1.smallFont, 0.75f);
            return _instance;
        }
    }

    private SubtitleHUDMessage(SpriteFont font, float fontScaling)
    {
        _subtitles = new List<SubtitleHUDMessageElement>();
        _font = font;
        _fontScaling = fontScaling;
        
        AddSubtitle("Test Subtitle");
        AddSubtitle("Test Subtitle2");
        AddSubtitle("Test Subtitle3");
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
        var elHeight = (int) (_font.MeasureString("Ing!").Y * _fontScaling);
        var elPadding = (int) (8 * _fontScaling);
        var mainPadding = 16;

        var height = 2f * mainPadding - elPadding;  // we trim off the padding from the last element
        foreach (var sub in _subtitles)
        {
            height += (_font.MeasureString(sub.Message).Y * _fontScaling) + elPadding;
        }
        
        var minHeight = (2 * mainPadding) + elHeight;

        var safeArea = Utility.getSafeArea();
        var x = safeArea.Left + 16;
        var y = safeArea.Top + 16;

        var boxSourceRect = new Rectangle(301, 288, 15, 15);
        IClickableMenu.drawTextureBox(b, Game1.mouseCursors, boxSourceRect, x, y, 300, Math.Max((int) height, minHeight), Color.White, drawShadow: false, scale: 4f);
        
        
        // draw the actual subtitles
        y += mainPadding;
        foreach (var sub in _subtitles)
        {
            var pos = new Vector2(x + mainPadding, y);
            b.DrawString(_font, sub.Message, pos, Color.White * sub.Transparency, 0, Vector2.Zero, _fontScaling, SpriteEffects.None, 1f);
            y += elHeight + elPadding;
        }
    }
}