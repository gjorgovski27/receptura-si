using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CookingAssistantAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()   // Allows all origins
                  .AllowAnyHeader()   // Allows all headers
                  .AllowAnyMethod();  // Allows all HTTP methods (GET, POST, PUT, DELETE)
        });
});

// Register the DbContext
builder.Services.AddDbContext<CookingAssistantDbContext>(options =>
    options.UseSqlite("Data Source=CookingAssistant.db")); // Use your SQLite connection string

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKey"; // Replace with a secure key
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
builder.Services.AddRazorPages(); // Add Razor Pages support

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Middleware pipeline
app.UseHttpsRedirection();
app.UseStaticFiles(); // Must be placed before UseRouting and Razor Pages/Controllers
app.UseRouting();

app.UseAuthentication(); // This must be added before UseAuthorization
app.UseAuthorization();

app.MapControllers();  // Map API controllers
app.MapRazorPages();   // Map Razor Pages

app.Run();
