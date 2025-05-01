using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace StardewAudioCaptions.Captions;

public class CaptionHudMessage
{
    private readonly List<CaptionHudMessageElement> _captions;
    public float FontScaling { get; set; }
    public int MaxVisible { get; set; }
    
    public bool CaptionsOn { get; set; }

    public CaptionHudMessage(ModConfig config)
    {
        _captions = new List<CaptionHudMessageElement>();
        FontScaling = config.FontScaling;
        MaxVisible = config.MaxVisibleCaptions;
        CaptionsOn = config.CaptionsOn;
    }

    public void AddCaption(Cue cue, string message, int maxDurationTicks, Caption backingCaption)
    {
        // is this caption already displayed?
        foreach (var caption in _captions)
        {
            if (caption.Message == message)
            {
                caption.Reset();
                return;
            }
        }
        
        var el = new CaptionHudMessageElement(cue, message, maxDurationTicks, backingCaption);
        _captions.Add(el);
    }
    
    public void Update()
    {
        _captions.RemoveWhere(s => s.Update());
    }

    public void Draw(SpriteBatch b)
    {
        if (_captions.Count == 0 || !CaptionsOn) return;
        
        var elHeight = (int) (Game1.smallFont.MeasureString("Ing!").Y * FontScaling);
        var elPadding = (int) (8 * FontScaling);
        var mainPadding = 16;

        var height = 2f * mainPadding - elPadding;  // we trim off the padding from the last element
        for (var i=0; i < MaxVisible && i < _captions.Count; i++)
        {
            var sub = _captions[i];
            height += (Game1.smallFont.MeasureString(sub.Message).Y * FontScaling) + elPadding;
        }
        

        var safeArea = Utility.getSafeArea();
        var x = safeArea.Left + 16;
        var y = safeArea.Top + 16;

        var boxSourceRect = new Rectangle(301, 288, 15, 15);
        IClickableMenu.drawTextureBox(b, Game1.mouseCursors, boxSourceRect, x, y, 300, (int) height, Color.White, drawShadow: false, scale: 4f);
        
        
        // draw the actual captions
        y += mainPadding;
        for (var i=0; i < MaxVisible && i < _captions.Count; i++)
        {
            var sub = _captions[i];
            var pos = new Vector2(x + mainPadding, y);
            b.DrawString(Game1.smallFont, sub.Message, pos, Color.White * sub.Transparency, 0, Vector2.Zero, FontScaling, SpriteEffects.None, 1f);
            y += elHeight + elPadding;
        }
    }

    public void ClearDisabledCaptions(ModConfig config)
    {
        _captions.RemoveWhere(c => !config.CaptionToggles.GetValueOrDefault(c.Caption.CaptionId, true));
    }
}