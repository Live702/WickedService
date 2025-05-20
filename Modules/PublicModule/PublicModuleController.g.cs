
namespace PublicModule;
public partial class PublicModuleController : PublicModuleControllerBase {
        public PublicModuleController(
            IPublicModuleAuthorization publicModuleAuthorization,
			IBlurbRepo blurbRepo,
			IMessageRepo messageRepo,
			IPremiseRepo premiseRepo,
			IChatRepo chatRepo,
			ICategoryRepo categoryRepo,
			ITagRepo tagRepo,
			IPetRepo petRepo,
			IOrderRepo orderRepo
            ) 
        {
            PublicModuleAuthorization = publicModuleAuthorization;
			BlurbRepo = blurbRepo;
			MessageRepo = messageRepo;
			PremiseRepo = premiseRepo;
			ChatRepo = chatRepo;
			CategoryRepo = categoryRepo;
			TagRepo = tagRepo;
			PetRepo = petRepo;
			OrderRepo = orderRepo;

            Init();
        }
}
