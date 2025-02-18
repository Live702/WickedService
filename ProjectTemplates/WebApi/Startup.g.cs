public partial class Startup
{
    private class AwsOptions
    {
        public string Profile { get; set; }
        public string Region { get; set; }
        public string UserPoolId { get; set; }
    }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        var profile = Environment.GetEnvironmentVariable("AWSPROFILE");
        if (string.IsNullOrEmpty(profile)) throw new Exception("AWSPROFILE environment variable empty or missing");
        var region = Environment.GetEnvironmentVariable("AWSREGION");
        if (string.IsNullOrEmpty(region)) throw new Exception("AWSREGION environment variable empty or missing");
        services.AddDefaultAWSOptions(new AWSOptions() { Profile = profile, Region = Amazon.RegionEndpoint.GetBySystemName(region) });
        services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();
        services.AddAWSService<IAmazonSecurityTokenService>();

        services.AddHttpClient<DevConfigService>();
        services.AddSingleton<DevConfigService>();
        services.AddSingleton<KvsService>();

        ConfigureSvcs(services);

        services.AddCors(opt =>
        {
            opt.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services
            .AddControllers()
            .AddNewtonsoftJson();

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DevConfigService devConfigService)
    {
        devConfigService.FetchAndApplyDevConfigAsync().Wait();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCustomRouting();
        app.UseRouting();
        app.UseCors();
        app.UseWebSockets(new WebSocketOptions()
        {
            KeepAliveInterval = TimeSpan.FromSeconds(120)
        });
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

    }
}