
using Microsoft.EntityFrameworkCore;
using SsoBackend.Infrastructure; // <-- adjust namespace if different
using SsoBackend.API.Routes;
using DotNetEnv;


// Load .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure SQL Server connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=SsoDb;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
    
// Register GoogleOAuthService for DI
builder.Services.AddScoped<GoogleOAuthService>();

// Register PlexOAuthService for DI
builder.Services.AddHttpClient<PlexOAuthService>();

// Register JwtService for DI
builder.Services.AddSingleton<SsoBackend.Infrastructure.JwtService>();

// Enable CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
    );
});

var app = builder.Build();
// Use CORS
app.UseCors("FrontendPolicy");
// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();


// Register endpoints from separate routes
app.MapPlexAuthRoutes();
app.MapGoogleAuthRoutes();
app.MapUserRoutes();

app.Run();
