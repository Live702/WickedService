
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Amazon.CloudFrontKeyValueStore.Model;
using Amazon.CloudFrontKeyValueStore;
using YamlDotNet.RepresentationModel;

public partial class DevConfigService
{
    private readonly HttpClient _httpClient;
    private readonly KvsService _kvsService;
    private readonly ILogger<DevConfigService> _logger;
    private JObject _currentConfig;

    public DevConfigService(HttpClient httpClient, ILogger<DevConfigService> logger, KvsService kvsService)
    {
        _httpClient = httpClient;
        _kvsService = kvsService;
        _logger = logger;
    }

    /// <summary>
    ///  This method reads the systemconfig.yaml file to get the list of tenants and the root domain for each tenant. 
    ///  It then reads the KVS entries for each tenant. These KVS entries are used in the http pipeline to 
    ///  add headers required by the module code.
    /// </summary>
    /// <returns></returns>
    public virtual async Task FetchAndApplyDevConfigAsync()
    {
        try
        {
            using (var reader = new StreamReader("../../systemconfig.yaml"))
            {
                // Read the serviceconfig.yaml file to get the list of tenants and the root domain for each tenant
                var yaml = new YamlStream();
                yaml.Load(reader);
                var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
                if (mapping.Children.TryGetValue(new YamlScalarNode("Tenants"), out var tenantsNode))
                {
                    var tenants = tenantsNode as YamlMappingNode;
                    foreach (var tenant in tenants.Children)
                    {
                        var tenantKey = tenant.Key.ToString();
                        var tenantValue = tenant.Value as YamlMappingNode;
                        var tenantData = (YamlMappingNode)tenant.Value;
                        string rootDomain = ((YamlScalarNode)tenantData["RootDomain"]).Value;
                        _logger.Log(LogLevel.Information, $"Tenant: {tenantKey}, {rootDomain}");

                        // Get the KVS Arn for the system
                        var response = await _httpClient.GetAsync($"https://{rootDomain.TrimEnd('/')}/devconfig");
                        if (response.IsSuccessStatusCode)
                        {
                            var jsonContent = await response.Content.ReadAsStringAsync();
                            _currentConfig = JObject.Parse(jsonContent);

                            // Set all properties from JSON as environment variables
                            foreach (var prop in _currentConfig.Properties())
                            {
                                var propName = prop.Name.ToUpperInvariant();
                                var propValue = prop.Value.ToString();
                                _logger.LogInformation($"Setting environment variable {propName} to {propValue}");

                                if (propName == "KVSARN")
                                {
                                    var kvsArn = propValue;
                                    await _kvsService.GetKvsEntriesDictionaryAsync(kvsArn, rootDomain);
                                }
                            }

                            _logger.LogInformation($"Successfully applied dev configuration for {rootDomain}");
                        }
                        else
                        {
                            _logger.LogError($"Failed to fetch dev configuration. Status code: {response.StatusCode}" );
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dev configuration");
        }
    }
}
