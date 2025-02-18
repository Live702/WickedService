using LazyMagic.Service.AwsTenancyConfigService;    
namespace AdminSchemaRepo;

public static partial class AdminSchemaRepoExtensions
{
    static partial void AddCustom(IServiceCollection services)
    {
        services.TryAddSingleton<ITenancyConfigService, AwsTenancyConfigService>();   
    }
}
