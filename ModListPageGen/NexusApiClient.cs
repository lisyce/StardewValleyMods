using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using StardewModdingAPI;

// ReSharper disable InconsistentNaming

namespace ModListPageGen;

public class NexusApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IMonitor _monitor;

    public bool Validated { get; private set; }
    public int HourlyRequestsLeft { get; private set; }

    private class NexusApiGetResponse
    {
        public string name { get; set; }
        public string summary { get; set; }
        public string picture_url { get; set; }
        public int mod_downloads { get; set; }
        public int mod_unique_downloads { get; set; }
        public int mod_id { get; set; }
        public int endorsement_count { get; set; }
        public bool contains_adult_content { get; set; }
    
        public NexusInfo ToNexusInfo()
        {
            return new NexusInfo(name, summary, picture_url, mod_downloads, mod_unique_downloads, mod_id,
                endorsement_count, contains_adult_content);
        }
    }

    public NexusApiClient(string apiKey, IMonitor monitor)
    {
        _monitor = monitor;
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("https://api.nexusmods.com/v1/"),
        };
        
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
        _httpClient.DefaultRequestHeaders.Add("Application-Version", "1.0.0");
        _httpClient.DefaultRequestHeaders.Add("Application-Name", "Shareable Mod List Generator");
        
        Validated = Validate();
    }
    
    public bool Validate(bool printReqLeft = false)
    {
        var task = _httpClient.GetAsync("users/validate");
        task.Wait();

        if (printReqLeft)
        {
            if (task.Result.Headers.TryGetValues("x-rl-daily-remaining", out var vals))
            {
                var remaining = vals.FirstOrDefault("0");
                _monitor.Log($"Daily API calls remaining: {remaining}", LogLevel.Info);
            }
        
            if (task.Result.Headers.TryGetValues("x-rl-hourly-remaining", out var vals2))
            {
                var remaining = vals2.FirstOrDefault("0");
                HourlyRequestsLeft = int.Parse(remaining);
                _monitor.Log($"Hourly API calls remaining: {remaining}", LogLevel.Info);
            }
        }
        

        Validated = task.Result.IsSuccessStatusCode;
        return task.Result.IsSuccessStatusCode;
    }
    
    public async Task<(NexusInfo? result, string errMsg)> GetNexusInfo(string modId)
    {
        if (!Validated) return (null, "Not Validated");
        
        // rate limit ourself
        await Task.Delay(100);
        
        var response = await _httpClient.GetAsync("games/stardewvalley/mods/" + modId);
        
        if (!response.IsSuccessStatusCode)
        {
            return (null, $"Failed to call Nexus API. Status Code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }
        
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<NexusApiGetResponse>(body);
        
        return json == null ? (null, "failed to deserialize response JSON") : (json.ToNexusInfo(), "");
    }
}