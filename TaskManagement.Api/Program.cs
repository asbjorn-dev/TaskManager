using TaskManagement.Api.Data;
using TaskManagement.Api.Helpers;
using TaskManagement.Api.Interfaces;
using TaskManagement.Api.Repositories;
using TaskManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();

// db connection
builder.Services.AddSqlite<AppDbContext>(
    builder.Configuration.GetConnectionString("DbConnection")!
);

// controllers
builder.Services.AddControllers();

// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// repositories 
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();

// services 
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITeamService, TeamService>();

// helpers
builder.Services.AddScoped<TaskServiceHelper>();

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

app.MapControllers();

app.Run();
