using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace SystemInstaller.Gateway;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services
        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp", policy =>
            {
                policy.WithOrigins("http://localhost:3000", "http://localhost:8080")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // Add Authentication with Keycloak for future API routes
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var keycloakConfig = builder.Configuration.GetSection("Keycloak");
                var authority = $"{keycloakConfig["Authority"]}/realms/{keycloakConfig["Realm"]}";
                
                options.Authority = authority;
                options.Audience = keycloakConfig["ClientId"];
                options.RequireHttpsMetadata = false; // For development
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false, // Allow any audience for now
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = authority,
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = "preferred_username"
                };

                // Optional: Log authentication events
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine($"Token validated for user: {context.Principal?.Identity?.Name ?? "Unknown"}");
                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.UseCors("AllowReactApp");
        app.UseAuthentication();
        app.UseAuthorization();

        // Health check endpoint (public)
        app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow })
           .WithName("HealthCheck");

        // Gateway info endpoint (public)
        app.MapGet("/gateway/info", () => new 
        { 
            Gateway = "SystemInstaller API Gateway",
            Version = "1.0.0",
            Timestamp = DateTime.UtcNow,
            Environment = app.Environment.EnvironmentName,
            Keycloak = new
            {
                Authority = app.Configuration["Keycloak:Authority"],
                Realm = app.Configuration["Keycloak:Realm"],
                ClientId = app.Configuration["Keycloak:ClientId"]
            }
        })
        .WithName("GatewayInfo");

        // Sample protected API endpoint
        app.MapGet("/api/user", (HttpContext context) => new
        {
            Message = "This is a protected endpoint",
            User = context.User.Identity?.Name,
            Claims = context.User.Claims.Select(c => new { Type = c.Type, Value = c.Value }),
            Timestamp = DateTime.UtcNow
        })
        .RequireAuthorization()
        .WithName("GetUser");

        // Sample public API endpoint
        app.MapGet("/api/public", () => new
        {
            Message = "This is a public endpoint",
            Timestamp = DateTime.UtcNow,
            Status = "Available"
        })
        .WithName("GetPublic");

        // Map the reverse proxy
        app.MapReverseProxy();

        app.Run();
    }
}
