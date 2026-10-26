using Fiap.Soat.VehicleSales.Api.Endpoints;
using Fiap.Soat.VehicleSales.Api.Shared.Extensions;
using Fiap.Soat.VehicleSales.Api.Shared.HealthChecks;
using Fiap.Soat.VehicleSales.Api.Shared.Middlewares;
using Fiap.Soat.VehicleSales.Application.Shared;
using Fiap.Soat.VehicleSales.Infrastructure.Data;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

_ = builder.Services.AddEndpointsApiExplorer();
_ = builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
_ = builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
_ = builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


// Configure MySQL
var mySqlConnectionString = builder.Configuration.GetConnectionString("MySql")
                            ?? throw new InvalidOperationException("Connection string 'MySql' was not configured.");
_ = builder.Services.AddDbContext<AppDbContext>((options) => options.UseMySql(mySqlConnectionString, ServerVersion.AutoDetect(mySqlConnectionString)));

// Add services to the container.
_ = builder.Services.AddServiceExtensions();
_ = builder.Services.AddRepositoryExtensions();
_ = builder.Services.AddUseCases();
_ = builder.Services.AddHttpContextAccessor();
_ = builder.Services.AddHealthChecks()
    .AddCheck<DetailedHealthCheck>("detailed")
    .AddDbContextCheck<AppDbContext>("database");
_ = builder.Services.AddRouting(options => options.LowercaseUrls = true);
_ = builder.Services.AddMemoryCache();
_ = builder.Services.AddInterfaceAdapters();

_ = builder.Services.AddOpenApi();
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
_ = builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
_ = builder.Services.AddHttpClient();
_ = builder.Services.AddHttpLogging();

var app = builder.Build();

_ = app.UseCors("AllowedOrigins");
_ = app.UseHttpLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

_ = app.UseHttpsRedirection();

_ = app.MapScalarApiReference();
_ = app.UseMiddleware<ExceptionMiddleware>();
_ = app.UseAuthorization();
_ = app.MapControllers();
_ = app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

_ = SaleEndpoints.Map(app);
_ = VehiclesEndpoints.Map(app);
_ = WebhookEndpoints.Map(app);
_ = PaymentGatewayEndpoints.Map(app);

await app.RunAsync();

[ExcludeFromCodeCoverage]
public partial class Program
{
    protected Program() { }
}
