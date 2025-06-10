using StardewModdingAPI;
using StardewValley;

namespace ModListPageGen;

public class ConsoleCommandsHelper
{
    public static readonly string GenerateJsonUsage = 
        "Generates an json-formatted mod list that can later be converted to other formats. Usage: mod_list_json \"<title>\" \"<author>\" [(bool) skip Nexus API calls]. The last argument is optional.";

    public static readonly string ShareHtmlUsage =
        "Creates a shareable link for an existing json mod list and saves the resulting HTML to this mod's folder. Usage: mod_list_share \"<title>\", [theme]";
    
    public static bool TryParseGenerateJsonArgs(string[] args, out string title, out string author, out bool skipNexus, out string error)
    {
        // defaults
        title = "";
        author = "";
        skipNexus = false;
        error = "";
        
        if (!ArgUtility.TryGet(args, 0, out title, out error, allowBlank: false)) return false;
        if (!ArgUtility.TryGet(args, 1, out author, out error, allowBlank: false)) return false;

        if (!ArgUtility.TryGetOptionalBool(args, 3, out skipNexus, out error)) return false;
        return true;
    }

    public static bool TryParseShareHtmlArgs(string[] args, out string title, out string theme, out string error)
    {
        title = "";
        theme = "";
        
        if (!ArgUtility.TryGet(args, 0, out title, out error, allowBlank: false)) return false;
        if (!ArgUtility.TryGetOptional(args, 1, out theme, out error, "default", allowBlank: false)) return false;

        return true;
    }
}