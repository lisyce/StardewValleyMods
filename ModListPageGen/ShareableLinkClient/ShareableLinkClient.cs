using System.Net.Http.Json;
using StardewModdingAPI;
using StardewValley;

namespace ModListPageGen.ShareableLinkClient;

// ReSharper disable InconsistentNaming

public class ShareableLinkClient
{
    private readonly HttpClient _httpClient;
    private readonly IMonitor _monitor;
    
    private class LinkResponse
    {
        public string LinkId { get; set; }
        public string RawHTML { get; set; }
    }

    public ShareableLinkClient(IMonitor monitor)
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:8080/"),
        };
        _monitor = monitor;
    }

    public bool TryCreateLink(ModList list, string theme, out string link, out string html)
    {
        link = "";  // default value
        html = "";

        try
        {
            var req = new ModPageData(list, theme);
            var task = _httpClient.PostAsJsonAsync("mod-lists", req);
            task.Wait();
            
            if (!task.IsCompletedSuccessfully || !task.Result.IsSuccessStatusCode)
            {
                _monitor.Log(task.Result?.ReasonPhrase);
                return false;
            }
            
            var result = task.Result.Content.ReadFromJsonAsync<LinkResponse>();
            result.Wait();
            if (!result.IsCompletedSuccessfully || result.Result == null) return false;

            link = _httpClient.BaseAddress + "mod-lists/" + result.Result.LinkId;
            html = result.Result.RawHTML;

            return true;
        }
        catch (Exception e)
        {
            _monitor.Log(e.Message);
            _monitor.Log(e.StackTrace);
            return false;
        }
    }
}