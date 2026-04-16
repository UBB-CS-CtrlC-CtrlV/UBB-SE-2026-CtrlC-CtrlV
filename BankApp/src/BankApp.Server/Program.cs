using BankApp.Contracts.Enums;
using BankApp.Server.DataAccess.TypeHandlers;
using BankApp.Server.Middleware;
using BankApp.Server.DependencyInjection;
using Dapper;
using Serilog;
using Serilog.Events;

const string DefaultLogFilePath = "logs/bankapp-server-.log";

SqlMapper.AddTypeHandler(new EnumTypeHandler<TransactionDirection>());
SqlMapper.AddTypeHandler(new EnumTypeHandler<TransactionStatus>());
SqlMapper.AddTypeHandler(new EnumTypeHandler<CardType>());
SqlMapper.AddTypeHandler(new EnumTypeHandler<CardStatus>());
SqlMapper.AddTypeHandler(new NotificationTypeHandler());

// Configure Serilog before building the host so that startup errors are also captured.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: DefaultLogFilePath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting BankApp.Server");

    var builder = WebApplication.CreateBuilder(args);

    // Replace the default MEL providers with Serilog. Configuration (log levels, sinks)
    // can be further overridden via appsettings.json under the "Serilog" key.
    builder.Host.UseSerilog((context, services, config) =>
        config
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                path: context.Configuration["Logging:FilePath"] ?? DefaultLogFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14));

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Paste your JWT token here"
        });
        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        });
    });

    // Allow the client to connect
    builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    app.UseExceptionHandler(a => a.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = "Something went wrong." });
    }));

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors();

    // Logs each HTTP request: method, path, status code, and duration.
    // Placed before middleware that may short-circuit the pipeline so all
    // requests are captured, including those rejected by session validation.
    app.UseSerilogRequestLogging();

    app.UseMiddleware<SessionValidationMiddleware>();

    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "BankApp.Server terminated unexpectedly");
}
finally
{
    // Flush and close all Serilog sinks before the process exits.
    Log.CloseAndFlush();
}

/// <summary>
/// Exposes the auto-generated Program class so integration tests can reference it
/// via <c>WebApplicationFactory&lt;Program&gt;</c>.
/// </summary>
public partial class Program
{
}
