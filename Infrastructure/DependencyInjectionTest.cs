using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SystemInstaller.Infrastructure.Data;

namespace SystemInstaller.Infrastructure;

public static class DependencyInjectionTest
{
    public static IServiceCollection AddInfrastructureTest(this IServiceCollection services, IConfiguration configuration)
    {
        // Empty test
        return services;
    }
}
