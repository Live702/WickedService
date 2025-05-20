
namespace StoreModule;
public partial class StoreModuleController : StoreModuleControllerBase {
        public StoreModuleController(
            IStoreModuleAuthorization storeModuleAuthorization,
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
            StoreModuleAuthorization = storeModuleAuthorization;
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
