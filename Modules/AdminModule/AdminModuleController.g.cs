
namespace AdminModule;
public partial class AdminModuleController : AdminModuleControllerBase {
        public AdminModuleController(
            IAdminModuleAuthorization adminModuleAuthorization,
			IBlurbRepo blurbRepo,
			IMessageRepo messageRepo,
			IPremiseRepo premiseRepo,
			IChatRepo chatRepo,
			ICategoryRepo categoryRepo,
			ITagRepo tagRepo,
			IPetRepo petRepo,
			IOrderRepo orderRepo,
			ITenantUserRepo tenantUserRepo
            ) 
        {
            AdminModuleAuthorization = adminModuleAuthorization;
			BlurbRepo = blurbRepo;
			MessageRepo = messageRepo;
			PremiseRepo = premiseRepo;
			ChatRepo = chatRepo;
			CategoryRepo = categoryRepo;
			TagRepo = tagRepo;
			PetRepo = petRepo;
			OrderRepo = orderRepo;
			TenantUserRepo = tenantUserRepo;

            Init();
        }
}
