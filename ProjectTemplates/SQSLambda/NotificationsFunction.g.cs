// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace LambdaFunc;

public class NotificationsFunction
{
    private class WebSocketMessage
    {
        public string Action { get; set; } = "";
        public JsonElement Data { get; set; }
    }

    private class NotificationAction
    {
        public string Id { get; set; } = "";
        public string Topics { get; set; } = "";
        public string UserId { get; set; } = "";
        public string PayloadParentId { get; set; } = "";
        public string PayloadType { get; set; } = "";
        public string Payload { get; set; } = "";
        public string PayloadAction { get; set; } = "";
        public string SessionId { get; set; } = "";
        public long CreatedUtcTick { get; set; } = 0;
        public long UpdateUtcTick { get; set; } = 0;
    }

    private readonly ILogger<NotificationsFunction> _logger;
    private readonly ILzSubscriptionRepo _subscriptionSchemaRepo;
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private IAmazonApiGatewayManagementApi? _gatewayManagementApi;
    private string? _connectionMgmtUrl = "";

    public NotificationsFunction()
    {
        // Set up dependency injection - this allows us to use AddAWSService and other DI features
        var serviceCollection = new ServiceCollection();

        // Configure logging
        serviceCollection.AddLogging(builder => builder.AddConsole());

        // Register application services
        serviceCollection.AddSingleton<ILzSubscriptionRepo, LzSubscriptionRepo>();
        serviceCollection.AddAWSService<IAmazonDynamoDB>();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Resolve services 
        _logger = serviceProvider.GetRequiredService<ILogger<NotificationsFunction>>();
        _subscriptionSchemaRepo = serviceProvider.GetRequiredService<ILzSubscriptionRepo>();
        _dynamoDbClient = serviceProvider.GetRequiredService<IAmazonDynamoDB>();

        // Get environment variables
        _connectionMgmtUrl = Environment.GetEnvironmentVariable("WEBSOCKET_MGMT_URL");
        if(string.IsNullOrEmpty(_connectionMgmtUrl))
        {
            throw new InvalidOperationException("WEBSOCKETURL environment variable not set");
        }

        // Early Initialization
        _gatewayManagementApi ??= new AmazonApiGatewayManagementApiClient(
            new AmazonApiGatewayManagementApiConfig
            {
                ServiceURL = _connectionMgmtUrl
            });

        _logger.LogInformation($"WebSocket Mgmt. URL: {_connectionMgmtUrl}");
    }
    public Task FunctionHandlerAsync(DynamoDBEvent dynamoEvent, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"Beginning to process {dynamoEvent.Records.Count} records...");

            foreach (var record in dynamoEvent.Records)
            {
                if (record.EventName == "INSERT")
                {
                    var newImage = record.Dynamodb.NewImage;    
                    if (newImage != null)
                    {
                        context.Logger.LogInformation($"New Image: {JsonConvert.SerializeObject(newImage)}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error processing records: {ex.Message}");
            context.Logger.LogError($"Stack trace: {ex.StackTrace}");
            throw;
        }
        return Task.CompletedTask;
    }
    // Method for broadcasting messages to a specific connection
    public async Task BroadcastMessageAsync(string connectionId, string message)
    {
        if (_gatewayManagementApi == null)
        {
            throw new InvalidOperationException("Gateway management API not initialized");
        }

        try
        {
            _logger.LogInformation($"Broadcasting message to {connectionId}: {message}");
            await _gatewayManagementApi.PostToConnectionAsync(new PostToConnectionRequest
            {
                ConnectionId = connectionId,
                Data = new MemoryStream(Encoding.UTF8.GetBytes(message))
            });
        }
        catch (GoneException)
        {
            // Connection is no longer valid
            _logger.LogWarning("Connection {ConnectionId} is no longer valid", connectionId);
            // You might want to remove this connection from your connection tracking
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast message to connection {ConnectionId}", connectionId);
            // Handle logging or retry logic here if needed
        }
        return;
    }
}