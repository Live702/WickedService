using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WickedSchemaRepo;

public partial interface IChatRepo : IDocumentRepo<Chat> {
    Task<ObjectResult> ListChatsByBlurbIdAsync(ICallerInfo callerInfo, string blurbId, int limit = 0);
    Task<ObjectResult> ListChatsByPremiseIdAsync(ICallerInfo callerInfo, string premiseId, int limit = 0);
}
public partial class ChatRepo
{
    [ActivatorUtilitiesConstructor]
    public ChatRepo(
        IAmazonDynamoDB client,
        IMessageRepo messageRepo
        ) : base(client) {
        this.messageRepo = messageRepo;
    }
    private IMessageRepo messageRepo;
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

    public override async Task<StatusCodeResult> DeleteAsync(ICallerInfo callerInfo, string id)
    {
        try
        {
            // Get all Messages belonging to this Chat
            var messagesResult = await messageRepo.ListMessagesByChatIdAsync(callerInfo, id);

            // Delete all Messages belonging to this Chat
            if (messagesResult.Value is IEnumerable<Message> messageList)
            {
                var messageDeleteTasks = messageList.Select(message => 
                    messageRepo.DeleteAsync(callerInfo, message.Id)
                ).ToList();
                
                if (messageDeleteTasks.Any())
                {
                    await Task.WhenAll(messageDeleteTasks);
                }
            }

            // Finally, delete the Chat itself
            return await base.DeleteAsync(callerInfo, id);
        }
        catch (Exception ex)
        {
            // Log the error and return a meaningful error response
            // For now, we'll re-throw the exception to maintain consistent error handling
            // In production, you might want to log this and return a specific status code
            throw new Exception($"Failed to delete Chat and its related records: {ex.Message}", ex);
        }
    }

}
