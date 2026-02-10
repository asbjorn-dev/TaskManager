using System;
using TaskManagement.Identity.Models;

namespace TaskManagement.Identity.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
