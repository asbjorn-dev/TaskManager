using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Identity.Models;

namespace TaskManagement.Identity.Data;

public class IdentityDbContext : DbContext
{
    // DI til db config (hvilken db, connection string osv.).
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
}
