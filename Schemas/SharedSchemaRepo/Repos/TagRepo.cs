using System.Collections.Generic;

namespace SharedSchemaRepo;
public partial class TagRepo
{
    // For this demo, this is a static list.
    // // Later, if we want a dynamic store, we can
    // move it into the database.
    private readonly List<Tag> list = new() {
        new Tag { Id = "HouseTrained", Name = "HouseTrained" },
        new Tag { Id = "HasShots", Name = "HasShots" },
        new Tag { Id = "Crazy", Name = "Crazy" }
};

    public override async Task<ObjectResult> ListAsync(ICallerInfo callerInfo, int limit = 0)
    {
        await Task.Delay(0);
        ObjectResult objResult = new ObjectResult(list)
        {
            StatusCode = 200
        };
        return (objResult);
    }

}