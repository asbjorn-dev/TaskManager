using System;
using TaskManagement.Identity.Dtos;

namespace TaskManagement.Identity.Interfaces;

public interface IAuthService
{
    // response er nullable pga. de kan fejle (email allerede i brug eller password forkert)
    // gør det muligt at validate i controller og return null og gør Unauthorized();
    Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
}
