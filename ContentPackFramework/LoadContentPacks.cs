using BZP_Allergies.Apis;
using BZP_Allergies.Config;
using StardewModdingAPI;

namespace BZP_Allergies.ContentPackFramework
{
    internal class LoadContentPacks : Initializable
    {
        public static void LoadPacks(IEnumerable<IContentPack> packs, ModConfig config)
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

        private static bool ProcessPack(IContentPack pack, ModConfig config)
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
            for (int i = 0; i < content.CustomAllergens.Count; i++)
            {
                CustomAllergen allergen = content.CustomAllergens[i];

                if (allergen.Name == null)
                {
                    Monitor.Log("No Name was specified for allergen at index " + i, LogLevel.Error);
                    return false;
                }

                if (allergen.Id == null)
                {
                    Monitor.Log("No Id was specified for allergen at index " + i, LogLevel.Error);
                    return false;
                }

                if (!AllergenManager.ALLERGEN_OBJECTS.ContainsKey(allergen.Id))
                {
                    AllergenManager.ALLERGEN_OBJECTS.Add(allergen.Id, new HashSet<string>());
                }

                if (!AllergenManager.ALLERGEN_CONTEXT_TAGS.ContainsKey(allergen.Id))
                {
                    AllergenManager.ALLERGEN_CONTEXT_TAGS.Add(allergen.Id, new HashSet<string>());
                }

                if (!AllergenManager.ALLERGEN_TO_DISPLAY_NAME.ContainsKey(allergen.Id))
                {
                    AllergenManager.ALLERGEN_TO_DISPLAY_NAME.Add(allergen.Id, allergen.Name);
                }

                if (!config.Farmer.allergies.ContainsKey(allergen.Id))
                {
                    config.Farmer.allergies.Add(allergen.Id, false);
                }
            }
        
            // allergen assignments
            for (int i = 0; i < content.AllergenAssignments.Count; i++)
            {
                AllergenAssignments allergenAssign = content.AllergenAssignments[i];

                if (allergenAssign.AllergenId == null)
                {
                    Monitor.Log("No AllergenId was specified for allergen assignment at index " + i, LogLevel.Error);
                    return false;
                }

                if (!AllergenManager.ALLERGEN_TO_DISPLAY_NAME.ContainsKey(allergenAssign.AllergenId))
                {
                    Monitor.Log("No allergen exists with Id " + allergenAssign.AllergenId, LogLevel.Error);
                    return false;
                }

                // object Ids
                foreach (string id in allergenAssign.ObjectIds)
                {
                    AllergenManager.ALLERGEN_OBJECTS[allergenAssign.AllergenId].Add(id);
                }

                // context tags
                foreach (string tag in allergenAssign.ContextTags)
                {
                    AllergenManager.ALLERGEN_CONTEXT_TAGS[allergenAssign.AllergenId].Add(tag);
                }
            }

            return true;  // no errors :)
        }
    }
}
