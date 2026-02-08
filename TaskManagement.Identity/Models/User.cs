using System;
using TaskManagement.Identity.Models.Enums;

namespace TaskManagement.Identity.Models;

public class User
{
    public Guid id {get; set; }
    public required string Name {get; set; }
    public required string Email {get; set; }
    public required string PasswordHash {get; set; }
    public UserRole Role {get; set; } = UserRole.Member;
    public DateTime CreatedAt {get; set; } = DateTime.UtcNow;
}
