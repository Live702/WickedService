
namespace StoreModule;
public partial class StoreModuleController : StoreModuleControllerBase {
        public StoreModuleController(
            IStoreModuleAuthorization storeModuleAuthorization,
			ICategoryRepo categoryRepo,
			ITagRepo tagRepo,
			IPetRepo petRepo,
			IOrderRepo orderRepo
            ) 
        {
            StoreModuleAuthorization = storeModuleAuthorization;
			CategoryRepo = categoryRepo;
			TagRepo = tagRepo;
			PetRepo = petRepo;
			OrderRepo = orderRepo;

            Init();
        }
}
