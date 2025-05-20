using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json.Linq;

namespace WickedSchemaRepo;

public partial interface IMessageRepo : IDocumentRepo<Message> { 
    public Task<ObjectResult> ListMessagesByChatIdAsync(ICallerInfo callerInfo, string blurbId, int limit = 0); 
}
public partial class MessageRepo
{
    protected override void AssignEntityAttributes(ICallerInfo callerInfo, JObject jobjectData, Dictionary<string, AttributeValue> dbrecord, long now)
    {
        base.AssignEntityAttributes(callerInfo, jobjectData, dbrecord, now);
        // Assign SK1 (ChatId) index
        var blurbId = jobjectData["chatId"]?.ToString();
        if (blurbId != null)
            dbrecord.Add("SK1", new AttributeValue { S = blurbId });
    }

    public async Task<ObjectResult> ListMessagesByChatIdAsync(ICallerInfo callerInfo, string blurbId, int limit = 0)
    {
        return await ListAsync(callerInfo, "SK1", blurbId, limit);
    }

}
