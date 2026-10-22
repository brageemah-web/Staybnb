using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace staybnb.ViewModels;

public class CheckInSetupViewModel
{
    public int PropertyId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = "";

    [Required]
    [Display(Name = "House rules and check-in instructions")]
    public string Rules { get; set; } = "";

    public List<string> DocumentTypes { get; set; } = new();
}

public class CheckInConfig{
    public string Rules { get; set; } = "";
    public List<string> DocumentTypes { get; set; } = new();

    public static CheckInConfig Parse(string? json){
        if (string.IsNullOrWhiteSpace(json)){
            return new CheckInConfig();
        }

        try { 
            return JsonSerializer.Deserialize<CheckInConfig>(json) ?? new CheckInConfig();
        }
        catch (JsonException) { 
            return new CheckInConfig(); 
        }
    }
}