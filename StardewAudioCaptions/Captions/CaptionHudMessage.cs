using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewAudioCaptions.Captions;

public class CaptionHudMessage
{
    private readonly List<CaptionHudMessageElement> _captions;
    private int _width;

    public CaptionHudMessage()
    {
        _captions = new List<CaptionHudMessageElement>();
    }

    public void AddCaption(Cue cue, string message, Caption backingCaption, Color color)
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
        
        var el = new CaptionHudMessageElement(cue, message, backingCaption, color);
        _captions.Add(el);
        _captions.Sort((x, y) => y.Caption.Priority.CompareTo(x.Caption.Priority));
    }
    
    public void Update()
    {
        _captions.RemoveWhere(s => s.Update());
    }

    public void Draw(SpriteBatch b, ModConfig config)
    {
        if (_captions.Count == 0 || !config.CaptionsOn || (config.HideInMenu && Game1.activeClickableMenu != null)) return;

        _width = (int)(350 * config.FontScaling);  // set the width field
        
        var elHeight = (int) (Game1.smallFont.MeasureString("Ing!").Y * config.FontScaling);
        var elPadding = (int) (8 * config.FontScaling);
        var mainPadding = 16;

        var height = 2f * mainPadding - elPadding;  // we trim off the padding from the last element
        for (var i=0; i < config.MaxVisibleCaptions && i < _captions.Count; i++)
        {
            var sub = _captions[i];
            height += (Game1.smallFont.MeasureString(sub.Message).Y * config.FontScaling) + elPadding;
        }
        

        var loc = GetCaptionLocation(config, height);
        var x = (int) loc.X + config.CaptionOffsetX;
        var y = (int) loc.Y + config.CaptionOffsetY;

        var boxSourceRect = new Rectangle(301, 288, 15, 15);
        IClickableMenu.drawTextureBox(b, Game1.mouseCursors, boxSourceRect, x, y, _width, (int) height, Color.White, drawShadow: false, scale: 4f);
        
        
        // draw the actual captions
        y += mainPadding;
        for (var i=0; i < config.MaxVisibleCaptions && i < _captions.Count; i++)
        {
            var el = _captions[i];
            var pos = new Vector2(x + mainPadding, y);
            b.DrawString(Game1.smallFont, el.Message, pos, el.Color * el.Transparency, 0, Vector2.Zero, config.FontScaling, SpriteEffects.None, 1f);
            y += elHeight + elPadding;
        }
    }

    private Vector2 GetCaptionLocation(ModConfig config, float boxheight)
    {
        var safeArea = Utility.getSafeArea();
        return config.CaptionPosition switch
        {
            "Center Left" => new Vector2(safeArea.Left + 8, safeArea.Top + safeArea.Height / 2f - boxheight / 2),
            "Bottom Left" => new Vector2(safeArea.Left + 8, safeArea.Bottom - boxheight - 8),
            "Bottom Center" => IsToolbarBottom(out var toolbarYpos) ? new Vector2(safeArea.Left + safeArea.Width / 2f - _width / 2f, toolbarYpos - 8 - boxheight) : new Vector2(safeArea.Left + safeArea.Width / 2f - _width / 2f, Game1.uiViewport.Height - 8 - boxheight),
            "Bottom Right" => ShouldDrawBottomRightFlush(safeArea, out var nonFlushXpos) ? new Vector2(safeArea.Right - _width - 8, safeArea.Bottom - boxheight - 8) : new Vector2(nonFlushXpos, safeArea.Bottom - boxheight - 8),
            "Center Right" => new Vector2(safeArea.Right - _width - 8, safeArea.Top + safeArea.Height / 2f - boxheight / 2),
            // default is top left
            _ => PlayerInMines() ? new Vector2(safeArea.Left + 8, safeArea.Top + 64) : new Vector2(safeArea.Left + 8, safeArea.Top + 8)
        };
    }

    private bool IsToolbarBottom(out int toolbarYpos)
    {
        toolbarYpos = -1;
        var menu = Game1.onScreenMenus.First(m => m is Toolbar);
        if (menu is not Toolbar tb) return false;
        toolbarYpos = tb.yPositionOnScreen - tb.height / 2;
        
        return tb.yPositionOnScreen > Game1.uiViewport.Height / 2;
    }

    private bool ShouldDrawBottomRightFlush(Rectangle safeArea, out int nonFlushXpos)
    {
        nonFlushXpos = -1;
        var menu = Game1.onScreenMenus.First(m => m is Toolbar);
        if (menu is not Toolbar tb || Game1.CurrentEvent != null) return true;
        
        if (Game1.showingHealthBar)
        {
            nonFlushXpos = safeArea.Width - 8 - 48 - 56 - 8 - _width;
            return tb.buttons[11].bounds.Right + 40 + _width + 8 + 48 + 56 >= Game1.uiViewport.Width;
        }
        else
        {
            nonFlushXpos = safeArea.Width - 8 - 48 - 8 - _width;
            return tb.buttons[11].bounds.Right + 40 + _width + 8 + 48 >= Game1.uiViewport.Width;
        }
    }

    private bool PlayerInMines()
    {
        return Game1.player.currentLocation is MineShaft;
    }

    public void ClearDisabledCaptions(ModConfig config)
    {
        _captions.RemoveWhere(c => !config.CaptionToggles.GetValueOrDefault(c.Caption.CaptionId, true));
    }
}