using Microsoft.AspNetCore.Mvc;

namespace AdminModule
{
    public partial class AdminModuleControllerImpl
    {
        // We implement our own constructcor to inject the ISubtenantRepo
        // as SubtenantRepo is not generated. You must update this constructor
        // if a new generated repo is added/delted on the module.
        [ActivatorUtilitiesConstructor] // force DI to use this constructor
        public AdminModuleControllerImpl(
            IAdminModuleAuthorization adminModuleAuthorization,
            ICategoryRepo categoryRepo,
            ITagRepo tagRepo,
            IPetRepo petRepo,
            IOrderRepo orderRepo,
            ITenantUserRepo tenantUserRepo,
            ISubtenantRepo subtenantRepo
            )
        {
            this.adminModuleAuthorization = adminModuleAuthorization;
            this.categoryRepo = categoryRepo;
            this.tagRepo = tagRepo;
            this.petRepo = petRepo;
            this.orderRepo = orderRepo;
            this.tenantUserRepo = tenantUserRepo;
            this.subtenantRepo = subtenantRepo;

            Init();
        }

        // Implement methods for which the generator does not generate 
        // an implementation. 

        public override async Task<ActionResult<TenantUserStatus>> IsAdmin()
        {
            try
            {
                // callerInfo will throw if permission is denied
                Console.WriteLine("Checking if user is admin");
                var callerInfo = await adminModuleAuthorization.GetCallerInfoAsync(this.Request);
                return new TenantUserStatus() { IsAdmin = true};
            } catch 
            {
                return new TenantUserStatus() { IsAdmin = false};
            }
        }

        public override async Task<IActionResult> SeedPets(string store, int numPets)
        {
            Console.WriteLine($"Seeding {numPets} pets in store {store}");
            var callerInfo = await adminModuleAuthorization.GetCallerInfoAsync(this.Request);
            return Ok();
        }
    }
}
