var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
var app = builder.Services
    .AddEndpointsApiExplorer() //fixed aggregatewxception
    .AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Masa EShop - Basket HTTP API",
            Version = "v1",
            Description = "The Basket Service HTTP API"
        });
    })
    .AddScoped<IBasketRepository, BasketRepository>()
    .AddServices(builder, options => options.DisableAutoMapRoute = true);

app.UseRouting();
app.UseCloudEvents();
app.UseEndpoints(endpoints =>
{
    endpoints.MapSubscribeHandler();
});

app.UseSwagger().UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Masa EShop Service HTTP API v1");
});
app.Run();
