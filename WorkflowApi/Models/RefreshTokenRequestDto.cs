using System.ComponentModel.DataAnnotations;

namespace WorkflowApi.Models;

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}