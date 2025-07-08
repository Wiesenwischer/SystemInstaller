using SystemInstaller.Web.Components;
using SystemInstaller.Web.Services;
using Microsoft.EntityFrameworkCore;
using SystemInstaller.Web.Data;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Entity Framework DbContext mit SQL Server
builder.Services.AddDbContext<SystemInstallerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    var keycloakConfig = builder.Configuration.GetSection("Authentication:Keycloak");
    
    options.Authority = keycloakConfig["Authority"];
    options.ClientId = keycloakConfig["ClientId"];
    options.ClientSecret = keycloakConfig["ClientSecret"];
    options.ResponseType = keycloakConfig["ResponseType"] ?? "code";
    options.RequireHttpsMetadata = keycloakConfig.GetValue<bool>("RequireHttpsMetadata");
    
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    
    options.GetClaimsFromUserInfoEndpoint = true;
    options.SaveTokens = true;
    
    options.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProviderForSignOut = context =>
        {
            var logoutUri = keycloakConfig["Authority"] + "/protocol/openid-connect/logout?client_id=" + 
                           keycloakConfig["ClientId"] + "&post_logout_redirect_uri=" + 
                           Uri.EscapeDataString(context.Request.Scheme + "://" + context.Request.Host);
            context.Response.Redirect(logoutUri);
            context.HandleResponse();
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Register custom services
builder.Services.AddHttpClient<AgentApiClient>();
builder.Services.AddSingleton<AgentApiClient>();
// InstallationService wird später angepasst (Scoped)
builder.Services.AddScoped<InstallationService>();

// Tenant Management Services
builder.Services.AddScoped<TenantService>();
builder.Services.AddScoped<InvitationService>();


var app = builder.Build();

// Migrationen beim Start anwenden, aber erst wenn SQL Server erreichbar ist (Retry-Logik)
var maxRetries = 15;
var delay = TimeSpan.FromSeconds(4);
var retries = 0;
bool dbAvailable = false;
while (retries < maxRetries && !dbAvailable)
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SystemInstallerDbContext>();
            db.Database.CanConnect(); // Testverbindung
            dbAvailable = true;
            db.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        retries++;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] SQL Server nicht erreichbar (Versuch {retries}/{maxRetries}): {ex.Message}");
        if (retries >= maxRetries)
        {
            Console.WriteLine($"Fehler bei Migrationen: {ex.Message}\n{ex.StackTrace}");
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

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .RequireAuthorization(); // Requires authentication for all pages

// Add login/logout endpoints
app.MapGet("/login", async (HttpContext context) =>
{
    await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
});

app.MapPost("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
});

app.Run();
