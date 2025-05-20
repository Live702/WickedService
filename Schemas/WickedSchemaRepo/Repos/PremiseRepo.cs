using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json.Linq;

namespace WickedSchemaRepo;

public partial interface IPremiseRepo : IDocumentRepo<Premise> { 
    public Task<ObjectResult> ListPremisesByBlurbIdAsync(ICallerInfo callerInfo, string blurbId, int limit = 0);
}
public partial class PremiseRepo
{
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

}
