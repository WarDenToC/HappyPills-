using MedSafety.Core.Security;

namespace MedSafety.Web.Services;

// Prototype stand-in for login: the role the demo user is currently acting as.
// Scoped, so each browser session has its own role.
public class CurrentUser
{
    public Role Role { get; private set; } = Role.Prescriber;

    public string Name => Role switch
    {
        Role.Prescriber => "Dr Prescriber",
        Role.Pharmacist => "Pat Pharmacist",
        Role.Administrator => "Alex Admin",
        _ => "Unknown"
    };

    public event Action? Changed;

    public void SwitchTo(Role role)
    {
        if (role == Role) return;
        Role = role;
        Changed?.Invoke();
    }
}