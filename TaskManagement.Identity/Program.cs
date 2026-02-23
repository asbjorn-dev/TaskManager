using Microsoft.EntityFrameworkCore;
using TaskManagement.Identity.Data;
using TaskManagement.Identity.Infrastructure;
using TaskManagement.Identity.Interfaces;
using TaskManagement.Identity.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// db
builder.Services.AddSqlite<IdentityDbContext>(
    builder.Configuration.GetConnectionString("IdentityDb")!);

// services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// rabbitmq - singleton best practice i rabbit docs (en instans deles i hele app's levetid)
builder.Services.AddSingleton<RabbitMqConnectionFactory>();
builder.Services.AddSingleton<IEventPublisher, RabbitMqPublisher>();

var app = builder.Build();

// auto migration + seed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    context.Database.Migrate();

    // seed admin bruger i development
    if (app.Environment.IsDevelopment())
        IdentitySeeder.SeedAdmin(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
