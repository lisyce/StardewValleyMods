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
            "Generates an HTML-formatted mod list. Usage: bzp_mod_list \"<title>\" \"<author>\"",
            GenerateList);
        _helper.ConsoleCommands.Add("bzp_share_mod_list", "Creates a shareable link for an existing mod list. Usage: bzp_share_mod_list \"<title>\"",
            ShareList);
    }

    private void ShareList(string command, string[] args)
    {
        if (args.Length < 1)
        {
            Monitor.Log("Not enough arguments provided to bzp_share_mod_list. Usage: bzp_share_mod_list \"<title>\"", LogLevel.Error);
            return;
        }
        var title = args[0];
        
        // does the list exist?
        var outputPath = Path.Combine(_helper.DirectoryPath, "GeneratedModLists", $"{MakeValidFileName(title)}.html");
        if (!File.Exists(outputPath))
        {
            Monitor.Log("Mod list with that name could not be found. Is there a file with that name in the <this mod's folder>/GeneratedModLists folder?", LogLevel.Error);
            return;
        }
        Monitor.Log("Generating shareable link. This may take a few seconds...", LogLevel.Info);
        
        var html = File.ReadAllText(outputPath);
        // post
        var client = new ShareableLinkClient(Monitor);
        if (!client.TryCreateLink(html, out string link))
        {
            Monitor.Log("Could not create shareable link. Please try again in a few minutes.", LogLevel.Error);
            return;
        }
        
        Monitor.Log($"Shareable link generated: {link}", LogLevel.Info);
        Monitor.Log("Links are valid for four weeks.", LogLevel.Debug);
    }

    private void GenerateList(string command, string[] args)
    {
        if (args.Length < 2)
        {
            Monitor.Log("Not enough arguments provided to bzp_mod_list. Usage: bzp_mod_list \"<title>\" \"<author>\"", LogLevel.Error);
            return;
        }
        var title = args[0];
        var author = args[1];
        
        var client = new NexusApiClient(Monitor);

        int len = _helper.ModRegistry.GetAll().Count();
        Monitor.Log($"Building Mod List of {len} Mods...", LogLevel.Info);
        
        var result = GetMods(_helper.ModRegistry.GetAll(), client).OrderBy(x => x.Name)
            .Select(x => x.ToTemplate(_helper)).ToList();
        
        var source = File.ReadAllText(_helper.DirectoryPath + "/template.html");
        var template = Handlebars.Compile(source);
        
        var data = new
        {
            Title = title,
            Author = author,
            ModCount = result.Count,
            Mods = result,
            Categories = result.GroupBy(x => x.CategoryName)
                .Select(x => new
                {
                    CategoryName = x.First().CategoryName,
                    CategoryClass = x.First().CategoryClass,
                    CategoryCount = x.Count()
                })
                .ToList(),
            DependencyTree = GetDependencyTree(Helper.ModRegistry.GetAll())
        };
        var templated = template(data);
        
        Directory.CreateDirectory(Path.Combine(_helper.DirectoryPath, "GeneratedModLists"));
        
        var outputPath = Path.Combine(_helper.DirectoryPath, "GeneratedModLists", $"{MakeValidFileName(title)}.html");
        File.WriteAllText(outputPath, templated);
        Monitor.Log($"Saved mod list to {outputPath}.", LogLevel.Info);
    }

    public class DepTreeElement
    {
        public string Name { get; set; }
        public int DepsCount { get; set; }
        public string ClassName { get; set; }
    }
    
    private List<DepTreeElement> GetDependencyTree(IEnumerable<IModInfo> mods)
    {
        var tree = new Dictionary<string, DepTreeElement>();
        foreach (var mod in mods)
        {
            // find everything this mod depends on
            HashSet<string> dependsOn = new();
            if (mod.Manifest.ContentPackFor?.UniqueID != null)
            {
                dependsOn.Add(mod.Manifest.ContentPackFor.UniqueID);
            }

            foreach (var dep in mod.Manifest.Dependencies)
            {
                if (dep.UniqueID == null) continue;
                dependsOn.Add(dep.UniqueID);
            }
            
            // add to the output reverse lookup
            foreach (var uniqueId in dependsOn)
            {
                var depMod = Helper.ModRegistry.Get(uniqueId);
                if (depMod == null) continue;
                
                if (!tree.ContainsKey(uniqueId)) tree.Add(uniqueId, new DepTreeElement { Name = depMod.Manifest.Name, ClassName = depMod.Manifest.Name.Replace(" ", "_"), DepsCount = 0 });
                tree[uniqueId].DepsCount += 1;
            }
        }
        
        var list = tree.Select(x => x.Value)
            .OrderByDescending(x => x.DepsCount)
            .ThenBy(x => x.Name)
            .ToList();
        return list;
    }
    
    private List<ModInfo> GetMods(IEnumerable<IModInfo> mods, NexusApiClient client)
    {
        List<ModInfo> result = new();
        Dictionary<int, string> nexusIds = new();
        
        foreach (var mod in mods)
        {
            // does this mod have nexus update keys?
            if (TryGetNexusModId(mod.Manifest, out int id))
            {
                nexusIds.TryAdd(id, mod.Manifest.UniqueID);
            }
            else
            {
                result.Add(new ModInfo(mod.Manifest, null));
            }
        }
        
        // call the api for all the nexus Ids
        var task = client.GetMods(nexusIds.Keys.ToHashSet());
        try
        {
            task.Wait();
            foreach (var nexusInfo in task.Result)
            {
                var uniqueId = nexusIds[nexusInfo.ModId];
                var modInfo = Helper.ModRegistry.Get(uniqueId);
                result.Add(new ModInfo(modInfo.Manifest, nexusInfo, nexusInfo.ModId.ToString()));
            }
        }
        catch (Exception ex)
        {
            Monitor.Log("Could not get Mods from Nexus API.", LogLevel.Error);
            Monitor.Log(ex.Message, LogLevel.Error);
        }
        
        return result;
    }

    private static bool TryGetNexusModId(IManifest manifest, out int nexusId)
    {
        foreach (var key in manifest.UpdateKeys)
        {
            if (key.ToLower().Contains("nexus"))
            {
                var strId = NexusIdRegex.Match(key).Value;
                if (int.TryParse(strId, out nexusId))
                {
                    return true;
                }
            }
        }

        nexusId = -1;
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