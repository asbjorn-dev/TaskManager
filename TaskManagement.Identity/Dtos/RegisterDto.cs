using System;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Identity.Dtos;

// håndter register request fra client
public class RegisterDto
{
    [Required]
    [MaxLength(100)]
    public required string Name {get; set;}

    [Required]
    [EmailAddress]
    public required string Email {get; set;}
    [Required]
    [MinLength(6)]
    public required string Password {get; set;}
}
