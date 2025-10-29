using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using LegalCaseManagement.Data;
using LegalCaseManagement.Mapping;
using System.Reflection;
using Swashbuckle.AspNetCore.Filters;
using DocumentManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add DbContext (single registration)
builder.Services.AddDbContext<LegalCaseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Register document services
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddSingleton<IDocumentStorageService, DocumentStorageService>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

// Configure FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Legal Case Management API",
        Version = "v1",
        Description = "A comprehensive API for managing legal cases, hearings, deadlines, and reports",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Legal Case Management System",
            Email = "support@legalcasemanagement.com"
        }
    });

    // Include XML comments for better API documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Enable example filters
    c.ExampleFilters();
});

// Register swagger example providers
builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetExecutingAssembly());

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

// Configure JSON options
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Legal Case Management API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

// Enable CORS
app.UseCors("AllowAll");

// Configure middleware
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Apply migrations and ensure database is up-to-date
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LegalCaseDbContext>();
    try
    {
        // Apply any pending migrations (creates DB and tables if missing)
        context.Database.Migrate();

        // Seed initial data if tables are empty
        await DataSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating the database");
    }
}

// Configure global exception handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var errorResponse = new
        {
            message = "An unexpected error occurred",
            timestamp = DateTime.UtcNow,
            path = context.Request.Path
        };
        
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(errorResponse));
    });
});

app.Run();