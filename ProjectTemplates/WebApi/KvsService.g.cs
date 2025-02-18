using System.Collections.Generic;
using Amazon.CloudFrontKeyValueStore;
using Amazon.CloudFrontKeyValueStore.Model;
using Amazon.Runtime;
using LazyMagic.Shared;
using Newtonsoft.Json;

public class DevKvsEntry
{
    public string SubtenantKey { get; set; }
    public string SystemKey { get; set; }
    public string TenantKey { get; set; }
    public string Ss { get; set; }
    public string Ts { get; set; }
    public string Sts { get; set; }
    public string Env { get; set; }
    public string Region { get; set; }
    public List<List<string>> Behaviors { get; set; } = new List<List<string>>();
}

public class DevApi
{
    public string Path { get; set; }
    public string BehaviorType { get; set; }
    public string ApiName { get; set; }
    public string Region { get; set; }
    public string Level { get; set; }
}

public class DevAsset
{
    public string Path { get; set; }
    public string BehaviorType { get; set; }
    public string Suffix { get; set; }
    public string Region { get; set; }
    public string Level { get; set; }
}

public class DevWebApp
{
    public string Path { get; set; }
    public string BehaviorType { get; set; }
    public string AppName { get; set; }
    public string Suffix { get; set; }
    public string Region { get; set; }
    public string Level { get; set; }
}

public class DevSubtenant
{
    public string Id { get; set; }
    public string SystemKey { get; set; }
    public string TenantKey { get; set; }
    public string SubtenantKey { get; set; }
    public string Ss { get; set; }
    public string Ts { get; set; }
    public string Sts { get; set; }
    public string Env { get; set; }
    public string Region { get; set; }
    public List<DevApi> Apis { get; set; } = new List<DevApi>();
    public List<DevAsset> Assets { get; set; } = new List<DevAsset>();
    public List<DevWebApp> WebApps { get; set; } = new List<DevWebApp>();
}

public partial class KvsService
{
    public Dictionary<string, DevKvsEntry> KvsEntries { get; set; } = new Dictionary<string, DevKvsEntry>();

    private IAmazonCloudFrontKeyValueStore? _client;

    /// <summary>
    /// Read the KVS entries for the tenant and return them as a dictionary
    /// Note that we transform the Behaviors array into the Apis, Assets, and WebApps collections.
    /// We don't store Apis, Assets, and WebApps in the KVS because they are would make the 
    /// KVSEntry too large to fit in the 1K value size limitof the AWS KVS.
    /// </summary>
    /// <param name="callerInfo"></param>
    /// <param name="kvsKey"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task GetKvsEntriesDictionaryAsync(string kvsarn, string kvsKey)
    {
        try
        {

            _client ??= new AmazonCloudFrontKeyValueStoreClient();

            var kvsEntries = new Dictionary<string, DevKvsEntry>();
            var request = new ListKeysRequest
            {
                KvsARN = kvsarn,
                MaxResults = 50,
                NextToken = null
            };
            do
            {
                var response = await _client.ListKeysAsync(request);
                request.NextToken = response.NextToken;
                foreach (var entry in response.Items)
                {
                    var key = entry.Key;
                    if (!key.StartsWith(kvsKey) || key.Equals(kvsKey)) continue;
                    var value = entry.Value;
                    var kvsEntry = JsonConvert.DeserializeObject<DevKvsEntry>(value);
                    if (kvsEntry == null)
                    {
                        throw new Exception("Failed to deserialize KvsEntry");
                    }

                    kvsEntries.Add(key, kvsEntry);
                }
            } while (request.NextToken != null);
            kvsEntries.Add(kvsKey, kvsEntries[kvsKey]);
        }
        catch (AmazonCloudFrontKeyValueStoreException ex)
        {
            Console.WriteLine($"CloudFront KVS specific error: {ex.Message}");
            Console.WriteLine($"Error type: {ex.ErrorType}");
            Console.WriteLine($"Request ID: {ex.RequestId}");
            throw;
        }
        catch (AmazonServiceException ex)
        {
            Console.WriteLine($"AWS service error: {ex.Message}");
            Console.WriteLine($"HTTP Status Code: {ex.StatusCode}");
            Console.WriteLine($"AWS Error Code: {ex.ErrorCode}");
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void ExtractFromKvsEntry(DevKvsEntry kvsEntry, DevSubtenant subtenant)
    {
        // process the Behaviors array to generate the Apis, Assets, and WebApps collections
        foreach (var behavior in kvsEntry!.Behaviors)
        {

            subtenant.Id = kvsEntry.SubtenantKey;
            subtenant.SystemKey = kvsEntry.SystemKey;
            subtenant.TenantKey = kvsEntry.TenantKey;
            subtenant.SubtenantKey = kvsEntry.SubtenantKey;
            subtenant.Ss = kvsEntry.Ss;
            subtenant.Ts = kvsEntry.Ts;
            subtenant.Sts = kvsEntry.Sts;
            subtenant.Env = kvsEntry.Env;
            subtenant.Region = kvsEntry.Region;

            var behaviorArray = behavior.ToArray();
            if (behaviorArray.Length == 0) continue;

            var behaviorType = behaviorArray[1];
            switch (behaviorType)
            {
                case "api":
                    var api = new DevApi
                    {
                        Path = behaviorArray[0],
                        BehaviorType = behaviorArray[1],
                        ApiName = behaviorArray[2],
                        Region = behaviorArray[3],
                        Level = behaviorArray[4]
                    };
                    subtenant.Apis.Add(api);
                    break;
                case "asset":
                    var asset = new DevAsset
                    {
                        Path = behaviorArray[0],
                        BehaviorType = behaviorArray[1],
                        Suffix = behaviorArray[2],
                        Region = behaviorArray[3],
                        Level = behaviorArray[4]
                    };
                    subtenant.Assets.Add(asset);
                    break;
                case "webapp":
                    var webapp = new DevWebApp
                    {
                        Path = behaviorArray[0],
                        BehaviorType = behaviorArray[1],
                        AppName = behaviorArray[2],
                        Suffix = behaviorArray[3],
                        Region = behaviorArray[4],
                        Level = behaviorArray[5]
                    };
                    subtenant.WebApps.Add(webapp);
                    break;
                default:
                    throw new Exception($"Unknown behavior type: {behaviorType}");
            }
        }
    }

}
