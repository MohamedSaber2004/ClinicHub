using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using ClinicHub.API.Filters;
using ClinicHub.API.Services;
using ClinicHub.Application;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.HealthCheck;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure;
using ClinicHub.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Scalar.AspNetCore;
using Serilog;
using System.Globalization;
using System.Reflection;
using ClinicHub.Application.Common.Models;
using ClinicHub.Persistence.Seeders;
using AspNetCoreRateLimit;
using ClinicHub.API.Transformers;
using ClinicHub.API.Middleware;

namespace ClinicHub.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);
                var env = builder.Environment;

                builder.Configuration.Sources.Clear();
                builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                JsonLocalizationProvider.Initialize(env.ContentRootPath);

                if (env.IsDevelopment() || env.EnvironmentName == "Test")
                {
                    var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                    if (appAssembly != null) builder.Configuration.AddUserSecrets(appAssembly, optional: true);
                }

                builder.Configuration.AddEnvironmentVariables().AddCommandLine(args);
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builder.Configuration)
                    .CreateBootstrapLogger();

                Log.Information("ClinicHub API is starting up and connecting to Seq at {Time}", DateTime.Now);

                builder.Host.UseSerilog();

                builder.Services.AddControllers(options =>
                {
                    options.Filters.Add<ApiExceptionFilterAttribute>();
                    options.MaxModelValidationErrors = 50;
                });

                builder.Services.AddApiVersioning(options =>
                {
                    options.DefaultApiVersion = new ApiVersion(2, 0);
                    options.AssumeDefaultVersionWhenUnspecified = false;
                    options.ReportApiVersions = true;
                    options.ApiVersionReader = new UrlSegmentApiVersionReader();
                })
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                });

                // Register OpenAPI documents for all known versions without premature BuildServiceProvider
                // Versions are discovered post-build from IApiVersionDescriptionProvider
                builder.Services.AddOpenApi("v1", options =>
                {
                    options.AddOperationTransformer<LanguageHeaderOperationTransformer>();
                });
                builder.Services.AddOpenApi("v2", options =>
                {
                    options.AddOperationTransformer<LanguageHeaderOperationTransformer>();
                });

                builder.Services.AddApplicationServices(builder.Configuration);
                builder.Services.AddPersistenceServices(builder.Configuration);
                builder.Services.AddInfrastructureServices(builder.Configuration);

                // Rate limiting configuration
                builder.Services.AddMemoryCache();

                builder.Services.AddInMemoryRateLimiting();

                builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
                builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
                builder.Services.Configure<SeedingSettings>(builder.Configuration.GetSection("SeedingSettings"));
                builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));

                builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
                builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

                builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
                builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddLocalization();
                builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
                builder.Services.Configure<RequestLocalizationOptions>(options =>
                {
                    var supportedCultures = new[] { new CultureInfo("ar"), new CultureInfo("en") };
                    options.DefaultRequestCulture = new RequestCulture("ar");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;

                    options.RequestCultureProviders = new List<IRequestCultureProvider>
                    {
                        new QueryStringRequestCultureProvider(),
                        new CookieRequestCultureProvider()
                    };
                });
                builder.Services.AddMvcCore().AddViewLocalization();
                builder.Services.AddHttpContextAccessor();
                builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
                builder.Services.AddHsts(options =>
                {
                    options.Preload = true;
                    options.IncludeSubDomains = true;
                    options.MaxAge = TimeSpan.FromDays(365);
                });
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy", cors => cors.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
                });

                var app = builder.Build();

                app.UseRequestLocalization();

                app.MapOpenApi("/openapi/{documentName}.json");

                var apiVersionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var description in apiVersionProvider.ApiVersionDescriptions)
                {
                    var name = description.GroupName;

                    app.MapScalarApiReference($"/scalar/{name}", options =>
                    {
                        options.WithTitle($"ClinicHub API {name}")
                               .WithTheme(ScalarTheme.BluePlanet)
                               .WithOpenApiRoutePattern($"/openapi/{name}.json");
                    });
                }

                app.MapGet("/", (IApiVersionDescriptionProvider provider) =>
                {
                    var lastVersion = provider.ApiVersionDescriptions.Last().GroupName;
                    return Results.Redirect($"/scalar/{lastVersion}");
                }).ExcludeFromDescription();


                // Run seeding in the background after the app starts to avoid blocking IIS startup timeout
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // Small delay to allow the host to finish starting before seeding
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        using var scope = app.Services.CreateScope();
                        var services = scope.ServiceProvider;
                        await services.SeedRolesAsync();
                        await services.SeedDataAsync();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "An error occurred during the database seeding process.");
                    }
                });

                app.UseXContentTypeOptions();
                app.UseXXssProtection(options => options.EnabledWithBlockMode());
                app.UseXfo(options => options.Deny());
                app.UseReferrerPolicy(options => options.NoReferrerWhenDowngrade());

                app.UseIpRateLimiting();

                if (app.Environment.IsDevelopment()) app.UseHttpsRedirection();

                app.UseSerilogRequestLogging(options =>
                {
                    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                    {
                        diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress);
                        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"]);
                        
                        var user = httpContext.User.Identity?.Name;
                        if (!string.IsNullOrEmpty(user)) diagnosticContext.Set("User", user);
                    };
                });

                if (app.Environment.IsDevelopment() && app.Environment.IsEnvironment("Test"))
                {
                    app.UseRequestResponseLogging();
                }

                app.UseRouting();
                app.UseCors("CorsPolicy");
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseStaticFiles();


                app.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = HealthCheckResponseWriter.WriteResponse,
                    AllowCachingResponses = false
                });

                app.MapControllers();

                await app.RunAsync();
            }
            catch (Exception ex) when (ex is not HostAbortedException)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
