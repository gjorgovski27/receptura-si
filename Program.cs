using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CookingAssistantAPI.Data;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Configure Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddAzureWebAppDiagnostics(); // Ensure this is supported
builder.Services.Configure<Microsoft.Extensions.Logging.AzureAppServices.AzureFileLoggerOptions>(options =>
{
    options.FileName = "azure-diagnostic-";
    options.FileSizeLimit = 50 * 1024; // 50MB
    options.RetainedFileCountLimit = 5;
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Configure the database path
var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CookingAssistant.db");
Console.WriteLine($"Using Database Path: {dbPath}");
builder.Services.AddDbContext<CookingAssistantDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "Y2fFP9qR8sBk5xGglJvV2ZB7WlgD1U2HSkB4j3XZGs8=";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CookingAssistantAPI";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Enable Authorization
builder.Services.AddAuthorization();

// Register controllers and Razor Pages
builder.Services.AddControllers();
builder.Services.AddRazorPages();

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure database migrations are applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CookingAssistantDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();

// Debugging info
Console.WriteLine($"Current Directory: {Environment.CurrentDirectory}");
Console.WriteLine($"Database File Exists: {File.Exists(dbPath)}");
