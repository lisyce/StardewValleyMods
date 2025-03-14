using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace BzpAllergies.HarmonyPatches.UI
{
    internal class CustomOptionsCheckbox : OptionsCheckbox
    {
        private Action<bool> OnChange;
        public string HoverText;

        public CustomOptionsCheckbox(string label, bool checkInit, Action<bool> onChange, string hoverText = "", int x = -1, int y = -1) : base(label, -2, x, y)
        {
            OnChange = onChange;
            this.isChecked = checkInit;
            HoverText = hoverText;
        }

        public override void receiveLeftClick(int x, int y)
        {
            base.receiveLeftClick(x, y);
            OnChange(isChecked);
        }

    }
}
