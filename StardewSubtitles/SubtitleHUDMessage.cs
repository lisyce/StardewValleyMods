using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace StardewSubtitles;

public class SubtitleHUDMessage
{
    private const int DefaultDurationTicks = 120;
    private List<SubtitleHUDMessageElement> _subtitles;
    private SpriteFont _font;
    private float _fontScaling;
    private int _maxVisible;

    private static SubtitleHUDMessage? _instance;

    public static SubtitleHUDMessage Instance
    {
        get
        {
            _instance ??= new SubtitleHUDMessage(Game1.smallFont, 0.75f, 6);
            return _instance;
        }
    }

    private SubtitleHUDMessage(SpriteFont font, float fontScaling, int maxVisible)
    {
        _subtitles = new List<SubtitleHUDMessageElement>();
        _font = font;
        _fontScaling = fontScaling;
        _maxVisible = maxVisible;
    }

    public void AddSubtitle(string message)
    {
        var el = new SubtitleHUDMessageElement(message, DefaultDurationTicks);
        _subtitles.Add(el);
    }
    
    public void Update()
    {
        _subtitles.RemoveWhere(s => s.Update());
    }

    public void Draw(SpriteBatch b)
    {
        if (_subtitles.Count == 0) return;
        
        var elHeight = (int) (_font.MeasureString("Ing!").Y * _fontScaling);
        var elPadding = (int) (8 * _fontScaling);
        var mainPadding = 16;

        var height = 2f * mainPadding - elPadding;  // we trim off the padding from the last element
        for (var i=0; i < _maxVisible && i < _subtitles.Count; i++)
        {
            var sub = _subtitles[i];
            height += (_font.MeasureString(sub.Message).Y * _fontScaling) + elPadding;
        }
        

        var safeArea = Utility.getSafeArea();
        var x = safeArea.Left + 16;
        var y = safeArea.Top + 16;

        var boxSourceRect = new Rectangle(301, 288, 15, 15);
        IClickableMenu.drawTextureBox(b, Game1.mouseCursors, boxSourceRect, x, y, 300, (int) height, Color.White, drawShadow: false, scale: 4f);
        
        
        // draw the actual subtitles
        y += mainPadding;
        for (var i=0; i < _maxVisible && i < _subtitles.Count; i++)
        {
            var sub = _subtitles[i];
            var pos = new Vector2(x + mainPadding, y);
            b.DrawString(_font, sub.Message, pos, Color.White * sub.Transparency, 0, Vector2.Zero, _fontScaling, SpriteEffects.None, 1f);
            y += elHeight + elPadding;
        }
    }
}