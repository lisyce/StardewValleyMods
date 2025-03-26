using System.Net.Http.Headers;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using StardewModdingAPI;

// ReSharper disable InconsistentNaming

namespace ModListPageGen;

public class NexusApiClient
{
    private readonly GraphQLHttpClient _graphQl;
    private readonly IMonitor _monitor;
    
    public NexusApiClient(IMonitor monitor)
    {
        _monitor = monitor;
        _graphQl = new GraphQLHttpClient(
            "https://api.nexusmods.com/v2/graphql",
            new NewtonsoftJsonSerializer());
        
        _graphQl.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _graphQl.HttpClient.DefaultRequestHeaders.Add("Application-Version", "1.0.0");
        _graphQl.HttpClient.DefaultRequestHeaders.Add("Application-Name", "Shareable Mod List Generator");
    }

    public async Task<Dictionary<int, NexusInfo>> GetMods(HashSet<int> modIds)
    {
        var loggedHalfway = false;
        List<GraphqlSchemas.LegacyModId> legacyModIds = modIds.Select(x => new GraphqlSchemas.LegacyModId { gameId = 1303, modId = x}).ToList();
        
        List<NexusInfo> nexusInfoList = new List<NexusInfo>();
        while (nexusInfoList.Count < legacyModIds.Count)
        {
            await Task.Delay(200);  // politely rate-limit ourself

            if (!loggedHalfway && nexusInfoList.Count() >= legacyModIds.Count / 2)
            {
                _monitor.Log("Halfway done!", LogLevel.Debug);
                loggedHalfway = true;
            }
            
            var request = new GraphQLRequest {
                Query = @"
                query legacyMods($ids: [CompositeIdInput!]!, $offset: Int) {
                    legacyMods(
                        ids: $ids,
                        offset: $offset,
                      ) {
                       nodes {
                            name 
                            summary
                            pictureUrl
                            downloads
                            endorsements
                            adultContent
                            modId
                            modCategory {
                                name
                            }
                        }
                        nodesCount
                    }
                }",
                OperationName = "legacyMods",
                Variables = new {
                    ids = legacyModIds.ToArray(),
                    offset = nexusInfoList.Count,
                }
            };
        
            var graphQLResponse = await _graphQl.SendQueryAsync<GraphqlSchemas.LegacyModsType>(request);
            if (graphQLResponse.Errors?.Length > 0)
            {
                _monitor.Log("Failed to call the Nexus API, but your mod list will still be generated. Category filters may be unavailable. Logging the error at TRACE level...", LogLevel.Warn);
                _monitor.Log(graphQLResponse.Errors?[0].Message ?? "Unknown Error");
                return new Dictionary<int, NexusInfo>();
            }
        
            var result = graphQLResponse.Data.legacyMods.nodes.Select(x => x.ToNexusInfo()).ToList();
            nexusInfoList.AddRange(result);
        }
        
        return nexusInfoList.ToDictionary(x => x.ModId, x => x);
    }
}