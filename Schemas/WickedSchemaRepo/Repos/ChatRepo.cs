using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json.Linq;

namespace WickedSchemaRepo;

public partial interface IChatRepo : IDocumentRepo<Chat> {
    Task<ObjectResult> ListChatsByBlurbIdAsync(ICallerInfo callerInfo, string blurbId, int limit = 0);
    Task<ObjectResult> ListChatsByPremiseIdAsync(ICallerInfo callerInfo, string premiseId, int limit = 0);
}
public partial class ChatRepo
{
    protected override void AssignEntityAttributes(ICallerInfo callerInfo, JObject jobjectData, Dictionary<string, AttributeValue> dbrecord, long now)
    {
        base.AssignEntityAttributes(callerInfo, jobjectData, dbrecord, now);
        // Assign SK1 (BlurbId) index
        var blurbId = jobjectData["blurbId"]?.ToString();
        if (blurbId != null)
            dbrecord.Add("SK1", new AttributeValue { S = blurbId });

        // Assign SK1 (PremiseId) index
        var premiseId = jobjectData["premiseId"]?.ToString();
        if (premiseId != null)
            dbrecord.Add("SK2", new AttributeValue { S = premiseId });
    }
    public async Task<ObjectResult> ListChatsByBlurbIdAsync(ICallerInfo callerInfo, string blurbId, int limit = 0)
    {
        return await ListAsync(callerInfo, "SK1", blurbId, limit);
    }

    public async Task<ObjectResult> ListChatsByPremiseIdAsync(ICallerInfo callerInfo, string premiseId, int limit = 0)
    {
        return await ListAsync(callerInfo, "SK2", premiseId, limit);
    }

}
