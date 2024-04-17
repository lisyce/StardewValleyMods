using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace BZP_Allergies.HarmonyPatches
{
    internal class AllergyMenu : SkillsPage
    {
        private readonly ClickableTextureComponent AllergyTab;
        private bool OnAllergyTab = false;

        public AllergyMenu(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            AllergyTab = new(
                "BarleyZP.BzpAllergies",
                new Rectangle(this.xPositionOnScreen - 48, this.yPositionOnScreen + 64 * 2, 64, 64),
                "",
                "Allergies",
                Game1.mouseCursors,
                new Rectangle(640, 80, 16, 16),
                4f
                );

        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (AllergyTab.containsPoint(x, y))
            {
                Game1.playSound("smallSelect");
                if (!OnAllergyTab)
                {
                    AllergyTab.bounds.X += CollectionsPage.widthToMoveActiveTab;
                }
                else
                {
                    AllergyTab.bounds.X -= CollectionsPage.widthToMoveActiveTab;
                }
                OnAllergyTab = !OnAllergyTab;
            } 
            else
            {
                base.receiveLeftClick(x, y, playSound);
            }
        }

        public override void performHoverAction(int x, int y)
        {
            if (AllergyTab.containsPoint(x, y))
            {
                Traverse.Create(this).Field("hoverText").SetValue(AllergyTab.hoverText);
            }
            else
            {
                base.performHoverAction(x, y);
            }
        }

        public override void draw(SpriteBatch b)
        {
            AllergyTab.draw(b);
            if (OnAllergyTab)
            {

            }
            else
            {
                base.draw(b);
            }
        }
    }
}
