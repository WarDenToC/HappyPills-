using MedSafety.Core.Security;

namespace MedSafety.Web.Services;

// The signed-in user for this browser session (scoped per Blazor circuit).
public class CurrentUser
{
    public bool IsAuthenticated { get; private set; }

    // Only meaningful while IsAuthenticated is true
    public Role Role { get; private set; } = Role.Prescriber;

    public string Name => Role switch
    {
        Role.Prescriber => "Dr Prescriber",
        Role.Pharmacist => "Pat Pharmacist",
        Role.Administrator => "Alex Admin",
        _ => "Unknown"
    };

    public event Action? Changed;

    public void SignIn(Role role)
    {
        Role = role;
        IsAuthenticated = true;
        Changed?.Invoke();
    }

    public void SignOut()
    {
        IsAuthenticated = false;
        Changed?.Invoke();
    }
}