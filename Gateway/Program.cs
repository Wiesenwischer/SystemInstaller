namespace SystemInstaller.Gateway;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services
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

        // Health check endpoint
        app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow })
           .WithName("HealthCheck");

        // Gateway info endpoint
        app.MapGet("/gateway/info", () => new 
        { 
            Gateway = "SystemInstaller API Gateway",
            Version = "1.0.0",
            Timestamp = DateTime.UtcNow,
            Environment = app.Environment.EnvironmentName
        })
        .WithName("GatewayInfo");

        // Map the reverse proxy
        app.MapReverseProxy();

        app.Run();
    }
}
