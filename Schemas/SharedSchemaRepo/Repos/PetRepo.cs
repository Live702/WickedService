using static System.Runtime.InteropServices.JavaScript.JSType;
using System;

namespace SharedSchemaRepo;
// This partial class demonstrates how you can extend the generated code in 
// various ways.
// - Extended constructor to take other repos as parameters
// - Implement a new method to seed the database


// Extend the IPetRepo interface so new method can be seen by module(s).
public partial interface IPetRepo
{
    public Task<ActionResult> SeedAsync(ICallerInfo callerInfo, string store, int numPets);
}
// Extend the PetRepo class to implement the new constructor and method.
public partial class PetRepo : DYDBRepository<PetEnvelope, Pet>, IPetRepo
{
    // Implement a constructor that takes the tag and category repos as parameters,
    // Microsoft DIwill select the constructor with the most parameters it can satisfy,
    // so this constructor will be used in lieu of the constructed defined in the
    // generated code.
    public PetRepo(IAmazonDynamoDB client, ICategoryRepo categoryRepo, ITagRepo tagRepo, ILzNotificationRepo notificationsRepo) : base(client) { 
        this.tagRepo = tagRepo; 
        this.categoryRepo = categoryRepo;
        this.notificationsRepo = notificationsRepo;
        UseNotifications = true;
    }

    private ITagRepo tagRepo;
    private ICategoryRepo categoryRepo; 
    private ILzNotificationRepo notificationsRepo;  


    // Implement the new method to satisfy the interface
    public async Task<ActionResult> SeedAsync(ICallerInfo callerInfo, string store, int numPets )
    {
        callerInfo.Table = store;

        List<string> petNames = ["Luna", "Max", "Bella", "Charlie", "Lucy", "Cooper", "Daisy", "Milo", "Lily", "Oliver", "Molly", "Rocky", "Bailey", "Shadow", "Sophie", "Tucker", "Coco", "Bear", "Maggie", "Leo",
"Ruby", "Oscar", "Sadie", "Zeus", "Penny", "Duke", "Chloe", "Winston", "Rosie", "Jack", "Lola", "Buddy", "Gracie", "Thor", "Nala", "Scout", "Hazel", "Bruno", "Millie", "Sam",
"Nova", "Bentley", "Piper", "Rex", "Pearl", "Atlas", "Willow", "Finn", "Maya", "Moose", "Pepper", "Ziggy", "Roxy", "Felix", "Ginger", "Koda", "Belle", "Blue", "Stella", "Banjo",
"Maple", "Louie", "Winnie", "Jasper", "Poppy", "Diesel", "Olive", "River", "Sage", "Simba", "Luna", "Archie", "Juniper", "Ozzy", "Pixie", "Hero", "Ivy", "Tank", "Pip", "Odin",
"Birdie", "Harvey", "Clover", "Rocket", "Nutmeg", "Storm", "Fiona", "Zigzag", "Echo", "Bolt", "Coral", "Phoenix", "Iris", "Theo", "Luna", "Atlas", "Nova", "Cedar", "Sage", "Ash"];

        var categoriesResult = await categoryRepo.ListAsync(callerInfo);
        var categories = (List<Category>)categoriesResult!.Value!;

        var tagResult = await tagRepo.ListAsync(callerInfo);
        var tags = (List<string>)tagResult!.Value!;

        var pets = new List<Pet>();
        var maxPets = numPets;
        Random randomPetName = new Random();   
        Random randomCategory = new Random();   
        Random randomTag = new Random();
        Random numberOfTags = new Random(); 
        for (int i = 0; i < maxPets; i++)
        {
            var petTags = new List<string>();
            var numTags = numberOfTags.Next(0, tags.Count + 1);
            for (int t = 0; t < numTags; t++)
            {
                string tag = "";
                do
                {
                    tag = tags[randomTag.Next(0, tags.Count)];
                } while (petTags.Contains(tag));

                petTags.Add(tag);
            }

            var pet = new Pet
            {
                Id = Guid.NewGuid().ToString(),
                Name = petNames[randomPetName.Next(0,petNames.Count)],
                Category = categories[randomCategory.Next(0,categories.Count)].Id,
                Tags = petTags,
                PhotoUrls = new List<string> { "" },
                PetStatus = PetStatus.Available,
                CreateUtcTick = DateTime.UtcNow.Ticks,
                UpdateUtcTick = DateTime.UtcNow.Ticks
            };
            pets.Add(pet);
        }
        foreach(var pet in pets)
        {
            await CreateAsync(callerInfo, pet);
        }

        return new StatusCodeResult(200);
    }

    public override async Task WriteNotificationAsync(ICallerInfo callerInfo, string dataType, string data, string topics, long updatedUtcTick, string action)
    {
        // throw new NotImplementedException();
        var notification = new LzNotification
        {
            Id = Guid.NewGuid().ToString(),
            Topics = topics,
            UserId = callerInfo.Table,
            PayloadParentId = callerInfo.Table,
            PayloadId = "",
            PayloadType = dataType,
            Payload = data,
            PayloadAction = action,
            SessionId = callerInfo.SessionId,
            CreateUtcTick = updatedUtcTick,
            UpdateUtcTick = updatedUtcTick
        };
        callerInfo.Table = "notifications";
        await notificationsRepo.CreateAsync(callerInfo, notification);
        return;
    }

    public override async Task WriteDeleteNotificationAsync(ICallerInfo callerInfo, string dataType, string sk, string topics, long updatedUtcTick)
    {
        //throw new NotImplementedException();
        var notification = new LzNotification
        {
            Id = Guid.NewGuid().ToString(),
            Topics = topics,
            UserId = callerInfo.Table,
            PayloadParentId = callerInfo.Table,
            PayloadId = sk,
            PayloadType = dataType,
            PayloadAction = "Delete",
            SessionId = callerInfo.SessionId,
            CreateUtcTick = updatedUtcTick,
            UpdateUtcTick = updatedUtcTick
        };
        callerInfo.Table = "notifications";
        await notificationsRepo.CreateAsync(callerInfo, notification);
        return;
        
    }


}
