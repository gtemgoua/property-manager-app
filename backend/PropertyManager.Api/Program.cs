using Microsoft.EntityFrameworkCore;
using PropertyManager.Api.Background;
using PropertyManager.Api.Data;
using PropertyManager.Api.Mapping;
using PropertyManager.Api.Options;
using PropertyManager.Api.Services.Implementations;
using PropertyManager.Api.Services.Interfaces;
using Serilog;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string is not configured.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddAutoMapper(typeof(DomainProfile).Assembly);

builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IRentalUnitService, RentalUnitService>();
builder.Services.AddScoped<IRentalContractService, RentalContractService>();
builder.Services.AddScoped<IRentPaymentService, RentPaymentService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddHostedService<LatePaymentAlertWorker>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("client", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["*"];
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();

        var exposed = builder.Configuration.GetSection("Cors:ExposedHeaders").Get<string[]>();
        if (exposed is not null && exposed.Length > 0)
        {
            policy.WithExposedHeaders(exposed);
        }
    });
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<PropertyManager.Api.Filters.NormalizeDateTimeFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PropertyManager API",
        Version = "v1",
        Description = "API for Property Manager application"
    });
});

var app = builder.Build();

// Expose Swagger UI at /swagger for easier local testing. Keep it enabled so OpenAPI docs are always available
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PropertyManager API v1");
    c.RoutePrefix = "swagger"; // serve at /swagger
});

app.UseSerilogRequestLogging();

app.UseCors("client");
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Log the migration failure and continue. This avoids crashing the container
        // when there are pending model changes in development environments.
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Database migration failed on startup — continuing without applying migrations.");
    }
}

app.Run();
