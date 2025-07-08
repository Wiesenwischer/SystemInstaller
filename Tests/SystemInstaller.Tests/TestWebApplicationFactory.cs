using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SystemInstaller.Web;
using SystemInstaller.Infrastructure.Data;

namespace SystemInstaller.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real database context registration
            services.RemoveAll(typeof(DbContextOptions<SystemInstallerDbContext>));
            services.RemoveAll(typeof(SystemInstallerDbContext));

            // Add In-Memory database for testing
            services.AddDbContext<SystemInstallerDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });
        });

        builder.UseEnvironment("Testing");
    }
}
