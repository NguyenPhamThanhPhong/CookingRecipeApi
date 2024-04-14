using CookingRecipeApi;
using CookingRecipeApi.Configs;
using CookingRecipeApi.Hubs;
using CookingRecipeApi.Services.RabbitMQServices;
using Microsoft.IdentityModel.Logging;
using System;
using System.Net;
using System.Net.Sockets;
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


string? ipv4Address = (Dns.GetHostAddresses(Dns.GetHostName()).Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault())?.ToString();

Console.WriteLine($"Server is running on https://{ipv4Address}:{7000}/swagger/index.html");
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
