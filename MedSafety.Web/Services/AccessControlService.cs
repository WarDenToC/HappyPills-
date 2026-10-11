namespace MedSafety.Web.Services;

public enum UserRole { Prescriber, Pharmacist, Admin }

public static class AccessControlService
{
    public static bool CanPerform(UserRole role, string action) => action switch
    {
        "Prescribe" => role == UserRole.Prescriber,
        "ReviewPrescription" => role == UserRole.Pharmacist,
        "ViewDashboard" => role == UserRole.Admin,
        _ => false
    };
}