using CookingRecipeApi.Configs;
using CookingRecipeApi.Repositories.Interfaces;
using CookingRecipeApi.Repositories.Repos;
using CookingRecipeApi.Services.AuthenticationServices;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using CookingRecipeApi.Services.BusinessServices.Services;
using CookingRecipeApi.Services.RabbitMQServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

namespace CookingRecipeApi
{
    public static class Startup
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddSignalR();
            services.ConfigDbContext(config);
            services.ConfigAuthentication(config);
            // must have services & repositories to config rabbitMQ
            services.ConfigServicesRepositories();
            // rabbitMQ here
            services.ConfigRabbitMQ(config);
            services.ConfigCORS(config);

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
            services.AddSingleton<TokenGenerator>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = authenticationConfigs.Issuer,
                        ValidAudience = authenticationConfigs.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationConfigs.AccessTokenSecret))
                    };
                });
            services.AddAuthorization();
            return services;
        }
        public static IServiceCollection ConfigRabbitMQ(this IServiceCollection services, IConfiguration config)
        {
            MessageQueueConfigs rabbitMQConfigs = new MessageQueueConfigs();
            config.GetSection("RabbitMQConfigs").Bind(rabbitMQConfigs);
            Console.WriteLine(JsonSerializer.Serialize(rabbitMQConfigs));
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
            services.AddSingleton<INotificationService, NotificationService>();
            return services;
        }
        public static IServiceCollection ConfigCORS(this IServiceCollection services, IConfiguration config)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });
            return services;
        }
        public static IServiceCollection ConfigAzureBlob(this IServiceCollection services, IConfiguration config)
        {
            AzureBlobConfigs azureBlobConfigs = new AzureBlobConfigs();
            config.GetSection("AzureBlobConfigs").Bind(azureBlobConfigs);
            services.AddSingleton(azureBlobConfigs);
            return services;
        }
        public static IServiceCollection ConfigSMTP(this IServiceCollection services, IConfiguration config)
        {
            SMTPConfigs smtpConfigs = new SMTPConfigs();
            config.Bind("SMTPConfiguration", smtpConfigs);
            services.AddSingleton(smtpConfigs);
            return services;
        }
    }
}
