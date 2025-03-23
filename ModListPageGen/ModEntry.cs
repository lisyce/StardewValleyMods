using System.Text.RegularExpressions;
using HandlebarsDotNet;
using StardewModdingAPI;

namespace ModListPageGen;

public class ModEntry : Mod
{
    private static readonly Regex NexusIdRegex = new(@"\d+");
        
    private IModHelper _helper;
    
    public override void Entry(IModHelper helper)
    {
        _helper = helper;

        _helper.ConsoleCommands.Add("bzp_mod_list",
            "Generates an HTML-formatted mod list. Usage: bzp_mod_list \"<title>\" \"<author>\" <nexus API key (optional)>",
            GenerateList);
    }

    public void GenerateList(string command, string[] args)
    {
        if (args.Length < 2)
        {
            Monitor.Log("Not enough arguments provided to bzp_mod_list. Usage: bzp_mod_list \"<title>\" \"<author>\" <nexus API key (optional)>", LogLevel.Error);
            return;
        }
        var title = args[0];
        var author = args[1];

        var apiKey = args.Length > 2 ? args[2] : "NO_API_KEY";
        
        var client = new NexusApiClient(apiKey, Monitor);

        if (!client.Validated)
        {
            Monitor.Log("Could not validate with the Nexus Mods API.", LogLevel.Warn);
        }
        
        Monitor.Log($"Building Mod List of {_helper.ModRegistry.GetAll().Count()} Mods...", LogLevel.Info);
        
        var tasks = _helper.ModRegistry.GetAll().Select(x => GetModInfo(x, client)).ToList();
        
        var completed = Task.WhenAll(tasks);
        try {
            completed.Wait();
            
            var source = File.ReadAllText(_helper.DirectoryPath + "/template.html");
            var template = Handlebars.Compile(source);

            var modData = completed.Result.Select(x => x.Item1?.ToTemplate())
                .Where(x => x is not null)
                .OrderBy(x => x!.Name);
            var data = new
            {
                Title = title,
                Author = author,
                ModCount = modData.Count(),
                Mods = modData,
                NotShownCount = modData.Count(x => !x.HasNexus),
            };
            var result = template(data);

            Directory.CreateDirectory(Path.Combine(_helper.DirectoryPath, "GeneratedModLists"));
            
            var outputPath = Path.Combine(_helper.DirectoryPath, "GeneratedModLists", $"{MakeValidFileName(title)}.html");
            File.WriteAllText(outputPath, result);
            Monitor.Log($"Saved mod list to {outputPath}.", LogLevel.Info);
        }
        catch (Exception e)
        {
            Monitor.Log($"Failed to generate mod list: {e}", LogLevel.Error);
        }
    }

    private async Task<(ModInfo? result, string errMsg)> GetModInfo(IModInfo info, NexusApiClient client)
    {
        // does this mod have update keys?
        if (TryGetNexusModId(info.Manifest, out string nexusId))
        {
            // call the nexus API
            var (result, errMsg) = await client.GetNexusInfo(nexusId);
            return (new ModInfo(info.Manifest, result), errMsg);
        }

        return (new ModInfo(info.Manifest, null), "No Nexus update keys found");
    }

    private static bool TryGetNexusModId(IManifest manifest, out string nexusId)
    {
        foreach (var key in manifest.UpdateKeys)
        {
            if (key.ToLower().Contains("nexus"))
            {
                nexusId = NexusIdRegex.Match(key).Value;
                return true;
            }
        }

        nexusId = "-1";
        return false;
    }
    
    private static string MakeValidFileName(string name)
    {
        foreach (var ic in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(ic, '_');
        }

        return name;
    }
}