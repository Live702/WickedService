namespace SharedSchemaRepo;

public partial class CategoryRepo 
{
    // Since this is a relatively static list, we can just define it 
    // here. Later, if the list became dynamic, we could move it into 
    // the repo.
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
