using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewSubtitles;

public class ModEntry : Mod
{
    private IModHelper _helper;
    
    public override void Entry(IModHelper helper)
    {
        _helper = helper;
        _helper.Events.Display.RenderedHud += OnRenderedHud;
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (Game1.game1.takingMapScreenshot || Game1.HostPaused) return;
        SubtitleHUDMessage.Instance.Draw(e.SpriteBatch);
    }
}