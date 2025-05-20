
namespace WickedModule;
public partial class WickedModuleController : WickedModuleControllerBase {
        public WickedModuleController(
            IWickedModuleAuthorization wickedModuleAuthorization,
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
            WickedModuleAuthorization = wickedModuleAuthorization;
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
