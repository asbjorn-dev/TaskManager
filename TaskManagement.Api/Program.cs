using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using TaskManagement.Api.Data;
using TaskManagement.Api.Helpers;
using TaskManagement.Api.Infrastructure;
using TaskManagement.Api.Interfaces;
using TaskManagement.Api.Repositories;
using TaskManagement.Api.Services;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();

// db connection
builder.Services.AddSqlite<AppDbContext>(
    builder.Configuration.GetConnectionString("DbConnection")!
);

// register redis vha. StackExchange.Redis (open source library)
builder.Services.AddStackExchangeRedisCache(options =>
{
    // connection string til redis server 
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// controllers
builder.Services.AddControllers();

// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => // tilføjer auth knap på swagger
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});



// repositories 
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();

// services 
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddHostedService<DueSoonBackgroundService>(); // er singleton - backgroundservice for DueSoon tasks notifications

// RabbitMQ - Singleton grundet bekostninger(1 connection deles af alle requests, står i rabbit docs)
builder.Services.AddSingleton<RabbitMqConnectionFactory>();
builder.Services.AddSingleton<IEventPublisher, RabbitMqPublisher>();

// helpers
builder.Services.AddScoped<TaskServiceHelper>();

// jwt authentication - validere Issuer, Audience, Lifetime og IssuerSigningKey fra Identity service
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters  
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),

                // JWT middleware ved den skal kigge efter fulde URI som role-claim. 
                // Så matcher den hvad TokenService putter i token
                RoleClaimType = System.Security.Claims.ClaimTypes.Role 
        };
});

var app = builder.Build();

// migrate database on startup
app.MigrateDb();

// Seed kun i development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!context.Teams.Any())
    {
        DbSeeder.Seed(context);
    }
    
    // swagger in dev
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
