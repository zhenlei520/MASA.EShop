var builder = WebApplication.CreateBuilder(args);

var app = builder.Services
    .AddValidatorsFromAssembly(Assembly.GetEntryAssembly())
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Masa EShop - Payment HTTP API",
            Version = "v1",
            Description = "The Payment Service HTTP API"
        });
    })
    .AddDomainEventBus(options =>
    {
        options.UseIntegrationEventBus(dispatcherOptions => dispatcherOptions.UseDapr().UseEventLog<PaymentDbContext>())
               .UseEventBus(eventBuilder => eventBuilder.UseMiddleware(typeof(ValidatorMiddleware<>)))
               .UseUoW<PaymentDbContext>(dbOptions => dbOptions.UseSqlServer())
               .UseRepository<PaymentDbContext>();
    })
    .AddServices(builder, options => options.DisableAutoMapRoute = true);

app.MigrateDbContext<PaymentDbContext>((context, services) =>
{
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

public partial class Program
{
    public static string Namespace = typeof(IntegrationEventService).Namespace!;
    public static string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);
}
