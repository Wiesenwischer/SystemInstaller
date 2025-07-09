using Microsoft.EntityFrameworkCore;
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
