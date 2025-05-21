
namespace ConsumerModule;
public partial class ConsumerModuleController : ConsumerModuleControllerBase {
        public ConsumerModuleController(
            IConsumerModuleAuthorization consumerModuleAuthorization,
			IPreferencesRepo preferencesRepo,
			ICategoryRepo categoryRepo,
			ITagRepo tagRepo,
			IPetRepo petRepo,
			IOrderRepo orderRepo
            ) 
        {
            ConsumerModuleAuthorization = consumerModuleAuthorization;
			PreferencesRepo = preferencesRepo;
			CategoryRepo = categoryRepo;
			TagRepo = tagRepo;
			PetRepo = petRepo;
			OrderRepo = orderRepo;

            Init();
        }
}
