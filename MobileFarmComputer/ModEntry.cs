using MobileFarmComputer.Apis;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MobileFarmComputer;

public class ModEntry : Mod
{
    private IViewEngine viewEngine = null!;
    private string viewAssetPrefix = null!;
    
    public override void Entry(IModHelper helper)
    {
        viewAssetPrefix = $"Mods/{ModManifest.UniqueID}/Views";
        
        helper.Events.Input.ButtonPressed += OnButtonPressed;
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        viewEngine = Helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI")!;
        viewEngine.RegisterViews(viewAssetPrefix, "assets");
#if DEBUG
        viewEngine.EnableHotReloading();
#endif
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button == SButton.B)
        {
            var items = TodoGenerators.GetTodos();
            
            var context = new
            {
                Location = "Report for: " + (Game1.player.currentLocation.GetRootLocation().GetDisplayName() ??
                                             "Unnamed Location"),
                TodoItems = items.todo,
                DoneItems = items.done
            };
            Game1.activeClickableMenu = viewEngine.CreateMenuFromAsset($"{viewAssetPrefix}/FarmComputer", context);
        }
    }
}