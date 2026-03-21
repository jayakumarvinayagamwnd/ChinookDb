using Serilog;
using Chinook.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Chinook.API.Infrastructure.DependencyInjection;
using Chinook.API.Infrastructure.Caching;
using Chinook.API.Common.DependencyInjection;
using Chinook.API.Common.Exceptions;
using Chinook.API.Features.Analytics;
using Chinook.API.Features.Billing;
using Chinook.API.Features.Catalog;
using Chinook.API.Features.Customers;
using Chinook.API.Features.Employees;
using Chinook.API.Features.Playlists;

var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
Directory.CreateDirectory(logsPath);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Chinook.API")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Application} {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        Path.Combine(logsPath, "log-.txt"), 
        rollingInterval: RollingInterval.Day, 
        fileSizeLimitBytes: 20_097_152, 
        rollOnFileSizeLimit: true,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Application} {Message:lj}{NewLine}{Exception}", 
        retainedFileCountLimit: 30)
    .CreateLogger();
try
{
    Log.Information("Starting web host");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi();
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
    // Register MediatR services from the assembly containing MediatRServiceCollections
    builder.Services.AddMediatRServices();
    // Register FluentValidation validators and MediatR validation behavior
    builder.Services.AddFluentValidationServices();
    // Register AutoMapper profiles from the current assembly
    builder.Services.AddAutoMapperServices();
    // Register the DbContext with a connection string from configuration
    builder.Services.AddChinookDbContext(builder.Configuration);
    // Register Redis IDistributedCache and CachingBehavior pipeline behavior
    builder.Services.AddCacheServices(builder.Configuration);
   
    //builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    app.UseExceptionHandler();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();   
    app.MapCatalogEndpoints();
    app.MapPlaylistEndpoints();
    app.MapCustomerEndpoints();
    app.MapBillingEndpoints();
    app.MapEmployeeEndpoints();
    app.MapAnalyticsEndpoints();

    Log.Information("Scalar UI quick access: http://localhost:5185/scalar/v1");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
