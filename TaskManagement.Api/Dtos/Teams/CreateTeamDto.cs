using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.Dtos.Teams
{
    // DTOs er API kontraker, imens models er databasemodeller
    public class CreateTeamDto
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }
    }

}
