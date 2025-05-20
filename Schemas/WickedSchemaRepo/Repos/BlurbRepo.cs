using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json.Linq;

namespace WickedSchemaRepo;

public partial interface IBlurbRepo : IDocumentRepo<Blurb> {
    Task<ObjectResult> ListBlurbsByStatusAsync(ICallerInfo callerInfo, string indexValue, int limit = 0);
}
public partial class BlurbRepo
{
    protected override void AssignEntityAttributes(ICallerInfo callerInfo, JObject jobjectData, Dictionary<string, AttributeValue> dbrecord, long now)
    {
        base.AssignEntityAttributes(callerInfo, jobjectData, dbrecord, now);
        // Assign SK1 (Status) index
        var status = jobjectData["status"]?.ToString();
        if (status != null)
            dbrecord.Add("SK1", new AttributeValue { S = status });
    }
    public async Task<ObjectResult> ListBlurbsByStatusAsync(ICallerInfo callerInfo, string indexValue, int limit = 0)
    {
        return await ListAsync(callerInfo, "SK1", indexValue, limit);
    }
}
