using System;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Identity.Data;
using TaskManagement.Identity.Dtos;
using TaskManagement.Identity.Interfaces;
using TaskManagement.Identity.Models;
using TaskManagement.Shared.Events;

namespace TaskManagement.Identity.Services;

public class AuthService : IAuthService
{
    private readonly IdentityDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IEventPublisher _eventPublisher;
    public AuthService(IdentityDbContext context, ITokenService tokenService, IEventPublisher eventPublisher)
    {
        _context = context;
        _tokenService = tokenService;
        _eventPublisher = eventPublisher;
    }


    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        // check email er i brug
        var emailExist = await _context.Users
            .AnyAsync(u => u.Email == dto.Email);

        if (emailExist)
            return null;
        
        // opretter ny User objekt med hashed password
        var user = new User
        {
            id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password) // tilføjer unikt salt
        };

        // microsoft dokumentation siger bruge synkront Add over AddAsync (vi ændre kun noget i memory)
        _context.Users.Add(user);
        await _context.SaveChangesAsync(); // her rører vi db og skal være asynkront

        // publish ny user event til rabbit (til sync user til core api)
        await _eventPublisher.PublishAsync(new UserRegisteredEvent
        {
            UserId = user.id,
            Name = user.Name,
            Email = user.Email
        }, "user.registered");

        // opret response dto med brugerinfo + ny token
        return new AuthResponseDto
        {
            UserId = user.id,
            Name = user.Name,
            Email = user.Email,
            Token = _tokenService.GenerateToken(user)
        };
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        // bruger findes ikke eller wrong password
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) 
            return null;


        return new AuthResponseDto
        {
            UserId = user.id,
            Name = user.Name,
            Email = user.Email,
            Token = _tokenService.GenerateToken(user)
        };
    }
}
