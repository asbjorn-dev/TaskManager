using System;

namespace TaskManagement.Identity.Dtos;

// server returner denne dto med en jwt token til client
public class AuthResponseDto
{
    public Guid UserId {get; set;}
    public required string Name {get; set;}
    public required string Email {get; set;}
    public required string Token {get; set;}
}
