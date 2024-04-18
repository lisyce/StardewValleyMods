using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace BZP_Allergies.HarmonyPatches.UI
{
    internal class CustomCheckbox : OptionsCheckbox
    {
        private Action<bool> OnChange;

        public CustomCheckbox(string label, bool checkInit, Action<bool> onChange, int x = -1, int y = -1) : base(label, -2, x, y)
        {
            OnChange = onChange;

            this.isChecked = checkInit;
        }

        public override void receiveLeftClick(int x, int y)
        {
            base.receiveLeftClick(x, y);
            OnChange(isChecked);
        }
    }
}
