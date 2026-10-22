using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using staybnb.Models;


namespace staybnb.ViewModels;

public class HostApplicationViewModel
{
    [Required, StringLength(100)]
    public string Title { get; set; } = "";

    [Required, StringLength(2000)]
    public string Description { get; set; } = "";

    [Required]
    public string Address { get; set; } = "";

    [Required, StringLength(100)]
    public string City { get; set; } = "";

    [Display(Name = "Property type")]
    public PropertyType PropertyType { get; set; }

    [Range(1, 1000000), Display(Name = "Price per night (R)")]
    public decimal PricePerNight { get; set; }

    [Range(0, 1000000), Display(Name = "Cleaning fee (R)")]
    public decimal CleaningFee { get; set; }

    [Range(1, 50), Display(Name = "Max guests")]
    public int MaxGuests { get; set; } = 1;

    [Range(0, 50)] public int Bedrooms { get; set; }
    [Range(0, 50)] public int Bathrooms { get; set; }

    public List<int> SelectedAmenityIds { get; set; } = new();

    // At least one image: checked in the controller
    public List<IFormFile> Images { get; set; } = new();

    [ValidateNever]
    public List<Amenity> AvailableAmenities { get; set; } = new();
}