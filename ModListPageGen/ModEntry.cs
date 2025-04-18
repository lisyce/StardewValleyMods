﻿using System.Text.RegularExpressions;
using HandlebarsDotNet;
using StardewModdingAPI;
using StardewValley;

namespace ModListPageGen;

public class ModEntry : Mod
{
    private static readonly string CSS = "https://cdn.jsdelivr.net/gh/lisyce/StardewValleyMods@main/ModListPageGen/style.min.css";
    private static readonly string JS = "https://cdn.jsdelivr.net/gh/lisyce/StardewValleyMods@main/ModListPageGen/script.min.js";
    
    private static readonly Regex NexusIdRegex = new(@"[-]?\d+");
        
    private IModHelper _helper;
    
    public override void Entry(IModHelper helper)
    {
        _helper = helper;

        _helper.ConsoleCommands.Add("bzp_mod_list",
            "Generates an HTML-formatted mod list. Usage: bzp_mod_list \"<title>\" \"<author>\" <(optional bool) skip Nexus API>",
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
            Monitor.Log("Not enough arguments provided to bzp_mod_list. Usage: bzp_mod_list \"<title>\" \"<author>\" <(optional bool) skip Nexus API>", LogLevel.Error);
            return;
        }
        var title = args[0];
        var author = args[1];
        var skipNexus = false;

        if (args.Length >= 3 && bool.TryParse(args[2].ToLower(), out var b))
        {
            skipNexus = b;
        }
        
        var len = _helper.ModRegistry.GetAll().Count();
        Monitor.Log($"Building Mod List of {len} Mods...", LogLevel.Info);

        var result = new List<ModInfo.TemplatedModInfo>();

        if (!skipNexus)
        {
            Monitor.Log("Getting information from Nexus API...", LogLevel.Info);
        }
        else
        {
            Monitor.Log("Skipping Nexus API calls because that argument was provided to this command. Category information may be unavailable.", LogLevel.Info);
        }
        
        var client = new NexusApiClient(Monitor);
        result = GetMods(_helper.ModRegistry.GetAll(), client, skipNexus).OrderBy(x => x.Name)
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
            DependencyTree = GetDependencyTree(Helper.ModRegistry.GetAll()),
            MainCssCDNLink = CSS,
            MainJsCDNLink = JS
        };
        var templated = template(data);
        
        Directory.CreateDirectory(Path.Combine(_helper.DirectoryPath, "GeneratedModLists"));
        
        var outputPath = Path.Combine(_helper.DirectoryPath, "GeneratedModLists", $"{MakeValidFileName(title)}.html");
        File.WriteAllText(outputPath, templated);
        Monitor.Log($"Saved mod list to {outputPath}.", LogLevel.Info);
    }

    private class DepTreeElement
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
                if (dep.UniqueID == null || !dep.IsRequired) continue;
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

            if (dependsOn.Count == 0)
            {
                if (!tree.ContainsKey("No Dependencies")) tree.Add("No Dependencies", new DepTreeElement { Name = "No Dependencies", ClassName = "No_Deps", DepsCount = 0 });
                tree["No Dependencies"].DepsCount += 1;
            }
        }
        
        var list = tree.Select(x => x.Value)
            .OrderByDescending(x => x.DepsCount)
            .ThenBy(x => x.Name)
            .ToList();
        return list;
    }
    
    private List<ModInfo> GetMods(IEnumerable<IModInfo> mods, NexusApiClient client, bool skipNexus)
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

        var nexusInfoResponse = new NexusApiClient.GetNexusInfoResponse();
        
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