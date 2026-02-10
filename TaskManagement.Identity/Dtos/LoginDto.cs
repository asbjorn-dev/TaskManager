using System;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Identity.Dtos;

// håndter login request fra client
public class LoginDto
{
    [Required]
    [EmailAddress]
    public required string Email {get; set;}
    [Required]
    public required string Password {get; set;}
}
