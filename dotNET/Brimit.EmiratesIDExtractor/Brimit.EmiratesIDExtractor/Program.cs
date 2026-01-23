using Brimit.EmiratesIDExtractor.Configuration;
using Brimit.EmiratesIDExtractor.Services;
using Serilog;
using System.Reflection;
using Brimit.EmiratesIDExtractor.Factories;
using Brimit.EmiratesIDExtractor.Processors;

// Configure Serilog from appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .Build())
    .CreateLogger();

try
{
    Log.Information("Starting Emirates ID Extractor API");
    
    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddControllers();
    
    // Configure Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Emirates ID Extractor API",
            Version = "v1",
            Description = "POC service for extracting data from Emirates ID documents using Azure AI Document Intelligence",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "Brimit",
                Email = "support@brimit.com"
            }
        });
        
        // Include XML comments for Swagger documentation
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // Configure options
    builder.Services.Configure<DocumentIntelligenceOptions>(
        builder.Configuration.GetSection(DocumentIntelligenceOptions.SectionName));

    // Register services
    builder.Services.AddSingleton<IPdfService, PdfService>();
    builder.Services.AddSingleton<IDocumentIntelligenceService, DocumentIntelligenceService>();
    builder.Services.AddSingleton<IDocumentProcessorFactory, DocumentProcessorFactory>();
    builder.Services.AddSingleton<EmiratesIdProcessor>();

    // Configure CORS for development
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentCors", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Add health checks
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseCors("DevelopmentCors");
    }

    // Enable Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Emirates ID Extractor API v1");
        c.RoutePrefix = "swagger"; // Serve Swagger UI at root
    });

    app.UseHttpsRedirection();

    // Use Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseAuthorization();

    app.MapControllers();
    
    app.MapHealthChecks("/health");

    // Log startup information
    Log.Information("Emirates ID Extractor API started successfully");
    Log.Information("Swagger UI available at: {SwaggerUrl}", app.Urls.FirstOrDefault(u => u.StartsWith("https")) ?? (app.Environment.IsDevelopment() ? "https://localhost:44357" : "https://yourdomain.com"));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}