
namespace ConsumerModule;
public partial class ConsumerModuleController : ConsumerModuleControllerBase {
        public ConsumerModuleController(
            IConsumerModuleAuthorization consumerModuleAuthorization,
			IBlurbRepo blurbRepo,
			IMessageRepo messageRepo,
			IPremiseRepo premiseRepo,
			IChatRepo chatRepo,
			IPreferencesRepo preferencesRepo,
			ICategoryRepo categoryRepo,
			ITagRepo tagRepo,
			IPetRepo petRepo,
			IOrderRepo orderRepo
            ) 
        {
            ConsumerModuleAuthorization = consumerModuleAuthorization;
			BlurbRepo = blurbRepo;
			MessageRepo = messageRepo;
			PremiseRepo = premiseRepo;
			ChatRepo = chatRepo;
			PreferencesRepo = preferencesRepo;
			CategoryRepo = categoryRepo;
			TagRepo = tagRepo;
			PetRepo = petRepo;
			OrderRepo = orderRepo;

            Init();
        }
}
