using ClinicHub.Application.Common.Behaviours;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using ClinicHub.Application.HealthCheck;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ClinicHub.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Resolve a safe drive path that works on any server (Windows or Linux)
            var drivePath = Path.GetPathRoot(AppContext.BaseDirectory) ?? @"C:\";
            var connectionString = configuration.GetConnectionString("CareClinicHubDb") ?? string.Empty;

            services.AddSingleton(new DiskSpaceHealthCheck(drivePath, 512));
            services.AddSingleton(new DatabaseHealthCheck(connectionString));

            services.AddHealthChecks()
                .AddCheck<CustomHealthCheck>("API Custom Checks")
                .AddCheck<DiskSpaceHealthCheck>("Disk Space", tags: new[] { "ready" })
                .AddCheck<DatabaseHealthCheck>("Database", tags: new[] { "ready" });

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));

            services.AddValidatorsFromAssemblies(assemblies: new[] { Assembly.GetExecutingAssembly() });

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

            services.AddAutoMapper(configAction: (action) => { }, Assembly.GetExecutingAssembly());

            services.Configure<GoogleMapsSettings>(configuration.GetSection("GoogleMaps"));

            services.Configure<OpenStreetMapsSettings>(configuration.GetSection("OpenStreetMaps"));

            services.Configure<FacebookAuthSettings>(configuration.GetSection("FacebookAuthSettings"));

            services.Configure<GoogleAuthSettings>(configuration.GetSection("GoogleAuthSettings"));

            return services;
        }
    }
}
