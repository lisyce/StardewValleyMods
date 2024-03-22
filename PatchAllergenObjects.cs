using StardewModdingAPI.Events;
using StardewValley.GameData.Objects;
using static BZP_Allergies.AllergenManager;

namespace BZP_Allergies
{
    internal sealed class PatchAllergenObjects
    {

        public static void AddAllergen (AssetRequestedEventArgs e, Allergens allergen)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, ObjectData>();

                    ISet<string> idsToEdit = GetObjectsWithAllergen(allergen);
                    foreach (string id in idsToEdit)
                    {
                        // add context tags and edibility data
                        ObjectData objectData = editor.Data[id];
                        objectData.ContextTags.Add(GetAllergenContextTag(allergen));
                        objectData.Edibility = -20;  // same as normal red mushroom

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
