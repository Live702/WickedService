
namespace AdminModule;
public partial class AdminModuleController : AdminModuleControllerBase {
        public AdminModuleController(
            IAdminModuleAuthorization adminModuleAuthorization,
			ICategoryRepo categoryRepo,
			ITagRepo tagRepo,
			IPetRepo petRepo,
			IOrderRepo orderRepo,
			ITenantUserRepo tenantUserRepo
            ) 
        {
            AdminModuleAuthorization = adminModuleAuthorization;
			CategoryRepo = categoryRepo;
			TagRepo = tagRepo;
			PetRepo = petRepo;
			OrderRepo = orderRepo;
			TenantUserRepo = tenantUserRepo;

            Init();
        }
}
