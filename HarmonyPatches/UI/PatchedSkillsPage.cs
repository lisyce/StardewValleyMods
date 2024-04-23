using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace BZP_Allergies.HarmonyPatches.UI
{
    internal class PatchedSkillsPage : SkillsPage
    {
        private readonly ClickableTextureComponent AllergyTab;
        private bool OnAllergyTab = false;
        private OptionsPage Options;

        public PatchedSkillsPage(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            AllergyTab = new(
                "BarleyZP.BzpAllergies",
                new Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 64 * 2, 64, 64),
                "",
                "Allergies",
                Game1.mouseCursors,
                new Rectangle(640, 80, 16, 16),
                4f
                );

            Options = new(x, y, width, height);

            // do we start random or not?
            PopulateOptions(false);

            if (AllergenManager.ModDataGet(Game1.player, "BarleyZP.BzpAllergies_Random", out string val))
            {
                PopulateOptions(val == "true");
            }
            else
            {
                PopulateOptions(false);
                Game1.player.modData["BarleyZP.BzpAllergies_Random"] = "false";
            }
        }

        private void PopulateOptions(bool random)
        {
            Options.options.Clear();  // get rid of the default settings
            Options.options.Add(new OptionsElement("My Allergies"));
            string smallText = random ? "You have randomized allergies." : "You have selected allergens.";
            Options.options.Add(new CustomOptionsSmallFontElement(smallText));
            Options.options.Add(new CustomOptionsHorizontalLine());


            if (random)
            {
                
                // get discovered allergies
                ISet<string> discovered = AllergenManager.ModDataSetGet(Game1.player, AllergenManager.FARMER_DISCOVERED_ALLERGIES_MODDATA_KEY);
                
                if (discovered.Count == 0)
                {
                    // render 0 count message
                    Options.options.Add(new CustomOptionsSmallFontElement("You haven't discovered any allergies yet."));
                }
                else
                {
                    // show all discovered messages
                    foreach (string allergy in discovered)
                    {
                        string displayName = AllergenManager.GetAllergenDisplayName(allergy);
                        Options.options.Add(new CustomOptionsSmallFontElement("- " + displayName));
                    }
                }

                // if hint, show many discovered/total
                if (ModEntry.Config.AllergenCountHint)
                {
                    ISet<string> has = AllergenManager.ModDataSetGet(Game1.player, AllergenManager.FARMER_HAS_ALLERGIES_MODDATA_KEY);
                    string hintCountText = "You've discovered " + discovered.Count + "/" + has.Count + " of your allergies.";
                    Options.options.Add(new CustomOptionsSmallFontElement(hintCountText));
                }

                Options.options.Add(new CustomOptionsHorizontalLine());
                Options.options.Add(new CustomOptionsButtonPair("Reroll Allergies", "Switch to Selection", RerollAllergies, AllergySelectionToggle));
            }
            else
            {
                Options.options.Add(new CustomOptionsHorizontalLine());
                Options.options.Add(new OptionsButton("Switch to Random", AllergySelectionToggle));
            }
        }

        private void AllergySelectionToggle()
        {
            bool currentlyRandom = false;
            if (AllergenManager.ModDataGet(Game1.player, "BarleyZP.BzpAllergies_Random", out string val))
            {
                currentlyRandom = val == "true";
            }
            Game1.player.modData["BarleyZP.BzpAllergies_Random"] = currentlyRandom ? "false" : "true";
            Options.currentItemIndex = 0;
            PopulateOptions(!currentlyRandom);
        }

        private void RerollAllergies()
        {

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
            else if (OnAllergyTab)
            {
                Options.receiveLeftClick(x, y, playSound);
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
            else if (OnAllergyTab)
            {
                Options.performHoverAction(x, y);
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
                ClickableTextureComponent upArrowRef = Traverse.Create(Options).Field("upArrow").GetValue<ClickableTextureComponent>();
                ClickableTextureComponent downArrowRef = Traverse.Create(Options).Field("downArrow").GetValue<ClickableTextureComponent>();

                upArrowRef.visible = Options.options.Count > 7;
                downArrowRef.visible = Options.options.Count > 7;

                Options.draw(b);
            }
            else
            {
                base.draw(b);
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            if (OnAllergyTab)
            {
                Options.snapToDefaultClickableComponent();
            }
            else
            {
                base.snapToDefaultClickableComponent();
            }
        }

        public override void applyMovementKey(int direction)
        {
            if (OnAllergyTab)
            {
                Options.applyMovementKey(direction);
            }
            else
            {
                base.applyMovementKey(direction);
            }
        }

        protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
        {
            if (OnAllergyTab)
            {
                Traverse.Create(Options).Method("customSnapBehavior").GetValue();
            }
            else
            {
                base.customSnapBehavior(direction, oldRegion, oldID);
            }
        }

        public override void snapCursorToCurrentSnappedComponent()
        {
            if (OnAllergyTab)
            {
                Options.snapCursorToCurrentSnappedComponent();
            }
            else
            {
                base.snapCursorToCurrentSnappedComponent();
            }
        }

        public override void leftClickHeld(int x, int y)
        {
            if (OnAllergyTab)
            {
                Options.leftClickHeld(x, y);
            }
            else
            {
                base.leftClickHeld(x, y);
            }
        }

        public override ClickableComponent getCurrentlySnappedComponent()
        {
            if (OnAllergyTab)
            {
                return Options.getCurrentlySnappedComponent();
            }
            else
            {
                return base.getCurrentlySnappedComponent();
            }
        }

        public override void setCurrentlySnappedComponentTo(int id)
        {
            if (OnAllergyTab)
            {
                Options.setCurrentlySnappedComponentTo(id);
            }
            else
            {
                base.setCurrentlySnappedComponentTo(id);
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (OnAllergyTab)
            {
                Options.receiveKeyPress(key);
            }
            else
            {
                base.receiveKeyPress(key);
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (OnAllergyTab)
            {
                Options.receiveScrollWheelAction(direction);
            }
            else
            {
                base.receiveScrollWheelAction(direction);
            }
        }


        public override void releaseLeftClick(int x, int y)
        {
            if (OnAllergyTab)
            {
                Options.releaseLeftClick(x, y);
            }
            else
            {
                base.releaseLeftClick(x, y);
            }
        }

    }
}
