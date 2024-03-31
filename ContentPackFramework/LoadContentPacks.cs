using BZP_Allergies.Apis;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BZP_Allergies.ContentPackFramework
{
    internal class LoadContentPacks : Initializable
    {
        public static void LoadPacks(IEnumerable<IContentPack> packs, IGenericModConfigMenuApi configMenu)
        {
            foreach (IContentPack contentPack in packs)
            {
                Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
                
            }
        }
    }
}
