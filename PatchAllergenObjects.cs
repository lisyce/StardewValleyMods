using BZP_Allergies.Config;
using StardewModdingAPI.Events;
using StardewValley.GameData.Objects;
using static BZP_Allergies.AllergenManager;
using StardewValley.GameData.Machines;

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

                    ISet<string> idsToEdit = GetObjectsWithAllergen(allergen, editor);
                    foreach (string id in idsToEdit)
                    {
                        // add context tags
                        ObjectData objectData = editor.Data[id];
                        if (objectData.ContextTags == null)
                        {
                            objectData.ContextTags = new List<string>();
                        }
                        objectData.ContextTags.Add(GetAllergenContextTag(allergen));

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
