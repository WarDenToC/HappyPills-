using MedSafety.Core.Enum;
using MedSafety.Data.Rows;

namespace MedSafety.Data;

public static class DatabaseSeeder
{
    // Creates the database if missing, and adds the synthetic data only once
    public static void EnsureSeeded(MedSafetyDbContext db)
    {
        db.Database.EnsureCreated();
        if (db.Drugs.Any())
            return;

        db.Drugs.AddRange(
            new DrugRow { Id = 1, DrugName = "Alphacillin", DrugClass = DrugClass.Penicilin, DoseUnit = DoseUnit.Mg, MinDose = 250, MaxDose = 500 },
            new DrugRow { Id = 2, DrugName = "Betanol", DrugClass = DrugClass.BetaBlocker, DoseUnit = DoseUnit.Mg, MinDose = 25, MaxDose = 100 },
            new DrugRow { Id = 3, DrugName = "Cardizyme", DrugClass = DrugClass.BetaBlocker, DoseUnit = DoseUnit.Mg, MinDose = 5, MaxDose = 20 },
            new DrugRow { Id = 4, DrugName = "Dolorex", DrugClass = DrugClass.NSAID, DoseUnit = DoseUnit.Mg, MinDose = 500, MaxDose = 1000 });

        db.Interactions.Add(
            new InteractionRow { Id = 1, DrugA = "Dolorex", DrugB = "Cardizyme", Severity = Severity.Critical });

        db.Patients.AddRange(
            new PatientRow { Id = 1, Firstname = "Kai", Lastname = "Ngata", DrugAllergies = { "alphacillin" } },
            new PatientRow { Id = 2, Firstname = "Riya", Lastname = "Sharma", ActiveMeds = { "Cardizyme" } },
            new PatientRow { Id = 3, Firstname = "Noah", Lastname = "Fletcher", ActiveMeds = { "Betanol" } });

        db.SaveChanges();
    }
}