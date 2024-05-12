using CookingRecipeApi.Configs;
using CookingRecipeApi.Helper;
using CookingRecipeApi.Repositories.Interfaces;
using CookingRecipeApi.Repositories.Repos;
using CookingRecipeApi.Services.AuthenticationServices;
using CookingRecipeApi.Services.AzureBlobServices;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using CookingRecipeApi.Services.BusinessServices.Services;
using CookingRecipeApi.Services.RabbitMQServices;
using CookingRecipeApi.Services.SMTPServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using System.Text.Json;

namespace CookingRecipeApi
{
    public static class Startup
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration config)
        {
            services.ConfigControllers();
            services.AddEndpointsApiExplorer();
            services.ConfigSwagger(config);
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddSignalR();
            services.ConfigDbContext(config);
            services.ConfigAuthentication(config);
            // must have services & repositories to config rabbitMQ
            services.ConfigServicesRepositories();
            // rabbitMQ here
            services.ConfigRabbitMQ(config);
            services.ConfigCORS(config);

            services.ConfigAzureBlob(config);
            services.ConfigSMTP(config);

        }
        public static IServiceCollection ConfigDbContext(this IServiceCollection services, IConfiguration config)
        {
            //database here
            DatabaseConfigs databaseConfigs = new DatabaseConfigs();
            config.GetSection("DatabaseConfigs").Bind(databaseConfigs);
            databaseConfigs.Initialize();
            //notification page limit
            ClientConstants clientConstants = new ClientConstants();
            config.GetSection("ClientConstants").Bind(clientConstants);
            services.AddSingleton(databaseConfigs);
            services.AddSingleton(clientConstants);
            return services;
        }
        public static IServiceCollection ConfigAuthentication(this IServiceCollection services, IConfiguration config)
        {
            AuthenticationConfigs authenticationConfigs = new AuthenticationConfigs();
            config.GetSection("AuthenticationConfigs").Bind(authenticationConfigs);
            services.AddSingleton(authenticationConfigs);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                // help config to know the KID of the token
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = authenticationConfigs.Issuer,
                    ValidAudience = authenticationConfigs.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationConfigs.AccessTokenSecret))
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        return Task.CompletedTask;
                    },
                };
            });
            services.AddSingleton<TokenGenerator>();
            services.AddAuthorization();
            return services;
        }
        public static IServiceCollection ConfigRabbitMQ(this IServiceCollection services, IConfiguration config)
        {
            MessageQueueConfigs rabbitMQConfigs = new MessageQueueConfigs();
            config.GetSection("RabbitMQConfigs").Bind(rabbitMQConfigs);
            services.AddSingleton(rabbitMQConfigs);
            services.AddSingleton<NotificationTaskProducer>();
            services.AddSingleton<NotificationTaskConsumer>();
            rabbitMQConfigs.Initialize();
            // try to trigger method Initialize of NotificationTaskConsumer
            return services;
        }
        public static IServiceCollection ConfigServicesRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<INotificationBatchRepository, NotificationBatchRepository>();
            services.AddSingleton<IRecipeRepository, RecipeRepository>();

            services.AddSingleton<ILoginService, LoginService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IRecipeService, RecipeService>();
            services.AddSingleton<INotificationService, NotificationService>();
            return services;
        }
        public static IServiceCollection ConfigCORS(this IServiceCollection services, IConfiguration config)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyMethod()
                            .AllowAnyHeader()
                            .SetIsOriginAllowed(host => true) // allow any origin
                            .AllowCredentials();
                });
            });
            return services;
        }
        public static IServiceCollection ConfigAzureBlob(this IServiceCollection services, IConfiguration config)
        {
            AzureBlobConfigs azureBlobConfigs = new AzureBlobConfigs();
            config.GetSection("AzureBlobConfigs").Bind(azureBlobConfigs);
            azureBlobConfigs.Initialize();
            services.AddSingleton(azureBlobConfigs);
            services.AddSingleton<AzureBlobHandler>();
            return services;
        }
        public static IServiceCollection ConfigSMTP(this IServiceCollection services, IConfiguration config)
        {
            SMTPConfigs smtpConfigs = new SMTPConfigs();
            config.Bind("SMTPConfiguration", smtpConfigs);
            services.AddSingleton(smtpConfigs);
            services.AddSingleton<EmailService>();
            return services;
        }
        public static IServiceCollection ConfigSwagger(this IServiceCollection services, IConfiguration config)
        {
            // config so that be able to custom header
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

                // Add JWT authentication in Swagger UI
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                });

                c.OperationFilter<SecurityRequirementsOperationFilter>();
                
            });
            return services;
        }
        public static IServiceCollection ConfigControllers(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.InputFormatters.Insert(0, new PlainTextConverter());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new TimeSpanConverter());
            });

            // add model binder
            services.AddMvc(options =>
            {
                options.ModelBinderProviders.Insert(0, new TimeSpanFormBinderProvider());
            });

            return services;
        }
    }
}