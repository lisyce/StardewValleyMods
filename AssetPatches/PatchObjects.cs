using StardewModdingAPI.Events;
using StardewValley.GameData.Objects;
using static BZP_Allergies.AllergenManager;

namespace BZP_Allergies.AssetPatches
{
    internal sealed class PatchObjects
    {

        public static void AddAllergen(AssetRequestedEventArgs e, string allergen)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(asset =>
                {
                    // update items containing allergens
                    var editor = asset.AsDictionary<string, ObjectData>();

                    ISet<string> idsToEdit = GetObjectsWithAllergen(allergen, editor);
                    foreach (string id in idsToEdit)
                    {
                        // add context tags
                        ObjectData objectData = editor.Data[id];
                        objectData.ContextTags ??= new List<string>();
                        objectData.ContextTags.Add(GetAllergenContextTag(allergen));
                    }
                },
                AssetEditPriority.Late
                );
            }
        }

    }
}
