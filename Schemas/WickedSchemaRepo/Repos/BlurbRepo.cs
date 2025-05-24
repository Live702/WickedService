using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WickedSchemaRepo;

public partial interface IBlurbRepo : IDocumentRepo<Blurb> {
    Task<ObjectResult> ListBlurbsByStatusAsync(ICallerInfo callerInfo, string indexValue, int limit = 0);
    Task<ObjectResult> SeedBlurbAsync(ICallerInfo callerInfo);
}
public partial class BlurbRepo
{
    [ActivatorUtilitiesConstructor]
    public BlurbRepo(
        IAmazonDynamoDB client, 
        IChatRepo chatRepo,
        IPremiseRepo premiseRepo,
        IMessageRepo messageRepo
        ) : base(client) {
        this.chatRepo = chatRepo;
        this.premiseRepo = premiseRepo;
        this.messageRepo = messageRepo;
    }
    private IChatRepo chatRepo;
    private IPremiseRepo premiseRepo;
    private IMessageRepo messageRepo;
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
    public async Task<ObjectResult> SeedBlurbAsync(ICallerInfo callerInfo)
    {
        try
        {
            var blurb = new Blurb
            {
                Id = "TestBlurb",
                Name= "Test Blurb"
            };

            var result = await CreateAsync(callerInfo, blurb);
            var chat = new Chat
            {
                Id = "TestChat",
                BlurbId = blurb.Id
            };
            var chatResult = await chatRepo.CreateAsync(callerInfo, chat);
            return new ObjectResult(blurb);
        } catch (Exception ex)
        {
            return new ObjectResult(new { error = ex.Message })
            {
                StatusCode = 500
            };
        }

    }

    public override async Task<StatusCodeResult> DeleteAsync(ICallerInfo callerInfo, string id)
    {
        try
        {
            // Create tasks to gather all child records in parallel
            var messagesTask = messageRepo.ListMessagesByBlurbIdAsync(callerInfo, id);
            var premisesTask = premiseRepo.ListPremisesByBlurbIdAsync(callerInfo, id);
            var chatsTask = chatRepo.ListChatsByBlurbIdAsync(callerInfo, id);

            // Wait for all list operations to complete
            await Task.WhenAll(messagesTask, premisesTask, chatsTask);

            // Delete all Messages belonging to this Blurb
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

            // Delete all Premises belonging to this Blurb
            if (premisesTask.Result.Value is IEnumerable<Premise> premiseList)
            {
                var premiseDeleteTasks = premiseList.Select(premise => 
                    premiseRepo.DeleteAsync(callerInfo, premise.Id)
                ).ToList();
                
                if (premiseDeleteTasks.Any())
                {
                    await Task.WhenAll(premiseDeleteTasks);
                }
            }

            // Delete all Chats belonging to this Blurb
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

            // Finally, delete the Blurb itself
            return await base.DeleteAsync(callerInfo, id);
        }
        catch (Exception ex)
        {
            // Log the error and return a meaningful error response
            // For now, we'll re-throw the exception to maintain consistent error handling
            // In production, you might want to log this and return a specific status code
            throw new Exception($"Failed to delete Blurb and its related records: {ex.Message}", ex);
        }
    }
}
