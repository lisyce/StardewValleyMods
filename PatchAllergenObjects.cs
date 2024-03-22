using BZP_Allergies.Config;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Objects;
using static BZP_Allergies.AllergenManager;

namespace BZP_Allergies
{
    internal sealed class PatchAllergenObjects
    {

        public static void AddAllergen (AssetRequestedEventArgs e, Allergens allergen, ModConfig config)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                bool allergic = FarmerIsAllergic(allergen, config);

                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, ObjectData>();

                    ISet<string> idsToEdit = GetObjectsWithAllergen(allergen);
                    foreach (string id in idsToEdit)
                    {
                        // add context tags and edibility data
                        ObjectData objectData = editor.Data[id];
                        objectData.ContextTags.Add(GetAllergenContextTag(allergen));
                        if (allergic)
                        {
                            objectData.Edibility = -20;  // same as normal red mushroom
                        }

                        // update descriptions
                        if (!objectData.Description.Contains("Allergens: "))
                        {
                            objectData.Description += "\nAllergens: ";
                        }
                        else
                        {
                            objectData.Description += ", ";
                        }
                        objectData.Description += GetAllergenReadableString(allergen);
                    }
                });
            }
        }
    }
}
