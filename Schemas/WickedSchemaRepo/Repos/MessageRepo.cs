using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;

namespace WickedSchemaRepo;

public partial interface IMessageRepo : IDocumentRepo<Message> { 
    public Task<ObjectResult> ListMessagesByChatIdAsync(ICallerInfo callerInfo, string chatId, int limit = 0);
    public Task<ObjectResult> ListMessagesByBlurbIdAsync(ICallerInfo callerInfo, string blurbId, int limit = 0);
    public Task<ObjectResult> ListMessagesByPremiseIdAsync(ICallerInfo callerInfo, string premiseId, int limit = 0);
}
public partial class MessageRepo
{
    [ActivatorUtilitiesConstructor]
    public MessageRepo(
        IAmazonDynamoDB client,
        IAmazonBedrockRuntime amazonBedrockRuntime
        ) : base(client)
    {
        _amazonBedrockRuntime = amazonBedrockRuntime;
        _bedrockRuntimeClient = new AmazonBedrockRuntimeClient();
    }

    private readonly IAmazonBedrockRuntime _amazonBedrockRuntime;
    private readonly AmazonBedrockRuntimeClient _bedrockRuntimeClient;


    protected override void AssignEntityAttributes(ICallerInfo callerInfo, JObject jobjectData, Dictionary<string, AttributeValue> dbrecord, long now)
    {
        base.AssignEntityAttributes(callerInfo, jobjectData, dbrecord, now);
        // Assign SK1 (ChatId) index
        var chatId = jobjectData["chatId"]?.ToString();
        if (chatId != null)
            dbrecord.Add("SK1", new AttributeValue { S = chatId });
        
        // Assign SK2 (BlurbId) index
        var blurbId = jobjectData["blurbId"]?.ToString();
        if (blurbId != null)
            dbrecord.Add("SK2", new AttributeValue { S = blurbId });
        
        // Assign SK3 (PremiseId) index
        var premiseId = jobjectData["premiseId"]?.ToString();
        if (premiseId != null)
            dbrecord.Add("SK3", new AttributeValue { S = premiseId });
    }

    public override async Task<ActionResult<Message>> CreateAsync(ICallerInfo callerInfo, Message data)
    {
        // Call Bedrock to process the message

        try
        {

            string prompt = data.Body;
            string modelId = "us.anthropic.claude-3-7-sonnet-20250219-v1:0";

            var invokeRequest = CreateInvokeRequest(modelId, prompt);

            // Invoke the model
            var response = await _bedrockRuntimeClient.InvokeModelAsync(invokeRequest);

            // Process the response based on the model
            var processedResponse = await ProcessResponse(modelId, response);

            data.Body = data.Body + processedResponse;


        }
        catch (Exception ex)
        {
            // Handle the exception
            return new BadRequestResult();
        }

        return await base.CreateAsync(callerInfo, data);
    }

    private InvokeModelRequest CreateInvokeRequest(string modelId, string prompt)
    {
        // Different models require different request formats
        if (modelId.StartsWith("us.anthropic.claude"))
        {
            // Claude models (v3)
            var requestBody = new
            {
                anthropic_version = "bedrock-2023-05-31",
                max_tokens = 200,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new[]
                        {
                            new
                            {
                                type = "text",
                                text = prompt
                            }
                        }
                    }
                }
            };

            var serializedBody = JsonSerializer.Serialize(requestBody);

            return new InvokeModelRequest
            {
                ModelId = modelId,
                ContentType = "application/json",
                Accept = "application/json",
                Body = new MemoryStream(Encoding.UTF8.GetBytes(serializedBody))
            };
        }
        else if (modelId.StartsWith("amazon.titan"))
        {
            // Amazon Titan models
            var requestBody = new
            {
                inputText = prompt,
                textGenerationConfig = new
                {
                    maxTokenCount = 512,
                    temperature = 0.5
                }
            };

            var serializedBody = JsonSerializer.Serialize(requestBody);

            return new InvokeModelRequest
            {
                ModelId = modelId,
                ContentType = "application/json",
                Accept = "application/json",
                Body = new MemoryStream(Encoding.UTF8.GetBytes(serializedBody))
            };
        }
        else
        {
            throw new NotSupportedException($"Model {modelId} is not supported.");
        }
    }

    private async Task<string> ProcessResponse(string modelId, InvokeModelResponse response)
    {
        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception($"Error invoking model: {response.HttpStatusCode}");
        }

        using var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();

        // For Claude models
        if (modelId.StartsWith("anthropic.claude"))
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseBody, options);

            return JsonSerializer.Serialize(new
            {
                response = claudeResponse?.Content?.FirstOrDefault()?.Text,
                model = modelId
            });
        }
        // For Titan models
        else if (modelId.StartsWith("amazon.titan"))
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var titanResponse = JsonSerializer.Deserialize<TitanResponse>(responseBody, options);

            return JsonSerializer.Serialize(new
            {
                response = titanResponse?.Results?.FirstOrDefault()?.OutputText,
                model = modelId
            });
        }

        return responseBody;
    }

    // Classes for deserializing responses
    public class ClaudeResponse
    {
        public string? Id { get; set; }
        public List<ClaudeContent>? Content { get; set; }
    }

    public class ClaudeContent
    {
        public string? Type { get; set; }
        public string? Text { get; set; }
    }

    public class TitanResponse
    {
        public List<TitanResult>? Results { get; set; }
    }

    public class TitanResult
    {
        public string? OutputText { get; set; }
    }


    public async Task<ObjectResult> ListMessagesByChatIdAsync(ICallerInfo callerInfo, string chatId, int limit = 0)
    {
        return await ListAsync(callerInfo, "SK1", chatId, limit);
    }

    public async Task<ObjectResult> ListMessagesByBlurbIdAsync(ICallerInfo callerInfo, string blurbId, int limit = 0)
    {
        return await ListAsync(callerInfo, "SK2", blurbId, limit);
    }

    public async Task<ObjectResult> ListMessagesByPremiseIdAsync(ICallerInfo callerInfo, string premiseId, int limit = 0)
    {
        return await ListAsync(callerInfo, "SK3", premiseId, limit);
    }

}
