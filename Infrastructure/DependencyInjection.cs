using Microsoft.EntityFrameworkCore;
using SystemInstaller.Application.Interfaces;
using SystemInstaller.Application.UseCases;
using SystemInstaller.Domain.Repositories;
using SystemInstaller.Domain.Services;
using SystemInstaller.Infrastructure.Data;
using SystemInstaller.Infrastructure.Repositories;
using SystemInstaller.Infrastructure.Services;

namespace SystemInstaller.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<SystemInstallerDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITenantUserRepository, TenantUserRepository>();
        services.AddScoped<IUserInvitationRepository, UserInvitationRepository>();
        services.AddScoped<IInstallationEnvironmentRepository, InstallationEnvironmentRepository>();
        services.AddScoped<IInstallationTaskRepository, InstallationTaskRepository>();

        // Infrastructure Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddHttpClient<AgentApiClient>();
        services.AddScoped<IAgentApiClient>(provider => 
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(AgentApiClient));
            var logger = provider.GetRequiredService<ILogger<AgentApiClient>>();
            return new AgentApiClient(httpClient, logger);
        });

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Domain Services
        services.AddScoped<ITenantDomainService, TenantDomainService>();

        // Application Services
        services.AddScoped<ITenantApplicationService, TenantApplicationService>();
        services.AddScoped<IUserInvitationApplicationService, UserInvitationApplicationService>();
        services.AddScoped<IInstallationApplicationService, InstallationApplicationService>();

        return services;
    }
}
