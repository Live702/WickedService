using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json.Linq;

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
        IChatRepo chatRepo
        ) : base(client) {
        this.chatRepo = chatRepo;
    }
    private IChatRepo chatRepo;
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
}
