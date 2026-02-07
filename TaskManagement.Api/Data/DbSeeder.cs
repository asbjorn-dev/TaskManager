using TaskManagement.Api.Models;
using TaskManagement.Api.Models.Enums;

namespace TaskManagement.Api.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // Teams først (har ingen dependencies)
            var team1 = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Platform Team",
                CreatedAt = DateTime.UtcNow
            };

            var team2 = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Frontend Team",
                CreatedAt = DateTime.UtcNow
            };

            context.Teams.AddRange(team1, team2);
            context.SaveChanges();


            // 2Users (har ingen dependencies)
            var user1 = new User
            {
                Id = Guid.NewGuid(),
                Name = "Alice",
                Email = "alice@example.com"
            };

            var user2 = new User
            {
                Id = Guid.NewGuid(),
                Name = "Bob",
                Email = "bob@example.com"
            };

            context.Users.AddRange(user1, user2);
            context.SaveChanges();


            // Projects (skal have et Teams)
            var project1 = new Project
            {
                Id = Guid.NewGuid(),
                TeamId = team1.Id, // foreign key
                Name = "Build Task API",
                Description = "Core backend for task management",
                Status = ProjectStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            var project2 = new Project
            {
                Id = Guid.NewGuid(),
                TeamId = team1.Id,
                Name = "Add Authentication",
                Description = null,
                Status = ProjectStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            var project3 = new Project
            {
                Id = Guid.NewGuid(),
                TeamId = team2.Id,
                Name = "Build UI Components",
                Status = ProjectStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            context.Projects.AddRange(project1, project2, project3);
            context.SaveChanges();


            // Tags (har ingen dependencies)
            var tag1 = new Tag
            {
                Id = Guid.NewGuid(),
                Name = "backend"
            };

            var tag2 = new Tag
            {
                Id = Guid.NewGuid(),
                Name = "urgent"
            };

            var tag3 = new Tag
            {
                Id = Guid.NewGuid(),
                Name = "bug"
            };

            context.Tags.AddRange(tag1, tag2, tag3);
            context.SaveChanges();


            // TaskItems (kræver Projects + Users)
            var task1 = new TaskItem
            {
                Id = Guid.NewGuid(),
                ProjectId = project1.Id, // foreign key
                Title = "Setup database models",
                Description = "Create EF Core models and migrations",
                State = TaskState.Done,
                Priority = TaskPriority.High,
                AssignedUserId = user1.Id, // foreign key
                DueDate = DateTime.UtcNow.AddDays(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            };

            var task2 = new TaskItem
            {
                Id = Guid.NewGuid(),
                ProjectId = project1.Id,
                Title = "Implement CRUD endpoints",
                Description = "Add controllers and services",
                State = TaskState.InProgress,
                Priority = TaskPriority.High,
                AssignedUserId = user2.Id,
                DueDate = DateTime.UtcNow.AddDays(3),
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            };

            var task3 = new TaskItem
            {
                Id = Guid.NewGuid(),
                ProjectId = project1.Id,
                Title = "Write unit tests",
                State = TaskState.Todo,
                Priority = TaskPriority.Medium,
                AssignedUserId = null, // ← ikke assigned
                DueDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var task4 = new TaskItem
            {
                Id = Guid.NewGuid(),
                ProjectId = project2.Id,
                Title = "Setup JWT authentication",
                State = TaskState.Todo,
                Priority = TaskPriority.High,
                AssignedUserId = user1.Id,
                DueDate = DateTime.UtcNow.AddDays(5),
                CreatedAt = DateTime.UtcNow
            };

            context.TaskItems.AddRange(task1, task2, task3, task4);
            context.SaveChanges();


            // TaskTags (kræver TaskItems + Tags)
            var taskTag1 = new TaskTag
            {
                TaskItemId = task1.Id,
                TagId = tag1.Id // backend
            };

            var taskTag2 = new TaskTag
            {
                TaskItemId = task2.Id,
                TagId = tag1.Id // backend
            };

            var taskTag3 = new TaskTag
            {
                TaskItemId = task2.Id,
                TagId = tag2.Id // urgent
            };

            var taskTag4 = new TaskTag
            {
                TaskItemId = task4.Id,
                TagId = tag1.Id // backend
            };

            var taskTag5 = new TaskTag
            {
                TaskItemId = task4.Id,
                TagId = tag2.Id // urgent
            };

            context.TaskTags.AddRange(taskTag1, taskTag2, taskTag3, taskTag4, taskTag5);
            context.SaveChanges();
        }
    }
}