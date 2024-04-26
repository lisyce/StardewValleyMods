using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace BZP_Allergies.HarmonyPatches.UI
{
    internal class CustomOptionsHorizontalLine : OptionsElement
    {
        public CustomOptionsHorizontalLine() : base(string.Empty)
        {
        }

        public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
        {
            if (context != null)
            {
                object[] drawPartitionArgs = { b, this.bounds.Y + slotY, true, -1, -1, -1 };
                Traverse.Create(context).Method("drawHorizontalPartition", drawPartitionArgs).GetValue(drawPartitionArgs);
            }
        }
    }
}
