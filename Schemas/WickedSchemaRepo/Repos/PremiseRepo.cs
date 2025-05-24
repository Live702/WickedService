using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WickedSchemaRepo;

public partial interface IPremiseRepo : IDocumentRepo<Premise> { 
    public Task<ObjectResult> ListPremisesByBlurbIdAsync(ICallerInfo callerInfo, string blurbId, int limit = 0);
}
public partial class PremiseRepo
{
    [ActivatorUtilitiesConstructor]
    public PremiseRepo(
        IAmazonDynamoDB client,
        IChatRepo chatRepo,
        IMessageRepo messageRepo
        ) : base(client) {
        this.chatRepo = chatRepo;
        this.messageRepo = messageRepo;
    }
    private IChatRepo chatRepo;
    private IMessageRepo messageRepo;
    protected override void AssignEntityAttributes(ICallerInfo callerInfo, JObject jobjectData, Dictionary<string, AttributeValue> dbrecord, long now)
    {
        base.AssignEntityAttributes(callerInfo, jobjectData, dbrecord, now);
        // Assign SK1 (BlurbId) index
        var blurbId = jobjectData["blurbId"]?.ToString();
        if (blurbId != null)
            dbrecord.Add("SK1", new AttributeValue { S = blurbId });
    }

    public async Task<ObjectResult> ListPremisesByBlurbIdAsync(ICallerInfo callerInfo, string blurbId, int limit = 0)
    {
        return await ListAsync(callerInfo, "SK1", blurbId, limit);
    }

    public override async Task<StatusCodeResult> DeleteAsync(ICallerInfo callerInfo, string id)
    {
        try
        {
            // Create tasks to gather all child records in parallel
            var chatsTask = chatRepo.ListChatsByPremiseIdAsync(callerInfo, id);
            var messagesTask = messageRepo.ListMessagesByPremiseIdAsync(callerInfo, id);

            // Wait for all list operations to complete
            await Task.WhenAll(chatsTask, messagesTask);

            // Delete all Chats belonging to this Premise
            if (chatsTask.Result.Value is IEnumerable<Chat> chatList)
            {
                var chatDeleteTasks = chatList.Select(chat => 
                    chatRepo.DeleteAsync(callerInfo, chat.Id)
                ).ToList();
                
                if (chatDeleteTasks.Any())
                {
                    await Task.WhenAll(chatDeleteTasks);
                }
            }

            // Delete all Messages belonging to this Premise
            if (messagesTask.Result.Value is IEnumerable<Message> messageList)
            {
                var messageDeleteTasks = messageList.Select(message => 
                    messageRepo.DeleteAsync(callerInfo, message.Id)
                ).ToList();
                
                if (messageDeleteTasks.Any())
                {
                    await Task.WhenAll(messageDeleteTasks);
                }
            }

            // Finally, delete the Premise itself
            return await base.DeleteAsync(callerInfo, id);
        }
        catch (Exception ex)
        {
            // Log the error and return a meaningful error response
            // For now, we'll re-throw the exception to maintain consistent error handling
            // In production, you might want to log this and return a specific status code
            throw new Exception($"Failed to delete Premise and its related records: {ex.Message}", ex);
        }
    }

}
