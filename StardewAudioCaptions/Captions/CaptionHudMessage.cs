using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;

namespace StardewAudioCaptions.Captions;

public class CaptionHudMessage
{
    private readonly List<CaptionHudMessageElement> _captions;
    private int _width;

    public CaptionHudMessage()
    {
        _captions = new List<CaptionHudMessageElement>();
    }

    public void AddCaption(ICue cue, string message, Caption backingCaption, Color color)
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
        if (_captions.Count == 0 || !config.CaptionsOn || !ShouldDrawDespiteMenu(config)) return;

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

    private bool ShouldDrawDespiteMenu(ModConfig config)
    {
        if (!config.HideInMenu) return true;
        return Game1.activeClickableMenu is null or DialogueBox or BobberBar;
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
            _ => ShouldOffsetTopLeftY(out var yOffset) ? new Vector2(safeArea.Left + 8, safeArea.Top + yOffset) : new Vector2(safeArea.Left + 8, safeArea.Top + 8)
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

    private bool ShouldOffsetTopLeftY(out int yOffset)
    {
        yOffset = 0;
        if (Game1.player.currentLocation is MineShaft)
        {
            yOffset = 64;
            return true;
        }
        else if (Game1.currentMinigame is TargetGame or FishingGame)
        {
            yOffset = 100;
            return true;
        }
        else if (FestivalCurrencyDisplay())
        {
            yOffset = 120;
            return true;
        }
        else if (ShowingCurrency())
        {
            yOffset = 125;
            return true;
        }

        return false;
    }

    private bool ShowingCurrency()
    {
        if (Game1.specialCurrencyDisplay == null) return false;
        
        return Game1.specialCurrencyDisplay.displayedCurrencies.Count > 0;
    }

    private bool FestivalCurrencyDisplay()
    {
        if (Game1.CurrentEvent == null) return false;

        return Game1.CurrentEvent.isSpecificFestival("fall16") || Game1.CurrentEvent.isSpecificFestival("spring13") ||
               Game1.CurrentEvent.isSpecificFestival("winter8");
    }

    public void ClearDisabledCaptions(ModConfig config)
    {
        _captions.RemoveWhere(c => !config.CaptionToggles.GetValueOrDefault(c.Caption.CaptionId, true));
    }
}