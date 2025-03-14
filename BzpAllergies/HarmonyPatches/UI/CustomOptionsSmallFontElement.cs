using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace BzpAllergies.HarmonyPatches.UI
{
    internal class CustomOptionsSmallFontElement : OptionsElement
    {
        public CustomOptionsSmallFontElement(string label) : base(label)
        {
        }

        public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
        {
            b.DrawString(Game1.dialogueFont, this.label, new Vector2(slotX + this.bounds.X + (int)this.labelOffset.X, slotY + this.bounds.Y + (int)this.labelOffset.Y + 12), Game1.textColor);
        }
    }
}
