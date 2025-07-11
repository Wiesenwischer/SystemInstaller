using SystemInstaller.Gateway.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.AspNetCore.Authentication;

namespace SystemInstaller.Gateway;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services
        builder.Services.AddControllers();
        builder.Services.AddHttpClient();
        
        // Add Authentication with OpenID Connect (Keycloak)
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = "SystemInstaller.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // For development
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromHours(24);
            options.SlidingExpiration = true;
            
            // Handle unauthenticated requests
            options.Events.OnRedirectToLogin = context =>
            {
                // For API requests, return 401 instead of redirect
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }
                
                // For web requests, redirect to Keycloak
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
        })
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            var keycloakConfig = builder.Configuration.GetSection("Keycloak");
            
            // Use external URL for Authority (browser redirects)
            var keycloakExternalUrl = keycloakConfig["ExternalUrl"]; // localhost:8082
            // Use internal URL for metadata (container communication)
            var keycloakInternalUrl = keycloakConfig["Url"]; // keycloak:8080
            // Use host URL for JwksUri (container-to-host communication)
            var keycloakHostUrl = keycloakConfig["HostUrl"]; // host.docker.internal:8082
            var realm = keycloakConfig["Realm"];
            
            // Debug logging
            Console.WriteLine($"DEBUG: KeycloakExternalUrl = {keycloakExternalUrl}");
            Console.WriteLine($"DEBUG: KeycloakInternalUrl = {keycloakInternalUrl}");
            Console.WriteLine($"DEBUG: KeycloakHostUrl = {keycloakHostUrl}");
            
            // Authority must be external URL for browser redirects
            options.Authority = $"{keycloakExternalUrl}/realms/{realm}";
            Console.WriteLine($"DEBUG: Authority = {options.Authority}");
            
            options.ClientId = keycloakConfig["ClientId"]!;
            options.ClientSecret = keycloakConfig["ClientSecret"];
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.RequireHttpsMetadata = false; // For development
            
            // Create a custom configuration manager that fetches metadata from HostUrl but returns modified configuration
            var metadataAddress = $"{keycloakHostUrl}/realms/{realm}/.well-known/openid-configuration";
            var httpDocumentRetriever = new HttpDocumentRetriever()
            {
                RequireHttps = false // Allow HTTP for development
            };
            
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                metadataAddress,
                new OpenIdConnectConfigurationRetriever(),
                httpDocumentRetriever);
            
            // Create a wrapper that modifies the configuration to use correct URLs
            options.ConfigurationManager = new CustomConfigurationManager(
                configurationManager,
                keycloakExternalUrl ?? "http://localhost:8082",
                keycloakInternalUrl ?? "http://keycloak:8080",
                realm ?? "systeminstaller");
            
            // Allow issuer mismatch since we're using mixed URLs
            options.TokenValidationParameters.ValidateIssuer = false;
            
            Console.WriteLine($"DEBUG: Authority = {options.Authority}");
            Console.WriteLine($"DEBUG: Using CustomConfigurationManager with metadata from: {metadataAddress}");
            Console.WriteLine($"DEBUG: ValidateIssuer = {options.TokenValidationParameters.ValidateIssuer}");
            
            // Test discovery endpoint connectivity
            try 
            {
                using var httpClient = new HttpClient();
                var discoveryAddress = $"{keycloakHostUrl}/realms/{realm}/.well-known/openid-configuration";
                Console.WriteLine($"DEBUG: Testing discovery endpoint connectivity to {discoveryAddress}");
                var response = httpClient.GetAsync(discoveryAddress).Result;
                Console.WriteLine($"DEBUG: Discovery endpoint response status: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"DEBUG: Discovery endpoint content length: {content.Length}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Discovery endpoint connectivity test failed: {ex.Message}");
            }
            
            // Configure scopes
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.Scope.Add("roles");
            
            // Save tokens in authentication properties
            options.SaveTokens = true;
            
            // Map claims
            options.TokenValidationParameters.NameClaimType = "preferred_username";
            options.TokenValidationParameters.RoleClaimType = "realm_access.roles";
            
            // Configure events for debugging
            options.Events = new OpenIdConnectEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                    if (context.Exception.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {context.Exception.InnerException.Message}");
                    }
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine($"Token validated for user: {context.Principal?.Identity?.Name ?? "Unknown"}");
                    return Task.CompletedTask;
                },
                OnRemoteFailure = context =>
                {
                    Console.WriteLine($"Remote failure: {context.Failure?.Message}");
                    return Task.CompletedTask;
                },
                OnAuthorizationCodeReceived = context =>
                {
                    Console.WriteLine($"Authorization code received");
                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddAuthorization();
        
        // Register Keycloak service (still useful for API operations)
        builder.Services.AddScoped<IKeycloakService, KeycloakService>();
        
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

        var app = builder.Build();

        app.UseCors("AllowReactApp");
        
        // Use built-in authentication middleware
        app.UseAuthentication();
        app.UseAuthorization();
        
        // Map controllers
        app.MapControllers();

        // Public health check endpoint
        app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow })
           .WithName("HealthCheck");

        // Public gateway info endpoint
        app.MapGet("/gateway/info", () => new 
        { 
            Gateway = "SystemInstaller API Gateway",
            Version = "1.0.0",
            Timestamp = DateTime.UtcNow,
            Environment = app.Environment.EnvironmentName,
            Keycloak = new
            {
                Url = app.Configuration["Keycloak:Url"],
                Realm = app.Configuration["Keycloak:Realm"],
                ClientId = app.Configuration["Keycloak:ClientId"]
            }
        })
        .WithName("GatewayInfo");

        // Authentication endpoints
        app.MapGet("/auth/login", () => Results.Challenge())
           .WithName("Login");
           
        app.MapPost("/auth/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            return Results.Ok(new { message = "Logged out successfully" });
        })
        .WithName("Logout");

        app.MapGet("/auth/user", (HttpContext context) =>
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                return Results.Unauthorized();
            }
            
            return Results.Ok(new
            {
                sub = context.User.FindFirst("sub")?.Value,
                username = context.User.Identity?.Name,
                email = context.User.FindFirst("email")?.Value,
                firstName = context.User.FindFirst("given_name")?.Value,
                lastName = context.User.FindFirst("family_name")?.Value,
                roles = context.User.FindAll("realm_access.roles").Select(c => c.Value).ToArray()
            });
        })
        .RequireAuthorization()
        .WithName("GetUser");

        // Protected API example
        app.MapGet("/api/protected", (HttpContext context) => new
        {
            Message = "This is a protected endpoint",
            User = context.User.Identity?.Name,
            Claims = context.User.Claims.Select(c => new { Type = c.Type, Value = c.Value }),
            Timestamp = DateTime.UtcNow
        })
        .RequireAuthorization()
        .WithName("ProtectedEndpoint");

        // Map the reverse proxy for other services with authentication
        app.MapReverseProxy(proxyPipeline =>
        {
            // Require authentication for all proxied requests
            proxyPipeline.UseAuthentication();
            proxyPipeline.UseAuthorization();
        }).RequireAuthorization();

        app.Run();
    }
}
