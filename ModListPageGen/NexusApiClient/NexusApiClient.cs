using System.Net.Http.Headers;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using StardewModdingAPI;
using StardewValley.Extensions;

// ReSharper disable InconsistentNaming

namespace ModListPageGen.NexusApiClient;

public class NexusApiClient
{
    public class GetNexusInfoResponse
    {
        public Dictionary<int, NexusInfo> foundNexusInfo = new();
        public HashSet<int> invalidNexusIds = new();
    }
    
    private readonly GraphQLHttpClient _graphQl;
    private readonly IMonitor _monitor;
    
    public NexusApiClient(IMonitor monitor)
    {
        _monitor = monitor;
        _graphQl = new GraphQLHttpClient(
            "https://api.nexusmods.com/v2/graphql",
            new NewtonsoftJsonSerializer());
        
        _graphQl.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _graphQl.HttpClient.DefaultRequestHeaders.Add("Application-Version", "2.0.0");
        _graphQl.HttpClient.DefaultRequestHeaders.Add("Application-Name", "Shareable Mod List Generator");
    }

    public async Task<GetNexusInfoResponse> GetNexusInfo(HashSet<int> modIds)
    {
        var result = new GetNexusInfoResponse();
        
        var legacyModIds = modIds.Select(x => new LegacyModId { gameId = 1303, modId = x})
            .Where(x => !result.invalidNexusIds.Contains(x.modId)).ToList();

        for (var batchSize = legacyModIds.Count / 2; batchSize >= 1 && legacyModIds.Count > 0; batchSize /= 2)
        {
            _monitor.Log($"Processing {legacyModIds.Count} Mods. Batch size: {batchSize}");
            var (sureOf, unsureOf) = await GetNexusInfoHelper(legacyModIds, batchSize);
            result.foundNexusInfo.TryAddMany(sureOf.foundNexusInfo);
            result.invalidNexusIds.AddRange(sureOf.invalidNexusIds);
            
            _monitor.Log($"Found {sureOf.foundNexusInfo.Keys.Count} valid mod Ids");
            foreach (var inv in sureOf.invalidNexusIds)
            {
                _monitor.Log($"Found invalid mod Id {inv}");
            }

            legacyModIds = unsureOf;

            var totalSureOf = result.foundNexusInfo.Count + result.invalidNexusIds.Count;
            _monitor.Log($"Got data for {totalSureOf}/{modIds.Count} Nexus update keys ({Math.Round((double) totalSureOf / modIds.Count * 100.0, 0)}% complete).", LogLevel.Debug);
        }

        return result;
    }
    
    private async Task<(GetNexusInfoResponse sureOf, List<LegacyModId> unsureOf)> GetNexusInfoHelper(List<LegacyModId> legacyModIds, int batchSize) {
        var unsureOf = new List<LegacyModId>();

        var sureOf = new GetNexusInfoResponse();

        for (var i = 0; i < legacyModIds.Count; i += batchSize)
        {
            var checkingNow = legacyModIds.Skip(i).Take(batchSize).ToList();

            var count = 0;
            _monitor.Log($"Checking {string.Join(", ", checkingNow.Select(x => x.modId))}");

            while (count < checkingNow.Count)
            {
                _monitor.Log($"offset: {count}");
                var request = GetGraphQlRequest(checkingNow, count);
                await Task.Delay(200);  // politely rate-limit ourself

                var graphQLResponse = await _graphQl.SendQueryAsync<LegacyMods>(request);
                if (graphQLResponse.Errors?.Length > 0)
                {
                    _monitor.Log(graphQLResponse.Errors[0].Message);
                    if (batchSize == 1)
                    {
                        sureOf.invalidNexusIds.AddRange(checkingNow.Select(x => x.modId));
                    }
                    else
                    {
                        unsureOf.AddRange(checkingNow);
                    }

                    break;  // don't bother with the rest of pagination
                }

                var data = graphQLResponse.Data.legacyMods.nodes.Select(x => x.ToNexusInfo())
                    .ToDictionary(x => x.ModId, x => x);
                _monitor.Log($"{data.Count} found");
                sureOf.foundNexusInfo.TryAddMany(data);
                count += data.Count;
            }
        }

        return (sureOf, unsureOf);
    }

    private GraphQLRequest GetGraphQlRequest(List<LegacyModId> modIds, int offset)
    {
        return new GraphQLRequest {
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
                ids = modIds.ToArray(),
                offset = offset,
            }
        };
    }
}