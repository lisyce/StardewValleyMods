using System.Net.Http.Json;
using StardewModdingAPI;

namespace ModListPageGen;

// ReSharper disable InconsistentNaming

public class ShareableLinkClient
{
    private readonly HttpClient _httpClient;
    private readonly IMonitor _monitor;
    
    private class LinkResponse
    {
        public string id { get; set; }
    }

    public ShareableLinkClient(IMonitor monitor)
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("https://stardewmodlistserver.onrender.com/"),
        };
        _monitor = monitor;
    }

    public bool TryCreateLink(string html, out string link)
    {
        link = "";  // default value
        
        var task = _httpClient.PostAsJsonAsync("", new { html = html });
        task.Wait();
        
        if (!task.IsCompletedSuccessfully || !task.Result.IsSuccessStatusCode) return false;

        var result = task.Result.Content.ReadFromJsonAsync<LinkResponse>();
        result.Wait();
        if (!result.IsCompletedSuccessfully) return false;
        
        var id = result.Result.id;
        link = _httpClient.BaseAddress + id;
        
        return true;
    }
}