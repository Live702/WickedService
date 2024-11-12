using System.Collections.Generic;

namespace SharedSchemaRepo;
public partial class TagRepo
{
    // For this demo we just define a static list of tags, for 
    // a real app, we would probalby want to store these in
    // the repo.
    private readonly List<string> list = ["HouseTrained", "HasShots", "Crazy"];

    // We only override the ListAsync method, since the other base methods are 
    // not called. 
    public override async Task<ObjectResult> ListAsync(ICallerInfo callerInfo)
    {
        await Task.Delay(0);
        ObjectResult objResult = new ObjectResult(list)
        {
            StatusCode = 200
        };
        return (objResult);
    }

}