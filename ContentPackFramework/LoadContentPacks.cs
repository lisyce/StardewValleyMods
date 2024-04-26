using BZP_Allergies.Config;
using StardewModdingAPI;

namespace BZP_Allergies.ContentPackFramework
{
    internal class LoadContentPacks : Initializable
    {
        public static void LoadPacks(IEnumerable<IContentPack> packs, ModConfigModel config)
        {
            foreach (IContentPack contentPack in packs)
            {
                Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Info);
                if (!ProcessPack(contentPack, config))
                {
                    Monitor.Log($"Unable to read content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Error);
                }
            }
        }

        private static string SanitizeAllergenId(string id)
        {
            // remove "," "_" and lowercase
            return id.Replace(",", "").Replace("_", "").ToLower();
        }

        private static bool ProcessPack(IContentPack pack, ModConfigModel config)
        {
            ContentModel? content = pack.ReadJsonFile<ContentModel>("content.json");
            if (content == null)
            {
                // show 'required file missing' error
                Monitor.Log("Pack is missing a content.json, or it is empty.", LogLevel.Error);
                return false;
            }

            // check format
            if (content.Format == null || !content.Format.Equals("1.0.0"))
            {
                Monitor.Log("Valid content format was not specified. Valid formats are: \"1.0.0\"", LogLevel.Error);
                return false;
            }

            // custom allergens
            foreach (var pair in content.CustomAllergens)
            {
                CustomAllergen allergen = pair.Value;
                string allergenId = SanitizeAllergenId(pair.Key);

                if (allergen.Name == null)
                {
                    Monitor.Log("No Name was specified for allergen with Id " + allergenId, LogLevel.Error);
                    return false;
                }

                if (AllergenManager.ALLERGEN_DATA.ContainsKey(allergenId))
                {
                    Monitor.Log("Allergen with Id " + allergenId + " already exists. Skipping...", LogLevel.Warn);
                    break;
                }

                AllergenManager.ALLERGEN_DATA.Add(allergenId, new(allergen.Name, pack.Manifest.UniqueID, pack.Manifest.Name));
            }

            // allergen assignments
            foreach (var pair in content.AllergenAssignments)
            {
                AllergenAssignments allergenAssign = pair.Value;
                string allergenId = SanitizeAllergenId(pair.Key);

                if (allergenId == null)
                {
                    Monitor.Log("No AllergenId was specified for allergen assignment with Id " + allergenId, LogLevel.Error);
                    return false;
                }

                AllergenModel model = AllergenManager.ALLERGEN_DATA[allergenId];

                // object Ids
                model.AddObjectIds(allergenAssign.ObjectIds);

                // context tags
                model.AddTags(allergenAssign.ContextTags);
            }

            return true;  // no errors :)
        }
    }
}
