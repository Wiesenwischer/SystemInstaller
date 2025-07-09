using SystemInstaller.Web.Components;
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
    options.TokenValidationParameters.RoleClaimType = "realm_access.roles";
    
    options.Events = new OpenIdConnectEvents
    {
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
