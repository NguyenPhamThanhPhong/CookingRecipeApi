using CookingRecipeApi;
using CookingRecipeApi.Configs;
using CookingRecipeApi.Hubs;
using CookingRecipeApi.Services.RabbitMQServices;
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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseRouting();

app.UseAuthentication();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");

app.Run();
