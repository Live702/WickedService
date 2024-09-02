using LazyMagic.Shared;
using Microsoft.AspNetCore.Http;

namespace SharedSchemaRepo;
public partial interface IPetRepo
{
    public Task<ActionResult> SeedAsync(ICallerInfo callerInfo);
}
public partial class PetRepo
{
    public async Task<ActionResult> SeedAsync(ICallerInfo callerInfo )
    {
        var pets = new List<Pet>()
        {
            { new Pet {
                Id = "1",
                Name = "Buddy",
                Category="Dog",
                PhotoUrls= new List<string> { "" },
                Tags = new List<string> { "HouseTrained", "HasShots"},
                PetStatus = PetStatus.Available,
                CreateUtcTick = DateTime.UtcNow.Ticks,
                UpdateUtcTick= DateTime.UtcNow.Ticks
                }
            },
            { new Pet {
                Id = "2",
                Name = "Alf",
                Category="Dog",
                PhotoUrls= new List<string> { "" },
                Tags = new List<string> { "HouseTrained", "HasShots"},
                PetStatus = PetStatus.Available,
                CreateUtcTick = DateTime.UtcNow.Ticks,
                UpdateUtcTick= DateTime.UtcNow.Ticks
                }
            },
            { new Pet {
                Id = "3",
                Name = "Sweeti",
                Category="Cat",
                PhotoUrls= new List<string> { "" },
                Tags = new List<string> { "HouseTrained", "Crazy", "HasShots"},
                PetStatus = PetStatus.Available,
                CreateUtcTick = DateTime.UtcNow.Ticks,
                UpdateUtcTick= DateTime.UtcNow.Ticks
                }
            }
        };
        foreach(var pet in pets)
        {
            await CreateAsync(callerInfo, pet);
        }

        return new StatusCodeResult(200);
    }

}
