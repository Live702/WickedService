namespace AdminSchemaRepo;
using Amazon.CloudFront;

using Amazon.CloudFrontKeyValueStore;
using Amazon.CloudFrontKeyValueStore.Model;
using Amazon.CloudFront.Model;


public interface ISubtenantRepo : IDocumentRepo<Subtenant>
{
    Task<IActionResult> SeedPetsAsync(ICallerInfo callerinfo, string store, int numPets);
}

public class SubtenantRepo : DYDBRepository<Subtenant>, ISubtenantRepo
{
    public SubtenantRepo(
        IAmazonDynamoDB client,
        IAmazonCloudFrontKeyValueStore cloudFrontKvs,
        IAmazonCloudFront cloudFront
        ) : base(client) 
    {
        _cloudFront = cloudFront;
        _cloudFrontKeyValueStore = cloudFrontKvs;
    }

    private readonly IAmazonCloudFrontKeyValueStore _cloudFrontKeyValueStore;
    private readonly IAmazonCloudFront _cloudFront;

    protected override void ConstructorExtensions()
    {
        // Users are stored in the TenantDB
        tableLevel = TableLevel.Tenant; // Use the TenantDB passed in callerInfo
        debug = false; // Log all calls to the console
        base.ConstructorExtensions();
    }

    public async Task<IActionResult> SeedPetsAsync(ICallerInfo callerinfo, string store, int numPets)
    {
        await Task.Delay(0);
        return new ObjectResult($"Pets seeded.")
        {
            StatusCode = 200
        };
    }

    public override async Task<ObjectResult> ListAsync(ICallerInfo callerInfo, int limit = 0)
    {  
        // We update the Subtenant records from the kvs entries each time we list
        // them. This is a mid-term solution as we start moving subtenant
        // creation from the PowerShell cmd creation model to subtenants managed
        // by this repo.
        Console.WriteLine("SubtenantRepo.ListAsync");


        if (callerInfo.Headers != null 
            && callerInfo.Headers.TryGetValue("lz-tenantid", out var kvskey)
            && callerInfo.Headers.TryGetValue("lz-aws-kvsarn", out var kvsarn)
            )
        {
            // kvskey is either "subdomain.domain.tld" or "domain.tld". In either case,
            // we only want the domain.tld part as we are processing all subtenants for 
            // the tenant. Remember that each tenant has a domain.tld and each 
            // subtenat has a subdomain.domain.tld. Also remember that the subdomain 
            // and subtentantKey need not be the same so you can't use sunbtenantKey 
            // as a surogate for the subdomain.
            var hostParts = kvskey.Split('.');
            var tenantKvsKey = string.Join(".", hostParts[^2..]);

            var request = new ListKeysRequest
            {
                KvsARN = kvsarn,
                MaxResults = 50,
                NextToken = null
            };

            do
            {
                var response = await _cloudFrontKeyValueStore!.ListKeysAsync(request);
                request.NextToken = response.NextToken;
                foreach (var entry in response.Items)
                {
                    var entryKey = entry.Key;
                    if (!entryKey.EndsWith(tenantKvsKey)) continue;
                    var value = entry.Value;

                    // Update the Subtenant record
                    var subtenant = new Subtenant(value, entryKey);
                    try
                    {
                        try
                        {
                            await CreateAsync(callerInfo, subtenant);
                        } catch
                        {
                            await UpdateAsync(callerInfo, subtenant);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error updating subtenant {subtenant.TenantKey}: {e.Message}");
                    }
                }
            } while (request.NextToken != null);
        }
        return await base.ListAsync(callerInfo);
    }
}
