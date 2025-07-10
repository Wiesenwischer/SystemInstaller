using SystemInstaller.Components; // For App component
using SystemInstaller.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SystemInstaller.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();

// Add distributed memory cache for session support
builder.Services.AddDistributedMemoryCache();

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Infrastructure and Application layers
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Authentication with Keycloak (OpenID Connect)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = builder.Configuration["Authentication:Keycloak:Authority"];
    options.ClientId = builder.Configuration["Authentication:Keycloak:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Keycloak:ClientSecret"];
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.RequireHttpsMetadata = false; // For development
    
    // Map claims
    options.TokenValidationParameters.NameClaimType = "preferred_username";
    options.TokenValidationParameters.RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
    
    // Ensure roles are properly mapped from different claim sources
    options.ClaimActions.Clear(); // Clear default claim actions
    options.ClaimActions.MapJsonSubKey("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "realm_access", "roles");
    options.ClaimActions.MapJsonKey("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "roles");
    options.ClaimActions.MapJsonKey("preferred_username", "preferred_username");
    options.ClaimActions.MapJsonKey("email", "email");
    
    options.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = context =>
        {
            // Debug: Log all claims
            var claims = context.Principal?.Claims?.ToList();
            if (claims != null)
            {
                Console.WriteLine("=== RECEIVED CLAIMS ===");
                foreach (var claim in claims)
                {
                    Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
                }
                Console.WriteLine("=== END CLAIMS ===");
                
                // Check if user has admin role - try different approaches
                var hasAdminRole1 = context.Principal?.IsInRole("admin") ?? false;
                var hasAdminRole2 = context.Principal?.FindAll(System.Security.Claims.ClaimTypes.Role)?.Any(c => c.Value == "admin") ?? false;
                var hasAdminRole3 = context.Principal?.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Any(c => c.Value == "admin") ?? false;
                
                Console.WriteLine($"User has admin role (IsInRole): {hasAdminRole1}");
                Console.WriteLine($"User has admin role (ClaimTypes.Role): {hasAdminRole2}");
                Console.WriteLine($"User has admin role (explicit claim type): {hasAdminRole3}");
                
                // Also check for roles in different claim types
                var roleClaims = claims.Where(c => c.Type.Contains("role", StringComparison.OrdinalIgnoreCase)).ToList();
                Console.WriteLine($"Role claims found: {roleClaims.Count}");
                foreach (var roleClaim in roleClaims)
                {
                    Console.WriteLine($"Role claim - Type: {roleClaim.Type}, Value: {roleClaim.Value}");
                }
            }
            return Task.CompletedTask;
        },
        OnAccessDenied = context =>
        {
            context.HandleResponse();
            context.Response.Redirect("/");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("admin"));
    options.AddPolicy("CustomerPolicy", policy => policy.RequireRole("customer", "admin"));
});

// Legacy services (to be migrated gradually)
// Note: Legacy services removed - using Application layer instead


var app = builder.Build();

// Migrationen beim Start anwenden, aber erst wenn SQL Server erreichbar ist (Retry-Logik)
var maxRetries = 15;
var delay = TimeSpan.FromSeconds(4);
var retries = 0;
bool migrationCompleted = false;

while (retries < maxRetries && !migrationCompleted)
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SystemInstallerDbContext>();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Versuche Datenbankverbindung (Versuch {retries + 1}/{maxRetries})...");
            
            // Erstelle Datenbank falls sie nicht existiert und führe Migration aus
            db.Database.Migrate();
            
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Datenbank-Migration erfolgreich abgeschlossen.");
            migrationCompleted = true;
        }
    }
    catch (Exception ex)
    {
        retries++;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Datenbankverbindung/Migration fehlgeschlagen (Versuch {retries}/{maxRetries}): {ex.Message}");
        if (retries >= maxRetries)
        {
            Console.WriteLine($"Fehler bei Migrationen nach {maxRetries} Versuchen: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
        Thread.Sleep(delay);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Authentication endpoints
app.MapGet("/Account/Login", async (HttpContext context) =>
{
    await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
});

app.MapPost("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .RequireAuthorization(); // Requires authentication for all pages

app.Run();

// Make Program class accessible for integration testing
public partial class Program { }
