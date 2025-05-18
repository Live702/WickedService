// This is our custom extension method to add the PublicSchemaRepo services
namespace PublicSchemaRepo;
public static partial class PublicSchemaRepoExtensions
{
    static partial void AddCustom(IServiceCollection services)
    {
        services.AddAWSService<IAmazonBedrockRuntime>();
    }
}
