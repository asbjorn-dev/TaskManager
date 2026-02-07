using Microsoft.EntityFrameworkCore;

namespace TaskManagement.Api.Data
{
    public static class DataExtension
    {
        // will automatically apply any pending migrations to db when the application starts
        public static void MigrateDb(this WebApplication app)
        {
            // Scope to get access to the AppDbContext 
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            dbContext.Database.Migrate();
        }
    }
}
