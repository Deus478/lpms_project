using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using LegalCaseManagement.Data;
using LegalCaseManagement.Mapping;
using System.Reflection;
using Swashbuckle.AspNetCore.Filters;
using DocumentManagement.Services;
using LegalCaseManagement.Infrastructure.Auth;
using LegalCaseManagement.Infrastructure.Audit;
using LegalCaseManagement.Infrastructure.Bootstrap;
using LegalCaseManagement.Infrastructure.Notifications;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditActionFilter>();
});

// Add DbContext (single registration)
builder.Services.AddDbContext<LegalCaseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Configure authentication/authorization (JWT)
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<AuditActionFilter>();

var jwtKey = builder.Configuration["Auth:JwtKey"];
var issuer = builder.Configuration["Auth:Issuer"];
var audience = builder.Configuration["Auth:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = string.IsNullOrWhiteSpace(jwtKey)
                ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes("DEV_FALLBACK_KEY_CHANGE_ME"))
                : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
            ValidIssuer = issuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(audience),
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

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

// Background notification generation
builder.Services.AddHostedService<NotificationSchedulerHostedService>();

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
    // Show detailed errors during development
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Legal Case Management API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();
// Enable CORS (must be between UseRouting and UseAuthorization when using endpoint routing)
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseMiddleware<DevBypassAuthMiddleware>();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Apply migrations and ensure database is up-to-date
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LegalCaseDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var autoMigrate = builder.Configuration.GetValue<bool>("Database:AutoMigrate");
        if (autoMigrate)
        {
            logger.LogInformation("Database AutoMigrate enabled; applying EF Core migrations...");
            await context.Database.MigrateAsync();
            await BootstrapSeeder.SeedAsync(context);
            logger.LogInformation("Database migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("Database AutoMigrate disabled; skipping EF Core migrations.");
        }
    }
    catch (Exception ex)
    {
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