using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace BZP_Allergies.HarmonyPatches.UI
{
    internal class CustomSmallFontElement : OptionsElement
    {
        public CustomSmallFontElement(string label) : base(label)
        {
        }

        public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
        {
            Utility.drawTextWithShadow(b, this.label, Game1.smallFont, new Vector2(slotX + this.bounds.X + (int)this.labelOffset.X, slotY + this.bounds.Y + (int)this.labelOffset.Y + 12), this.greyedOut ? (Game1.textColor * 0.33f) : Game1.textColor, 1f, 0.1f);
        }
    }
}
