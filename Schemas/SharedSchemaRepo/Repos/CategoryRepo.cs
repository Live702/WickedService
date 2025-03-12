namespace SharedSchemaRepo;

public partial class CategoryRepo 
{
    // For the demo, this is a static list.
    // Later, if we want a dynamic store, we can move
    // it into the database.
    private readonly List<Category> list = new List<Category>
    {
        new Category { Id = "Dogs", Name = "Dogs" },
        new Category { Id = "Cats", Name = "Cats" },
        new Category { Id = "Birds", Name = "Birds" },
        new Category { Id = "Fish", Name = "Fish" },
        new Category { Id = "Reptiles", Name = "Reptiles" },
        new Category { Id = "Rodents", Name = "Rodents" },
        new Category { Id = "Other", Name = "Other" }
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
