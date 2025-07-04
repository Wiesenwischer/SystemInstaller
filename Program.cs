using SystemInstaller.Web.Components;
using SystemInstaller.Web.Services;


using Microsoft.EntityFrameworkCore;
using SystemInstaller.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Entity Framework DbContext mit SQL Server
builder.Services.AddDbContext<SystemInstallerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register custom services
builder.Services.AddHttpClient<AgentApiClient>();
builder.Services.AddSingleton<AgentApiClient>();
// InstallationService wird später angepasst (Scoped)
builder.Services.AddScoped<InstallationService>();


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
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
