using System.ComponentModel.DataAnnotations;

namespace POSMT.API.DTOs;

// Terminal list and detail response
public record TerminalDto(
    int TerminalId,
    string TerminalName,
    string Status
);

// Create terminal request
public class CreateTerminalRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string TerminalName { get; set; } = string.Empty;
}

// Update terminal request
public class UpdateTerminalRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string TerminalName { get; set; } = string.Empty;
}

// Update terminal status request
public class UpdateTerminalStatusRequest
{
    [Required]
    [RegularExpression("^(Active|Inactive)$")]
    public string Status { get; set; } = string.Empty;
}