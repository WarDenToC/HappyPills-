using System.ComponentModel.DataAnnotations;

namespace MedSafety.Web.Models;

// FR1/FR2: the 7 mandatory fields, validated client-side before submission
public class PrescriptionFormModel
{
    [Required(ErrorMessage = "Drug is required")]
    public string DrugName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Dose amount is required")]
    public string DoseAmount { get; set; } = string.Empty;

    [Required(ErrorMessage = "Dose unit is required")]
    public string DoseUnit { get; set; } = "mg";

    [Required(ErrorMessage = "Frequency is required")]
    public string Frequency { get; set; } = string.Empty;

    [Required(ErrorMessage = "Route is required")]
    public string Route { get; set; } = string.Empty;

    [Required(ErrorMessage = "Duration is required")]
    public string Duration { get; set; } = string.Empty;

    [Required(ErrorMessage = "Patient's name is required")]
    public string PatientName { get; set; } = string.Empty;
}