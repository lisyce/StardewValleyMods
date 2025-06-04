using System.Text.RegularExpressions;
using ModListPageGen.ShareableLinkClient;
using StardewModdingAPI;
using StardewValley;

namespace ModListPageGen;

public class ModEntry : Mod
{
    private static readonly Regex NexusIdRegex = new(@"[-]?\d+");
    private IModHelper _helper;
    
    public override void Entry(IModHelper helper)
    {
        _helper = helper;

        _helper.ConsoleCommands.Add("mod_list_json", ConsoleCommandsHelper.GenerateJsonUsage, GenerateJson);
        _helper.ConsoleCommands.Add("mod_list_share", ConsoleCommandsHelper.ShareHtmlUsage, ShareList);
    }

    private void ShareList(string command, string[] args)
    {
        if (!ConsoleCommandsHelper.TryParseShareHtmlArgs(args, out var title, out var theme, out var error))
        {
            Monitor.Log(error, LogLevel.Warn);
            return;
        }
        
        // does the list exist?
        var jsonOutputPath = Path.Combine("GeneratedModListsJson", $"{MakeValidFileName(title)}.json");
        var modListJson = Helper.Data.ReadJsonFile<ModList>(jsonOutputPath);
        if (modListJson == null)
        {
            Monitor.Log("Mod list with that name could not be found. Is there a file with that name in the <this mod's folder>/GeneratedModLists folder?", LogLevel.Error);
            return;
        }
        
        Monitor.Log("Generating shareable link. This may take a few seconds...", LogLevel.Info);
        
        
        // post
        var client = new ShareableLinkClient.ShareableLinkClient(Monitor);
        if (!client.TryCreateLink(modListJson, theme, out string link, out string html))
        {
            Monitor.Log("Could not create shareable link. Please try again in a few minutes.", LogLevel.Error);
            return;
        }
        
        Monitor.Log($"Shareable link generated: {link}", LogLevel.Info);
        Monitor.Log("Links are valid for four weeks.", LogLevel.Debug);
        
        // create dir if necessary
        Directory.CreateDirectory(Path.Combine(_helper.DirectoryPath, "GeneratedModListsHtml"));
        
        // write html
        var htmlOutputPath = Path.Combine(_helper.DirectoryPath, "GeneratedModListsHtml", $"{MakeValidFileName(title)}.html");
        File.WriteAllText(htmlOutputPath, html);
        Monitor.Log($"Saved your HTML mod list to GeneratedModListsHtml/{MakeValidFileName(title)}.", LogLevel.Info);
    }

    private void GenerateJson(string command, string[] args)
    {
        if (!ConsoleCommandsHelper.TryParseGenerateJsonArgs(args, out var title, out var author, out var skipNexus, out var error))
        {
            Monitor.Log(error, LogLevel.Warn);
            return;
        }
        
        var len = _helper.ModRegistry.GetAll().Count();
        Monitor.Log($"Building Mod List of {len} Mods...", LogLevel.Info);

        var result = new List<ModListMod>();

        if (!skipNexus)
        {
            Monitor.Log("Getting information from Nexus API...", LogLevel.Info);
        }
        else
        {
            Monitor.Log("Skipping Nexus API calls. Category information may be unavailable.", LogLevel.Info);
        }
        
        var client = new NexusApiClient.NexusApiClient(Monitor);
        result = GetMods(_helper.ModRegistry.GetAll(), client, skipNexus).OrderBy(x => x.Name)
            .Select(x => x.ToModListMod(_helper)).ToList();

        var categories = result.GroupBy(x => x.CategoryName)
            .Select(x => new Category(x.First().CategoryName, x.First().CategoryClass, x.Count())).ToList();

        var dependencyList = GetDependencyList(_helper.ModRegistry.GetAll());

        var timeCreated = DateTime.Now;
        var timeCreatedString = $"{timeCreated:O}";
        var data = new ModList(title, author, result.Count, result, categories, dependencyList, timeCreatedString, Game1.version, Constants.ApiVersion.ToString());

        // create dir if necessary
        Directory.CreateDirectory(Path.Combine(_helper.DirectoryPath, "GeneratedModListsJson"));
        
        // write json
        var outputPath = Path.Combine("GeneratedModListsJson", $"{MakeValidFileName(title)}.json");
        Helper.Data.WriteJsonFile(outputPath, data);
        
        Monitor.Log($"Saved mod list to {outputPath}.", LogLevel.Info);
    }
    
    private List<DependencyListEntry> GetDependencyList(IEnumerable<IModInfo> mods)
    {
        var tree = new Dictionary<string, DependencyListEntry>();
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
                if (dep.UniqueID == null || !dep.IsRequired) continue;
                dependsOn.Add(dep.UniqueID);
            }
            
            // add to the output reverse lookup
            foreach (var uniqueId in dependsOn)
            {
                var depMod = Helper.ModRegistry.Get(uniqueId);
                if (depMod == null) continue;

                var lowered = uniqueId.ToLower();
                if (!tree.ContainsKey(lowered)) tree.Add(lowered, new DependencyListEntry { Name = depMod.Manifest.Name, CssClass = depMod.Manifest.Name.Replace(" ", "_"), DepsCount = 0 });
                tree[lowered].DepsCount += 1;
            }

            if (dependsOn.Count == 0)
            {
                if (!tree.ContainsKey("No Dependencies")) tree.Add("No Dependencies", new DependencyListEntry { Name = "No Dependencies", CssClass = "No_Deps", DepsCount = 0 });
                tree["No Dependencies"].DepsCount += 1;
            }
        }
        
        var list = tree.Select(x => x.Value)
            .OrderByDescending(x => x.DepsCount)
            .ThenBy(x => x.Name)
            .ToList();
        return list;
    }
    
    private List<ModInfo> GetMods(IEnumerable<IModInfo> mods, NexusApiClient.NexusApiClient client, bool skipNexus)
    {
        List<ModInfo> result = new();
        Dictionary<string, int> nexusIds = new();
        HashSet<(string name, string uniqueId, string nexusId)> modsWithInvalidIds = new();

        foreach (var mod in mods)
        {
            // does this mod have nexus update keys?
            if (TryGetNexusModId(mod.Manifest, out int id, out string verbatimUpdateKey))
            {
                nexusIds.TryAdd(mod.Manifest.UniqueID, id);
            }
            else if (verbatimUpdateKey.Length > 0)
            {
                modsWithInvalidIds.Add((mod.Manifest.Name, mod.Manifest.UniqueID, verbatimUpdateKey));
            }
        }

        var nexusInfoResponse = new NexusApiClient.NexusApiClient.GetNexusInfoResponse();
        
        if (!skipNexus)
        {
            var task = client.GetNexusInfo(nexusIds.Values.ToHashSet());
            try
            {
                task.Wait();
                nexusInfoResponse = task.Result;
            }
            catch (Exception ex)
            {
                Monitor.Log("Could not add Nexus info to mod list.", LogLevel.Error);
                Monitor.Log(ex.Message, LogLevel.Error);
            }
        }
        
        foreach (var mod in mods)
        {
            if (nexusIds.TryGetValue(mod.Manifest.UniqueID, out var modNexusId))
            {
                if (nexusInfoResponse.foundNexusInfo.TryGetValue(modNexusId, out var nexusInfo))
                {
                    // we have data!
                    result.Add(new ModInfo(mod.Manifest, nexusInfo, modNexusId.ToString()));
                }
                else if (nexusInfoResponse.invalidNexusIds.Contains(modNexusId))
                {
                    modsWithInvalidIds.Add((mod.Manifest.Name, mod.Manifest.UniqueID, modNexusId.ToString()));
                }
                else
                {
                    result.Add(new ModInfo(mod.Manifest, null, modNexusId.ToString()));
                }
            }
            else
            {
                result.Add(new ModInfo(mod.Manifest, null));
            }
        }

        if (modsWithInvalidIds.Any())
        {
            Monitor.Log($"{modsWithInvalidIds.Count} mods have invalid Nexus update keys. Your list will still be generated!", LogLevel.Info);
            foreach (var (name, uniqueId, nexusId) in modsWithInvalidIds)
            {
                Monitor.Log($"{name} ({uniqueId}): \"{nexusId}\".", LogLevel.Debug);    
            }
        }

        return result;
    }

    private bool TryGetNexusModId(IManifest manifest, out int nexusId, out string verbatimUpdateKey)
    {
        verbatimUpdateKey = "";
        nexusId = -1;

        try
        {
            foreach (var key in manifest.UpdateKeys)
            {
                if (key.ToLower().Contains("nexus"))
                {
                    verbatimUpdateKey = key;
                }

                if (key.ToLower().Contains("nexus:"))
                {
                    var strId = NexusIdRegex.Match(key).Value;
                    if (int.TryParse(strId, out nexusId) && nexusId > 0)
                    {
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Monitor.Log($"TryGetNexusModId failed for {manifest.Name} ({manifest.UniqueID}). {ex.Message}");
        }
        

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