ENSE707: HappyPills - This project is desgined and developed by Amrith Kong and Naz Janif. The purpose of this app is to make a prescription checker that's aimed to fix and alert mistakes.

MedSafety is a web application prototype for checking prescription safety and supporting pharmacist review. Prescribers can enter a prescription and run safety checks to surface potential issues before submission. Pharmacists can review submitted prescriptions, inspect alerts, and approve, reject, or override a flagged prescription with a reason. Prescription history, an audit log, and a quality dashboard provide views of decisions and application activity.

The application uses synthetic data for demonstration and coursework purposes only. It is not a clinical system and must not be used to make real medical decisions.

## Run the web application

### Requirements

- .NET 8 SDK

### Start

From the repository root, run:

```bash
dotnet run --project MedSafety.Web
```

Open the local address printed in the terminal. With the included HTTP launch profile, this is typically [http://localhost:5171](http://localhost:5171). On first startup, the application creates `MedSafety.Web/medsafety.db` and seeds it with fictional sample data.

### Sign in

The login page lets you select a role. The username is filled in automatically; use the shared demo password `password`.

| Role | Username |
| --- | --- |
| Prescriber | `prescriber` |
| Pharmacist | `pharmacist` |
| Administrator | `admin` |

## Navigate the application

After signing in, use the navigation menu:

- **Home** — Overview of the application and links to its main areas.
- **New prescription** — As a prescriber, select a sample patient and enter the prescription details. Choose **Check prescription** to see safety alerts, then submit the prescription for pharmacist review.
- **Review queue** — As a pharmacist, review submitted prescriptions and their alerts. Approve or reject each prescription; a reason is required for rejection, and critical alerts require a reason to override and approve.
- **History** — As a pharmacist, search and filter prescription records by patient, status, alert severity, and date.
- **Audit log** — As an administrator, review recorded sign-in, prescription, and access-related events. Filter by action or refresh the log.
- **Dashboard** — As an administrator, view prescription, alert, review, override, and rejection statistics.

Pages enforce role-based access. If a role cannot access a page, the application displays an access-denied message. Use the user menu to sign out.
