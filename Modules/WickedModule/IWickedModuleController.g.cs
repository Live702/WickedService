
namespace WickedModule
 
{

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "14.0.3.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.3.0))")]
    public interface IWickedModuleController
    {

        /// <summary>
        /// Add a new blurb to the store
        /// </summary>


        /// <returns>successful operation</returns>

        Task<ActionResult<Blurb>> CreateBlurb(Blurb body);

        /// <summary>
        /// Update an existing blurb
        /// </summary>


        /// <returns>successful operation</returns>

        Task<ActionResult<Blurb>> UpdateBlurb(Blurb body);

        /// <summary>
        /// List all blurbs
        /// </summary>

        /// <returns>successful operation</returns>

        Task<ActionResult<System.Collections.Generic.ICollection<Blurb>>> ListBlurbs();

        /// <summary>
        /// List Blurbs by status
        /// </summary>

        /// <remarks>
        /// Status
        /// </remarks>

        /// <param name="blurbStatus">Status value</param>

        /// <returns>successful operation</returns>

        Task<ActionResult<System.Collections.Generic.ICollection<Blurb>>> ListBlurbsByStatus(string blurbStatus);

        /// <summary>
        /// Add a new message to the store
        /// </summary>


        /// <returns>successful operation</returns>

        Task<ActionResult<Message>> CreateMessage(Message body);

        /// <summary>
        /// Update an existing message
        /// </summary>


        /// <returns>successful operation</returns>

        Task<ActionResult<Message>> UpdateMessage(Message body);

        /// <summary>
        /// Add a new premise to the store
        /// </summary>


        /// <returns>successful operation</returns>

        Task<ActionResult<Premise>> CreatePremise(Premise body);

        /// <summary>
        /// Update an existing premise
        /// </summary>


        /// <returns>successful operation</returns>

        Task<ActionResult<Premise>> UpdatePremise(Premise body);

        /// <summary>
        /// Add a new chat to the store
        /// </summary>


        /// <returns>successful operation</returns>

        Task<ActionResult<Chat>> CreateChat(Chat body);

        /// <summary>
        /// Update an existing chat
        /// </summary>


        /// <returns>successful operation</returns>

        Task<ActionResult<Chat>> UpdateChat(Chat body);

        /// <summary>
        /// Read blurb by ID
        /// </summary>

        /// <remarks>
        /// Returns a single blurb
        /// </remarks>

        /// <param name="blurbId">ID of blurb to return</param>

        /// <returns>successful operation</returns>

        Task<ActionResult<Blurb>> ReadBlurbById(string blurbId);

        /// <summary>
        /// Deletes a blurb
        /// </summary>

        /// <param name="blurbId">Blurb id to delete</param>

        /// <returns>Success</returns>

        Task<IActionResult> DeleteBlurb(string blurbId);

        /// <summary>
        /// Read message by ID
        /// </summary>

        /// <remarks>
        /// Returns a single message
        /// </remarks>

        /// <param name="messageId">ID of message to return</param>

        /// <returns>successful operation</returns>

        Task<ActionResult<Message>> ReadMessageById(string messageId);

        /// <summary>
        /// Deletes a message
        /// </summary>

        /// <param name="messageId">Message id to delete</param>

        /// <returns>Success</returns>

        Task<IActionResult> DeleteMessage(string messageId);

        /// <summary>
        /// List Messages by ChatId
        /// </summary>

        /// <remarks>
        /// Lists messages by chatId
        /// </remarks>

        /// <param name="chatId">ChatId message belongs to</param>

        /// <returns>successful operation</returns>

        Task<ActionResult<System.Collections.Generic.ICollection<Message>>> ListMessagesByChatId(string chatId);

        /// <summary>
        /// Read premise by ID
        /// </summary>

        /// <remarks>
        /// Returns a single premise
        /// </remarks>

        /// <param name="premiseId">ID of premise to return</param>

        /// <returns>successful operation</returns>

        Task<ActionResult<Premise>> ReadPremiseById(string premiseId);

        /// <summary>
        /// Deletes a premise
        /// </summary>

        /// <param name="premiseId">Premise id to delete</param>

        /// <returns>Success</returns>

        Task<IActionResult> DeletePremise(string premiseId);

        /// <summary>
        /// List Premises by blurbId
        /// </summary>

        /// <remarks>
        /// List Premiess by blurbId
        /// </remarks>

        /// <param name="blurbId">BlurbId the premise belongs to</param>

        /// <returns>successful operation</returns>

        Task<ActionResult<System.Collections.Generic.ICollection<Premise>>> ListPremisesByBlurbId(string blurbId);

        /// <summary>
        /// Read chat by ID
        /// </summary>

        /// <remarks>
        /// Returns a single chat
        /// </remarks>

        /// <param name="chatId">ID of chat to return</param>

        /// <returns>successful operation</returns>

        Task<ActionResult<Chat>> ReadChatById(string chatId);

        /// <summary>
        /// Deletes a chat
        /// </summary>

        /// <param name="chatId">Chat id to delete</param>

        /// <returns>Success</returns>

        Task<IActionResult> DeleteChat(string chatId);

        /// <summary>
        /// List Chats by blurbId
        /// </summary>

        /// <remarks>
        /// blurbId value chat belongs to
        /// </remarks>

        /// <param name="blurbId">BlurbId chat belongs to</param>

        /// <returns>successful operation</returns>

        Task<ActionResult<System.Collections.Generic.ICollection<Chat>>> ListChatsByBlurbId(string blurbId);

        /// <summary>
        /// List Chats by premiseId
        /// </summary>

        /// <remarks>
        /// premiseId value chat belongs to
        /// </remarks>

        /// <param name="premiseId">PremiseId chat belongs to</param>

        /// <returns>successful operation</returns>

        Task<ActionResult<System.Collections.Generic.ICollection<Chat>>> ListChatsByPremiseId(string premiseId);

    }

}
