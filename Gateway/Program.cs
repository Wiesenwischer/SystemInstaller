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
            
            // Fix correlation issues - ensure cookies work properly
            options.Cookie.IsEssential = true;
            options.Cookie.Domain = null; // Let the browser set the domain
            
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
            
            // Fix correlation issues
            options.Events.OnValidatePrincipal = context =>
            {
                return Task.CompletedTask;
            };
        })
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            var keycloakConfig = builder.Configuration.GetSection("Keycloak");
            var realm = keycloakConfig["Realm"] ?? "systeminstaller";
            
            // Build Keycloak URL from configurable host and port
            var keycloakHost = keycloakConfig["Host"] ?? "host.docker.internal";
            var keycloakPort = keycloakConfig["Port"] ?? "8082";
            var keycloakUrl = $"http://{keycloakHost}:{keycloakPort}";
            
            // Configuration for OIDC - use constructed URL for both Authority and MetadataAddress
            options.Authority = $"{keycloakUrl}/realms/{realm}";
            options.MetadataAddress = $"{keycloakUrl}/realms/{realm}/.well-known/openid-configuration";
            options.ClientId = keycloakConfig["ClientId"] ?? "systeminstaller-client";
            options.ClientSecret = keycloakConfig["ClientSecret"];
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.RequireHttpsMetadata = false;
            
            // Fix correlation issues - ensure proper redirect URIs
            options.CallbackPath = "/signin-oidc";
            options.SignedOutCallbackPath = "/signout-callback-oidc";
            options.RemoteSignOutPath = "/signout-oidc";
            
            // Enable PKCE and save tokens
            options.UsePkce = true;
            options.SaveTokens = true;
            
            // Configure scopes
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            
            // Fix correlation by configuring state and nonce properly
            options.ProtocolValidator.RequireNonce = true;
            options.ProtocolValidator.RequireState = true;
            options.ProtocolValidator.NonceLifetime = TimeSpan.FromMinutes(15);
            
            // Relax validation for development
            options.TokenValidationParameters.ValidateIssuer = false;
            options.TokenValidationParameters.ValidateAudience = false;
            options.TokenValidationParameters.ValidateLifetime = true;
            options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(5);
            
            // Map claims
            options.TokenValidationParameters.NameClaimType = "preferred_username";
            options.TokenValidationParameters.RoleClaimType = "realm_access.roles";
            
            // Events for debugging and error handling
            options.Events = new OpenIdConnectEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"❌ Authentication failed: {context.Exception.Message}");
                    if (context.Exception.InnerException != null)
                    {
                        Console.WriteLine($"❌ Inner exception: {context.Exception.InnerException.Message}");
                    }
                    
                    // Handle correlation failures specifically
                    if (context.Exception.Message.Contains("Correlation failed") || 
                        context.Exception.Message.Contains("correlation"))
                    {
                        Console.WriteLine("🔧 Correlation failure detected - clearing authentication cookies");
                        context.Response.Cookies.Delete("SystemInstaller.Auth");
                        context.Response.Cookies.Delete(".AspNetCore.OpenIdConnect.Nonce.CfDJ8");
                        context.Response.Cookies.Delete(".AspNetCore.Correlation.OpenIdConnect");
                    }
                    
                    // Redirect to a simple error page instead of throwing
                    context.Response.Redirect("/auth/error");
                    context.HandleResponse();
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine($"✅ Token validated for user: {context.Principal?.Identity?.Name ?? "Unknown"}");
                    return Task.CompletedTask;
                },
                OnTicketReceived = context =>
                {
                    Console.WriteLine("🎫 Authentication ticket received");
                    return Task.CompletedTask;
                },
                OnRemoteFailure = context =>
                {
                    Console.WriteLine($"❌ Remote failure: {context.Failure?.Message}");
                    
                    // Handle correlation failures in remote failure as well
                    if (context.Failure?.Message?.Contains("Correlation failed") == true)
                    {
                        Console.WriteLine("🔧 Remote correlation failure - clearing cookies and retrying");
                        context.Response.Cookies.Delete("SystemInstaller.Auth");
                        context.Response.Cookies.Delete(".AspNetCore.OpenIdConnect.Nonce.CfDJ8");
                        context.Response.Cookies.Delete(".AspNetCore.Correlation.OpenIdConnect");
                    }
                    
                    context.Response.Redirect("/auth/error");
                    context.HandleResponse();
                    return Task.CompletedTask;
                },
                OnRedirectToIdentityProvider = context =>
                {
                    Console.WriteLine($"🔄 Redirecting to identity provider: {context.ProtocolMessage.IssuerAddress}");
                    // Ensure proper redirect URI
                    var redirectUri = $"{context.Request.Scheme}://{context.Request.Host}{options.CallbackPath}";
                    context.ProtocolMessage.RedirectUri = redirectUri;
                    Console.WriteLine($"🔗 Setting redirect URI to: {redirectUri}");
                    return Task.CompletedTask;
                },
                OnRedirectToIdentityProviderForSignOut = context =>
                {
                    Console.WriteLine("🚪 Redirecting to identity provider for sign out");
                    // Allow redirect back to the app after logout for better UX
                    var redirectUri = $"{context.Request.Scheme}://{context.Request.Host}/";
                    context.ProtocolMessage.PostLogoutRedirectUri = redirectUri;
                    Console.WriteLine($"🔗 Post logout redirect URI set to: {redirectUri}");
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
        
        // Custom middleware to handle authentication for frontend routes
        app.Use(async (context, next) =>
        {
            var path = context.Request.Path.Value;
            var user = context.User;
            
            // Debug logging
            Console.WriteLine($"🔍 Middleware - Path: {path}, Authenticated: {user?.Identity?.IsAuthenticated}, User: {user?.Identity?.Name}");
            
            // Skip authentication middleware for OIDC callback endpoints and authentication-related paths
            if (path != null && (
                path.StartsWith("/signin-oidc") ||
                path.StartsWith("/signout-callback-oidc") ||
                path.StartsWith("/auth/") ||
                path.StartsWith("/api/") ||
                path.StartsWith("/_framework/") ||
                path.StartsWith("/_content/") ||
                path.StartsWith("/css/") ||
                path.StartsWith("/js/") ||
                path.StartsWith("/favicon.ico")))
            {
                Console.WriteLine($"⏭️  Skipping middleware for path: {path}");
                await next();
                return;
            }
            
            // If accessing root path and not authenticated, redirect to login
            if (user?.Identity?.IsAuthenticated != true && 
                path == "/" &&
                context.Request.Method == "GET")
            {
                Console.WriteLine($"🔒 Unauthenticated user accessing root, redirecting to login");
                context.Response.Redirect("/auth/login");
                return;
            }
            
            // If user is authenticated and accessing root, allow access to frontend
            if (user?.Identity?.IsAuthenticated == true && path == "/")
            {
                Console.WriteLine($"✅ Authenticated user {user.Identity.Name} accessing root, allowing access");
                await next();
                return;
            }
            
            await next();
        });
        
        // Map controllers
        app.MapControllers();

        // Public health check endpoint
        app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow })
           .WithName("HealthCheck")
           .AllowAnonymous();

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
        .WithName("GatewayInfo")
        .AllowAnonymous();

        // Authentication endpoints
        app.MapGet("/auth/login", (HttpContext context) =>
        {
            // If user is already authenticated, redirect to home
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                Console.WriteLine($"✅ User {context.User.Identity.Name} already authenticated, redirecting to home");
                return Results.Redirect("/");
            }
            
            // Otherwise, trigger authentication challenge
            Console.WriteLine($"🔄 Triggering authentication challenge for unauthenticated user");
            return Results.Challenge();
        })
           .WithName("Login")
           .AllowAnonymous();

        // Add /auth/logout endpoints - these are the correct authentication endpoints
        app.MapGet("/auth/logout", async (HttpContext context) =>
        {
            // Sign out from both local cookies and OIDC provider (Keycloak)
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            
            // The OIDC sign out will handle the redirect to Keycloak and back
            return Results.Empty;
        })
        .WithName("AuthLogoutGet")
        .AllowAnonymous();

        app.MapPost("/auth/logout", async (HttpContext context) =>
        {
            // Sign out from both local cookies and OIDC provider (Keycloak)
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            
            // The OIDC sign out will handle the redirect to Keycloak and back
            return Results.Empty;
        })
        .WithName("AuthLogoutPost")
        .AllowAnonymous();

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

        // Authentication error endpoint for handling correlation failures
        app.MapGet("/auth/error", (HttpContext context) =>
        {
            var errorMessage = context.Request.Query["error"].FirstOrDefault() ?? "Authentication failed";
            return Results.Ok(new 
            { 
                error = "authentication_failed",
                message = errorMessage,
                details = "There was an issue with the authentication process. Please try logging in again.",
                timestamp = DateTime.UtcNow,
                actions = new[] { "Clear browser cookies", "Try logging in again", "Contact support if issue persists" }
            });
        })
        .WithName("AuthError")
        .AllowAnonymous();

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

        // Map the reverse proxy for frontend routes (no global auth requirement)
        app.MapReverseProxy();

        app.Run();
    }
}
