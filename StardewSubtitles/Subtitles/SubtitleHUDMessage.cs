using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace StardewSubtitles.Subtitles;

public class SubtitleHUDMessage
{
    private readonly List<SubtitleHUDMessageElement> _subtitles;
    private readonly SpriteFont _font;
    public float FontScaling { get; set; }
    public int MaxVisible { get; set; }
    public int DefaultDurationTicks { get; set; }
    
    public bool SubtitlesOn { get; set; }

    public SubtitleHUDMessage(ModConfig config)
    {
        _subtitles = new List<SubtitleHUDMessageElement>();
        _font = Game1.smallFont;
        FontScaling = config.FontScaling;
        MaxVisible = config.MaxVisibleSubtitles;
        DefaultDurationTicks = config.DefaultDurationTicks;
        SubtitlesOn = config.SubtitlesOn;
    }

    public void AddSubtitle(string message, int durationTicks)
    {
        // is this subtitle already displayed?
        foreach (var subtitle in _subtitles)
        {
            if (subtitle.Message == message)
            {
                subtitle.Reset();
                return;
            }
        }
        
        var el = new SubtitleHUDMessageElement(message, durationTicks);
        _subtitles.Add(el);
    }
    
    public void Update()
    {
        _subtitles.RemoveWhere(s => s.Update());
    }

    public void Draw(SpriteBatch b)
    {
        if (_subtitles.Count == 0 || !SubtitlesOn) return;
        
        var elHeight = (int) (_font.MeasureString("Ing!").Y * FontScaling);
        var elPadding = (int) (8 * FontScaling);
        var mainPadding = 16;

        var height = 2f * mainPadding - elPadding;  // we trim off the padding from the last element
        for (var i=0; i < MaxVisible && i < _subtitles.Count; i++)
        {
            var sub = _subtitles[i];
            height += (_font.MeasureString(sub.Message).Y * FontScaling) + elPadding;
        }
        

        var safeArea = Utility.getSafeArea();
        var x = safeArea.Left + 16;
        var y = safeArea.Top + 16;

        var boxSourceRect = new Rectangle(301, 288, 15, 15);
        IClickableMenu.drawTextureBox(b, Game1.mouseCursors, boxSourceRect, x, y, 300, (int) height, Color.White, drawShadow: false, scale: 4f);
        
        
        // draw the actual subtitles
        y += mainPadding;
        for (var i=0; i < MaxVisible && i < _subtitles.Count; i++)
        {
            var sub = _subtitles[i];
            var pos = new Vector2(x + mainPadding, y);
            b.DrawString(_font, sub.Message, pos, Color.White * sub.Transparency, 0, Vector2.Zero, FontScaling, SpriteEffects.None, 1f);
            y += elHeight + elPadding;
        }
    }
}