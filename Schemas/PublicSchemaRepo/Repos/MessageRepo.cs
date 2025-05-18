using Amazon.BedrockRuntime.Model;
using System.Text.Json;
using System.Text;

namespace PublicSchemaRepo;
public partial interface IMessageRepo : IDocumentRepo<Message> {}
public partial class MessageRepo : DYDBRepository<Message>, IMessageRepo
{
    [ActivatorUtilitiesConstructor]
    public MessageRepo(
        IAmazonDynamoDB client,
        IAmazonBedrockRuntime amazonBedrockRuntime
        ) : base(client) {
        _amazonBedrockRuntime = amazonBedrockRuntime;
        _bedrockRuntimeClient = new AmazonBedrockRuntimeClient();
    }

    private readonly IAmazonBedrockRuntime _amazonBedrockRuntime;
    private readonly AmazonBedrockRuntimeClient _bedrockRuntimeClient;

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


        } catch (Exception ex)
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
}
