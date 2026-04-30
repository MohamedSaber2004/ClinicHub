using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Domain.Repositories.Interfaces.Base;
using ClinicHub.Infrastructure.Repositories.Implementations;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Infrastructure.Services;
using ClinicHub.Infrastructure.Services.Interfaces;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using ClinicHub.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ClinicHub.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, Repositories.Implementations.Base.UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IClinicRepository, ClinicRepository>();
            services.AddScoped<ISpecializationRepository, SpecializationRepository>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IFacebookAuth, FacebookAuth>();
            services.AddScoped<IGoogleAuth, GoogleAuth>();

            //services.AddHttpClient<IMapService, GoogleMapService>();
            services.AddHttpClient<IMapService, NominatimService>(client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", "ClinicHub-API");
            });

            var identityOptionsConfig = new IdentityModel();
            configuration.Bind("IdentityOptions", identityOptionsConfig);

            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireNonAlphanumeric = identityOptionsConfig.RequireNonAlphanumeric;
                options.Password.RequiredLength = identityOptionsConfig.RequiredLength;
                options.Password.RequireDigit = identityOptionsConfig.RequiredDigit;
                options.Password.RequireLowercase = identityOptionsConfig.RequireLowercase;
                options.Password.RequiredUniqueChars = identityOptionsConfig.RequiredUniqueChars;
                options.Password.RequireUppercase = identityOptionsConfig.RequireUppercase;
                options.Lockout.MaxFailedAccessAttempts = identityOptionsConfig.MaxFailedAttempts;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(identityOptionsConfig.LockoutTimeSpanInDays);
                options.SignIn.RequireConfirmedEmail = identityOptionsConfig.RequireConfirmedEmail;
                options.User.AllowedUserNameCharacters = identityOptionsConfig.AllowedUserNameCharacters;
                options.User.RequireUniqueEmail = identityOptionsConfig.RequireUniqueEmail;
            })
           .AddEntityFrameworkStores<ClinicHubContext>()
           .AddDefaultTokenProviders();

            var jwtSettings = new JwtSettings();
            configuration.Bind(nameof(JwtSettings), jwtSettings);
            services.AddSingleton(jwtSettings);

            services.Configure<FacebookAuthSettings>(configuration.GetSection(nameof(FacebookAuthSettings)));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                ValidateIssuer = false,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = false,
                ValidAudience = jwtSettings.Audience,
                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };
            services.AddSingleton(tokenValidationParameters);

            var facebookSection = configuration.GetSection("FacebookAuthSettings");
            var googleSection = configuration.GetSection("GoogleAuthSettings");

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddFacebook(options =>
            {
                options.AppId = facebookSection["AppId"];
                options.AppSecret = facebookSection["AppSecret"];
            })
            .AddGoogle(options =>
            {
                options.ClientId = googleSection["WebClientId"];
                options.ClientSecret = googleSection["WebClientSecret"];
                options.CallbackPath = "/signin-google";
                options.SaveTokens = true;
                options.AccessType = "offline";
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = tokenValidationParameters;
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var localizedMessage = JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ExceptionMessages.Unauthorized.Value);
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            succeeded = false,
                            message = localizedMessage,
                            errors = new Dictionary<string, string[]>(),
                            code = 401
                        });

                        return context.Response.WriteAsync(result);
                    }
                };
            });





            return services;
        }
    }
}
