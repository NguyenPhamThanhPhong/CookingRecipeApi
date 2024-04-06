using CookingRecipeApi;
using CookingRecipeApi.Configs;
using CookingRecipeApi.Hubs;
using CookingRecipeApi.Services.RabbitMQServices;
using Microsoft.IdentityModel.Logging;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.ConfigureServices(builder.Configuration);
//builder.Services.AddOutputCache();

var app = builder.Build();

// SETUP RABBITMQ consumer
app.Services.GetRequiredService<NotificationTaskConsumer>().SetupConsumer();

// handle rabbitMQ service when application stop
app.Lifetime.ApplicationStopping.Register(() =>
{
    var rabbitMQService = app.Services.GetRequiredService<MessageQueueConfigs>();
    rabbitMQService.Dispose();
});

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");

app.Run();
