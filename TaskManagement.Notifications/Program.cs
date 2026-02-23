using TaskManagement.Notifications;
using TaskManagement.Notifications.Handlers;
using TaskManagement.Notifications.Infrastructure;


var builder = Host.CreateApplicationBuilder(args);

// RabbitMQ Connection
builder.Services.AddSingleton<RabbitMqConnectionFactory>();

// registrer event handlers
builder.Services.AddSingleton<IEventHandler, TaskCreatedHandler>();
builder.Services.AddSingleton<IEventHandler, TaskDueSoonHandler>();

// worker service
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IEmailService, SmtpEmailService>();

var host = builder.Build();
host.Run();
