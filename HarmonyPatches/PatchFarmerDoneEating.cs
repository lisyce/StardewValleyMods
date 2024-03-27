using HarmonyLib;
using StardewValley;
using StardewModdingAPI;
using StardewValley.Buffs;
using StardewValley.GameData.Buffs;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Utilities;

using static BZP_Allergies.AllergenManager;
using StardewValley.ItemTypeDefinitions;
using Microsoft.Xna.Framework;
using System.Buffers;

namespace BZP_Allergies.HarmonyPatches
{
    [HarmonyPatch(typeof(Farmer), nameof(Farmer.doneEating))]
    internal class PatchFarmerDoneEating : Initializable
    {
        public static StardewValley.Object? item = null;

        [HarmonyPrefix]
        static bool DoneEating_Prefix(ref Farmer __instance, out int __state)
        {
            try
            {
                StardewValley.Object? itemToEat = __instance.itemToEat as StardewValley.Object;
                __state = itemToEat == null ? int.MinValue : itemToEat.Edibility;
                if (itemToEat == null)
                {
                    return true;
                }
                // MUST SET item FIRST
                item = itemToEat;
                Texture2D sprites = Game1.content.Load<Texture2D>(PathUtilities.NormalizeAssetName(@"Mods/BarleyZP.BzpAllergies/Sprites"));

                if (FarmerIsAllergic(itemToEat, Config, GameContent))
                {
                    // is it dairy and do we have the buff?
                    if (itemToEat.HasContextTag(GetAllergenContextTag(Allergens.DAIRY)) && __instance.hasBuff(LACTASE_PILLS_BUFF))
                    {
                        HUDMessage lactaseProtectionMessage = new("Good thing you took your lactase!");
                        lactaseProtectionMessage.messageSubject = itemToEat;
                        Game1.addHUDMessage(lactaseProtectionMessage);
                        return true;
                    }

                    // change edibility
                    itemToEat.Edibility = -20;

                    // add the allergic reaction buff
                    BuffAttributesData buffAttributesData = new()
                    {
                        Speed = -2,
                        Defense = -1,
                        Attack = -1,
                    };

                    BuffEffects effects = new(buffAttributesData);

                    SetBuffIcon(itemToEat, sprites, out Texture2D buffIcon);

                    // get texture of the item we ate for use in the buff icon
                    Buff reactionBuff = new(ALLERIC_REACTION_DEBUFF, "food", itemToEat.DisplayName,
                        120000, buffIcon, 0, effects,
                        true, "Allergic Reaction", "Probably shouldn't have eaten that...");
                    reactionBuff.glow = Microsoft.Xna.Framework.Color.Green;

                    __instance.applyBuff(reactionBuff);

                    // randomly apply nausea
                    if (new Random().NextDouble() < 0.50)
                    {
                        __instance.applyBuff(Buff.nauseous);
                    }
                }
                else if (itemToEat.QualifiedItemId.Equals("(O)BzpAllergies_AllergyMedicine"))
                {
                    // nausea is automatically removed. remove the reaction as well
                    __instance.buffs.Remove(ALLERIC_REACTION_DEBUFF);
                }
                else if (itemToEat.QualifiedItemId.Equals("(O)BzpAllergies_LactasePills"))
                {
                    // get that dairy immunity
                    Buff immuneBuff = new(LACTASE_PILLS_BUFF, "food", itemToEat.DisplayName,
                        120000, sprites, 3, null,
                        false, "Dairy Immunity", "Quick, eat the cheese!");

                    __instance.applyBuff(immuneBuff);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(DoneEating_Prefix)}:\n{ex}", LogLevel.Error);
                __state = int.MinValue;  // error value
            }
            return true; // run original logic
        }

        [HarmonyPostfix]
        static void DoneEating_Postfix(ref Farmer __instance, int __state)
        {
            try
            {
                StardewValley.Object? itemToEat = __instance.itemToEat as StardewValley.Object;
                if (itemToEat != null && __state != int.MinValue)
                {
                    // change edibility back to original value
                    itemToEat.Edibility = __state;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(DoneEating_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        private static void SetBuffIcon(StardewValley.Object itemToEat, Texture2D modSprites, out Texture2D result)
        {
            ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(itemToEat.QualifiedItemId);
            Texture2D itemToEatTexture = dataOrErrorItem.GetTexture();
            result = new(Game1.graphics.GraphicsDevice, 16, 16);

            Rectangle itemToEatSourceArea = dataOrErrorItem.GetSourceRect();

            Rectangle borderSourceArea = Game1.getSourceRectForStandardTileSheet(modSprites, 2, 16, 16);

            // patch the pixels
            // TODO: attribution
            //https://github.com/Pathoschild/SMAPI/blob/e8a86a0b98061d322c2af89af845ed9f5fd15468/src/SMAPI/Framework/Content/AssetDataForImage.cs#L78
  
            // this.PatchImageImpl(sourceData, source.Width, source.Height, sourceArea.Value, targetArea.Value, patchMode);

            // apply the itemToEat overlay
            TransparentOverlay(result, itemToEatTexture, itemToEatSourceArea);


            // apply the border overlay
            TransparentOverlay(result, modSprites, borderSourceArea);
        }

        private static void TransparentOverlay(Texture2D under, Texture2D over, Rectangle overSourceRect)
        {
            // TODO make this actually transparent lol
            // https://github.com/Pathoschild/SMAPI/blob/e8a86a0b98061d322c2af89af845ed9f5fd15468/src/SMAPI/Framework/Content/AssetDataForImage.cs#L53
            int pixelCount = overSourceRect.Width * overSourceRect.Height;
            int firstPixel = 0;
            int lastPixel = pixelCount - 1;

            Color[] sourceData = ArrayPool<Color>.Shared.Rent(pixelCount);  // no idea what this does
            try
            {
                over.GetData(0, overSourceRect, sourceData, 0, pixelCount);
                under.SetData(0, null, sourceData, firstPixel, pixelCount);
            }
            finally
            {
                ArrayPool<Color>.Shared.Return(sourceData);
            }
        }
    }
}
