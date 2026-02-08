using System;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.Dtos.Teams;

public class UpdateTeamDto
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
}
