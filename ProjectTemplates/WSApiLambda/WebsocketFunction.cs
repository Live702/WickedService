[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace LambdaFunc;

public class WebsocketFunction
{

    private class WebSocketMessage
    {
        public string Action { get; set; } = "";
        public JsonElement Data { get; set; }
    }

    private class AuthenticateAction
    {
        public string Token { get; set; } = "";
        public string SessionId { get; set; } = "";
    }

    private class AuthenticationSuccessAction
    {
        public string ConnectionId { get; set; } = "";
    }
    private readonly ILogger<WebsocketFunction> _logger;
    private readonly ILzSubscriptionRepo _subscriptionSchemaRepo;
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private IAmazonApiGatewayManagementApi? _gatewayManagementApi;
    private readonly IAmazonCognitoIdentityProvider _cognitoClient;
    private readonly bool _authenticationRequired;

    public WebsocketFunction()
    {
        // Set up dependency injection - this allows us to use AddAWSService and other DI features
        var serviceCollection = new ServiceCollection();

        // Configure logging
        serviceCollection.AddLogging(builder => builder.AddConsole());

        // Register AWS services
        serviceCollection.AddAWSService<IAmazonCognitoIdentityProvider>();

        // Register application services
        serviceCollection.AddSingleton<ILzSubscriptionRepo, LzSubscriptionRepo>();
        serviceCollection.AddAWSService<IAmazonDynamoDB>();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Resolve services 
        _logger = serviceProvider.GetRequiredService<ILogger<WebsocketFunction>>();
        _subscriptionSchemaRepo = serviceProvider.GetRequiredService<ILzSubscriptionRepo>();
        _dynamoDbClient = serviceProvider.GetRequiredService<IAmazonDynamoDB>();
        _cognitoClient = serviceProvider.GetRequiredService<IAmazonCognitoIdentityProvider>();

        // Get environment variables
        _authenticationRequired = Environment.GetEnvironmentVariable("AUTHENTICATION_REQUIRED") == "true";


        _logger.LogInformation("AuthRequired: {AuthRequired}", _authenticationRequired);
    }

    public async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            InitializeApiGateway(request);
            _logger.LogInformation("EventType: {EventType}", request.RequestContext.EventType);
            switch (request.RequestContext.EventType)
            {
                case "CONNECT":
                    return await HandleConnectAsync(request, context);
                case "MESSAGE":
                    return await HandleMessageAsync(request, context);
                case "DISCONNECT":
                    return await HandleDisconnectAsync(request, context);
                default:
                    {
                        _logger.LogError($"Invalid event type {request.RequestContext.EventType}");
                        return new APIGatewayProxyResponse { StatusCode = (int)HttpStatusCode.BadRequest };
                    }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing {EventType} for connection {ConnectionId}",
                request.RequestContext.EventType,
                request.RequestContext.ConnectionId);
            return new APIGatewayProxyResponse { StatusCode = (int)HttpStatusCode.InternalServerError };
        }
    }

    private void InitializeApiGateway(APIGatewayProxyRequest input)
    {
        if (_gatewayManagementApi == null)
        {
            var config = new AmazonApiGatewayManagementApiConfig
            {
                ServiceURL = $"https://{input.RequestContext.DomainName}/{input.RequestContext.Stage}"
            };
            _gatewayManagementApi = new AmazonApiGatewayManagementApiClient(config);
        }
    }

    private async Task<APIGatewayProxyResponse> HandleMessageAsync(APIGatewayProxyRequest input, ILambdaContext context)
    {
        // Deserialize the incoming message
        var message = JsonSerializer.Deserialize<WebSocketMessage>(input.Body);

        if (message == null)
        {
            _logger.LogError("Invalid message format");
            return new APIGatewayProxyResponse { StatusCode = 400, Body = "Invalid message format" };
        }

        if (message.Action != "AUTHENTICATE")
        {
            _logger.LogError($"Invalid Message Action: {message.Action}");
            return new APIGatewayProxyResponse { StatusCode = 400, Body = "Invalid message format" };
        }

        var token = message.Data.GetProperty("Token").GetString();
        var sessionId = message.Data.GetProperty("SessionId").GetString();

        // Connect with auth
        // Note: ApiGateway doesn't have the ability to use authentication headers for websocket 
        // routes. So we call cognito ourselves to get that done. Note that this is fine as 
        // we only call cognito once in the AUTHENTICATE message and we only use the
        // websocket to send messages to clients. We don't use the websocket to receive messages.
        try
        {
            _logger.LogInformation($"Authenticating with Token: {token}");

            try
            {
                var connectionId = input.RequestContext.ConnectionId;
                var userResponse = await _cognitoClient.GetUserAsync(new GetUserRequest { AccessToken = token });
                var userId = userResponse.UserAttributes.First(a => a.Name == "sub").Value;
                var userEmail = userResponse.UserAttributes.First(a => a.Name == "email").Value;
                _logger.LogInformation("New connection established: {ConnectionId} {userEmail}", connectionId, userEmail);

                var authenticateSuccessAction = new AuthenticationSuccessAction
                {
                    ConnectionId = connectionId
                };
                var successMessage = new WebSocketMessage
                {
                    Action = "AUTHENTICATESUCCESS",
                    Data = JsonSerializer.SerializeToElement(authenticateSuccessAction)
                };

                // Send client AUTHENTICATION message
                await BroadcastMessageAsync(connectionId, JsonSerializer.Serialize(successMessage));

                return new APIGatewayProxyResponse { StatusCode = 200, Body = connectionId };
            }
            catch (NotAuthorizedException)
            {
                _logger.LogInformation("Connection error. Invalid token");
                return new APIGatewayProxyResponse { StatusCode = 401, Body = "Invalid token" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Connect Error: {ex.Message}");
            return new APIGatewayProxyResponse { StatusCode = 500, Body = "Connection failed" };
        }
    }

    private async Task<APIGatewayProxyResponse> HandleConnectAsync(APIGatewayProxyRequest input, ILambdaContext context)
    {
        await Task.Delay(0);
        var connectionId = input.RequestContext.ConnectionId;
        _logger.LogInformation("New connection established: {ConnectionId}", connectionId);
        return new APIGatewayProxyResponse { StatusCode = 200, Body = connectionId };
    }

    private async Task<APIGatewayProxyResponse> HandleDisconnectAsync(APIGatewayProxyRequest input, ILambdaContext context)
    {
        await Task.Delay(0);
        var connectionId = input.RequestContext.ConnectionId;
        _logger.LogInformation("Connection closed: {ConnectionId}", connectionId);
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
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
    }
}
