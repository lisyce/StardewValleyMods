#nullable enable

using System.Diagnostics.CodeAnalysis;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;

namespace BzpAllergies.Apis;


/// <summary>
/// This enum is included for reference and has the order value for
/// all the default tabs from the base game. These values are intentionally
/// spaced out to allow for modded tabs to be inserted at specific points.
/// </summary>
public enum VanillaTabOrders
{
    Inventory = 0,
    Skills = 20,
    Social = 40,
    Map = 60,
    Crafting = 80,
    Animals = 100,
    Powers = 120,
    Collections = 140,
    Options = 160,
    Exit = 200
}


public interface IBetterGameMenuApi
{

    /// <summary>
    /// A delegate for drawing something onto the screen.
    /// </summary>
    /// <param name="batch">The <see cref="SpriteBatch"/> to draw with.</param>
    /// <param name="bounds">The region where the thing should be drawn.</param>
    public delegate void DrawDelegate(SpriteBatch batch, Rectangle bounds);

    #region Helpers

    /// <summary>
    /// Create a draw delegate that draws the provided texture to the
    /// screen. This supports basic animations if required.
    /// </summary>
    /// <param name="texture">The texture to draw from.</param>
    /// <param name="source">The source rectangle to draw.</param>
    /// <param name="scale">The scale to draw the source at.</param>
    /// <param name="frames">The number of frames to draw.</param>
    /// <param name="frameTime">The amount of time each frame should be displayed.</param>
    DrawDelegate CreateDraw(Texture2D texture, Rectangle source, float scale = 1f, int frames = 1, int frameTime = 16);

    #endregion
    
    #region Tab Registration

	/// <summary>
	/// Register a new tab with the system. 
	/// </summary>
	/// <param name="id">The id of the tab to add.</param>
	/// <param name="order">The order of this tab relative to other tabs.
	/// See <see cref="VanillaTabOrders"/> for an example of existing values.</param>
	/// <param name="getDisplayName">A method that returns the display name of
	/// this tab, to be displayed in a tool-tip to the user.</param>
	/// <param name="getIcon">A method that returns an icon to be displayed
	/// on the tab UI for this tab, expecting both a texture and a Rectangle.</param>
	/// <param name="priority">The priority of the default page instance
	/// provider for this tab. When multiple page instance providers are
	/// registered, and the user hasn't explicitly chosen one, then the
	/// one with the highest priority is used. Please note that a given
	/// mod can only register one provider for any given tab.</param>
	/// <param name="getPageInstance">A method that returns a page instance
	/// for the tab. This should never return a <c>null</c> value.</param>
	/// <param name="getDecoration">A method that returns a decoration for
	/// the tab UI for this tab. This can be used to, for example, add a
	/// sparkle to a tab to indicate that new content is available. The
	/// expected output is either <c>null</c> if no decoration should be
	/// displayed, or a texture, rectangle, number of animation frames
	/// to display, and delay between frame advancements. Please note that
	/// the decoration will be automatically cleared when the user navigates
	/// to the tab.</param>
	/// <param name="getTabVisible">A method that returns whether or not the
	/// tab should be visible in the menu. This is called whenever a menu is
	/// opened, as well as when <see cref="IBetterGameMenu.UpdateTabs(string?)"/>
	/// is called.</param>
	/// <param name="getMenuInvisible">A method that returns the value that the
	/// game menu should set its <see cref="IBetterGameMenu.Invisible"/> flag
	/// to when this is the active tab.</param>
	/// <param name="getWidth">A method that returns a specific width to use when
	/// rendering this tab, in case the page instance requires a different width
	/// than the standard value.</param>
	/// <param name="getHeight">A method that returns a specific height to use
	/// when rendering this tab, in case the page instance requires a different
	/// height than the standard value.</param>
	/// <param name="onResize">A method that is called when the game window is
	/// resized, in addition to the standard <see cref="IClickableMenu.gameWindowSizeChanged(Rectangle, Rectangle)"/>.
	/// This can be used to recreate a menu page if necessary by returning a
	/// new <see cref="IClickableMenu"/> instance. Several menus in the vanilla
	/// game use this logic.</param>
	/// <param name="onClose">A method that is called whenever a page instance
	/// is cleaned up. The standard Game Menu doesn't call <see cref="IClickableMenu.cleanupBeforeExit"/>
	/// of its pages, and only calls <see cref="IClickableMenu.emergencyShutDown"/>
	/// of the currently active tab, and we're keeping that behavior for
	/// compatibility. This method will always be called. This includes calling
	/// it for menus that were replaced by the <c>onResize</c> method.</param>
	void RegisterTab(
		string id,
		int order,
		Func<string> getDisplayName,
		Func<(DrawDelegate DrawMethod, bool DrawBackground)> getIcon,
		int priority,
		Func<IClickableMenu, IClickableMenu> getPageInstance,
		Func<DrawDelegate?>? getDecoration = null,
		Func<bool>? getTabVisible = null,
		Func<bool>? getMenuInvisible = null,
		Func<int, int>? getWidth = null,
		Func<int, int>? getHeight = null,
		Func<(IClickableMenu Menu, IClickableMenu OldPage), IClickableMenu?>? onResize = null,
		Action<IClickableMenu>? onClose = null
	);

    /// <summary>
    /// Register a tab implementation for an existing tab. This can be used
    /// to override an existing vanilla game tab.
    /// </summary>
    /// <param name="id">The id of the tab to register an implementation for.
    /// The keys for the vanilla game tabs are the same as those in the
    /// <see cref="VanillaTabOrders"/> enum.</param>
    /// <param name="priority">The priority of this page instance provider
    /// for this tab. When multiple page instance providers are
    /// registered, and the user hasn't explicitly chosen one, then the
    /// one with the highest priority is used. Please note that a given
    /// mod can only register one provider for any given tab.</param>
    /// <param name="getPageInstance">A method that returns a page instance
    /// for the tab. This should never return a <c>null</c> value.</param>
    /// <param name="getDecoration">A method that returns a decoration for
    /// the tab UI for this tab. This can be used to, for example, add a
    /// sparkle to a tab to indicate that new content is available. The
    /// expected output is either <c>null</c> if no decoration should be
    /// displayed, or a texture, rectangle, number of animation frames
    /// to display, and delay between frame advancements. Please note that
    /// the decoration will be automatically cleared when the user navigates
    /// to the tab.</param>
    /// <param name="getTabVisible">A method that returns whether or not the
    /// tab should be visible in the menu. This is called whenever a menu is
    /// opened, as well as when <see cref="IBetterGameMenu.UpdateTabs(string?)"/>
    /// is called.</param>
    /// <param name="getMenuInvisible">A method that returns the value that the
    /// game menu should set its <see cref="IBetterGameMenu.Invisible"/> flag
    /// to when this is the active tab.</param>
    /// <param name="getWidth">A method that returns a specific width to use when
    /// rendering this tab, in case the page instance requires a different width
    /// than the standard value.</param>
    /// <param name="getHeight">A method that returns a specific height to use
    /// when rendering this tab, in case the page instance requires a different
    /// height than the standard value.</param>
    /// <param name="onResize">A method that is called when the game window is
    /// resized, in addition to the standard <see cref="IClickableMenu.gameWindowSizeChanged(Rectangle, Rectangle)"/>.
    /// This can be used to recreate a menu page if necessary by returning a
    /// new <see cref="IClickableMenu"/> instance. Several menus in the vanilla
    /// game use this logic.</param>
    /// <param name="onClose">A method that is called whenever a page instance
    /// is cleaned up. The standard Game Menu doesn't call <see cref="IClickableMenu.cleanupBeforeExit"/>
    /// of its pages, and only calls <see cref="IClickableMenu.emergencyShutDown"/>
    /// of the currently active tab, and we're keeping that behavior for
    /// compatibility. This method will always be called. This includes calling
    /// it for menus that were replaced by the <c>onResize</c> method.</param>
    void RegisterImplementation(
        string id,
        int priority,
        Func<IClickableMenu, IClickableMenu> getPageInstance,
        Func<DrawDelegate?>? getDecoration = null,
        Func<bool>? getTabVisible = null,
        Func<bool>? getMenuInvisible = null,
        Func<int, int>? getWidth = null,
        Func<int, int>? getHeight = null,
        Func<(IClickableMenu Menu, IClickableMenu OldPage), IClickableMenu?>? onResize = null,
        Action<IClickableMenu>? onClose = null
    );

    #endregion

}
