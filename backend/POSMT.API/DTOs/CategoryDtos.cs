using System.ComponentModel.DataAnnotations;

namespace POSMT.API.DTOs;

public record CategoryDto(int CategoryId, string CategoryName);

public class CategoryRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string CategoryName { get; set; } = string.Empty;
}
