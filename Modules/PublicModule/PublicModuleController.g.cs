
namespace PublicModule;
public partial class PublicModuleController : PublicModuleControllerBase {
        public PublicModuleController(
            IPublicModuleAuthorization publicModuleAuthorization,
			ICategoryRepo categoryRepo,
			ITagRepo tagRepo,
			IPetRepo petRepo,
			IOrderRepo orderRepo
            ) 
        {
            PublicModuleAuthorization = publicModuleAuthorization;
			CategoryRepo = categoryRepo;
			TagRepo = tagRepo;
			PetRepo = petRepo;
			OrderRepo = orderRepo;

            Init();
        }
}
