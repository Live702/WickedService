
namespace PublicModule
 
{

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "14.0.3.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.3.0))")]
    public interface IPublicModuleController
    {

        /// <summary>
        /// List all pets
        /// </summary>

        /// <returns>successful operation</returns>

        Task<ActionResult<System.Collections.Generic.ICollection<Pet>>> ListPets();

        /// <summary>
        /// Finds Pets by status
        /// </summary>

        /// <remarks>
        /// Multiple status values can be provided with comma separated strings
        /// </remarks>

        /// <param name="petStatus">Status values that need to be considered for filter</param>

        /// <returns>successful operation</returns>

        Task<ActionResult<System.Collections.Generic.ICollection<Pet>>> FindPetsByStatus(System.Collections.Generic.IEnumerable<PetStatus> petStatus);

        /// <summary>
        /// Finds Pets by tags
        /// </summary>

        /// <remarks>
        /// Muliple tags can be provided with comma separated strings. Use\ \ tag1, tag2, tag3 for testing.
        /// </remarks>

        /// <param name="tags">Tags to filter by</param>

        /// <returns>successful operation</returns>

        Task<ActionResult<System.Collections.Generic.ICollection<Pet>>> FindPetsByTags(System.Collections.Generic.IEnumerable<string> tags);

        /// <summary>
        /// Get all Pet Categories
        /// </summary>

        /// <returns>successful operation</returns>

        Task<ActionResult<System.Collections.Generic.ICollection<Category>>> GetPetCategories();

        /// <summary>
        /// Get all Pet Tags
        /// </summary>

        /// <returns>successful operation</returns>

        Task<ActionResult<System.Collections.Generic.ICollection<Tag>>> GetPetTags();

        /// <summary>
        /// Update an existing message
        /// </summary>


        /// <returns>successful operation</returns>

        Task<ActionResult<Message>> UpdateMessage(Message body);

        /// <summary>
        /// Create Message
        /// </summary>


        /// <returns>successful operation</returns>

        Task<ActionResult<Message>> CreateMessage(Message body);

        /// <summary>
        /// List all Messages
        /// </summary>

        /// <returns>successful operation</returns>

        Task<ActionResult<System.Collections.Generic.ICollection<Message>>> ListMessages();

        /// <summary>
        /// Update an existing premise
        /// </summary>


        /// <returns>successful operation</returns>

        Task<ActionResult<Premise>> UpdatePremise(Premise body);

        /// <summary>
        /// Create Premise
        /// </summary>


        /// <returns>successful operation</returns>

        Task<ActionResult<Premise>> CreatePremise(Premise body);

        /// <summary>
        /// List all Premises
        /// </summary>

        /// <returns>successful operation</returns>

        Task<ActionResult<System.Collections.Generic.ICollection<Premise>>> ListPremises();

        /// <summary>
        /// Find pet by ID
        /// </summary>

        /// <remarks>
        /// Returns a single pet
        /// </remarks>

        /// <param name="petId">ID of pet to return</param>

        /// <returns>successful operation</returns>

        Task<ActionResult<Pet>> GetPetById(string petId);

        /// <summary>
        /// Read message b y id
        /// </summary>

        /// <param name="id">ID of pet to return</param>

        /// <returns>successful operation</returns>

        Task<ActionResult<Message>> GetMessageById(string id);

        /// <summary>
        /// Read premise b y id
        /// </summary>

        /// <param name="id">ID of pet to return</param>

        /// <returns>successful operation</returns>

        Task<ActionResult<Premise>> GetPremiseById(string id);

    }

}
