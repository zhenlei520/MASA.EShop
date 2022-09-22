var builder = WebApplication.CreateBuilder(args);

var app = builder.Services
    .AddValidatorsFromAssembly(Assembly.GetEntryAssembly())
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Masa EShop - Catalog HTTP API",
            Version = "v1",
            Description = "The Catalog Service HTTP API"
        });
    })
    .AddIntegrationEventBus(options =>
    {
        options
        .UseDapr()
        .UseEventLog<CatalogDbContext>()
        .UseEventBus(eventBuilder => eventBuilder.UseMiddleware(typeof(ValidatorMiddleware<>)))
        .UseUoW<CatalogDbContext>(dbOptions => dbOptions.UseSqlServer());
    })
    .AddAutoInject()
    .AddServices(builder, options => options.DisableAutoMapRoute = true);

app.MigrateDbContext<CatalogDbContext>((context, services) =>
{
    var env = services.GetService<IWebHostEnvironment>()!;
    var settings = services.GetService<IOptions<CatalogSettings>>()!;
    var logger = services.GetService<ILogger<CatalogContextSeed>>()!;

    new CatalogContextSeed()
        .SeedAsync(context, env, settings, logger)
        .Wait();
});

app.UseSwagger().UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Masa EShop Service HTTP API v1");
});
app.UseRouting();
app.UseCloudEvents();
app.UseEndpoints(endpoint =>
{
    endpoint.MapSubscribeHandler();
});

app.Run();


