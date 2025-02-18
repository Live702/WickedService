namespace AdminSchemaRepo;
using LazyMagic.Service.AwsTenancyConfigService;

public class SubtenantEnvelope : DataEnvelope<Subtenant> { }    

public interface ISubtenantRepo : IDYDBRepository<SubtenantEnvelope, Subtenant>
{
    Task<IActionResult> SeedPetsAsync(ICallerInfo callerinfo, string store, int numPets);
}

public class SubtenantRepo : DYDBRepository<SubtenantEnvelope, Subtenant>, ISubtenantRepo
{
    public SubtenantRepo(IAmazonDynamoDB client, ITenancyConfigService tenancyConfigService) : base(client) 
    {
        this.tenancyConfigService = tenancyConfigService;
    }
    
    ITenancyConfigService tenancyConfigService;

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

    public override async Task<ObjectResult> ListAsync(ICallerInfo callerInfo)
    {  
        // We update the Subtenant records each time we list them. This is 
        // a mid-term solution as we start moving subtenant creation from 
        // the PowerShell cmd creation model to subtenants managed by 
        // this repo.
        Console.WriteLine("SubtenantRepo.ListAsync");
        if (callerInfo.Headers != null && callerInfo.Headers.TryGetValue("lz-aws-kvskey", out var kvskey))
        {
            // kvskey is either "subdomain.domain.tld" or "domain.tld". In either case,
            // we only want the domain.tld part as we are processing all subtenants for 
            // the tenant. Remember that each tenant has a domain.tld and each 
            // subtenat has a subdomain.domain.tld. Also remember that the subdomain 
            // and subtentantKey need not be the same so you can't use sunbtenantKey 
            // as a surogate for the subdomain.
            var hostParts = kvskey.Split('.');
            var tenantKvsKey = string.Join(".", hostParts[^2..]);
            Console.WriteLine($"Processing all subtenants for tenant {tenantKvsKey}");
            var kvsEntries = await tenancyConfigService.GetSubtenancyConfigsAsync(callerInfo, tenantKvsKey);

            foreach (var kvsEntryItem in kvsEntries)
            {
                //var key = kvsEntryItem.Key;
                //Console.WriteLine($"Processing subtenant {key}");
                //var value = kvsEntryItem.Value;
                //var subtenantDomain = key.Split('.').First();
                //var subtenantData = new Subtenant
                //{
                //    SubDomain = subtenantDomain,
                //    KvsEntry = value
                //};
                //try
                //{
                //    ExtractFromKvsEntry(value, subtenantData);
                //    Console.WriteLine($"Subtenant {key} has {subtenantData.Apis.Count} APIs, {subtenantData.Assets.Count} Assets, and {subtenantData.WebApps.Count} WebApps");
                //    await UpdateAsync(callerInfo, subtenantData);
                //}
                //catch (Exception e)
                //{
                //    Console.WriteLine($"Failed to process subtenant {key}: {e.Message}");
                //}
            }
        }
        return await base.ListAsync(callerInfo);
    }

}
