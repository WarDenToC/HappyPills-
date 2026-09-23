using MedSafety.Core.Enum;
using MedSafety.Core.Models;
using MedSafety.Core.Models.FormularyTable;

namespace MedSafety.Core.Rules;

public class AllergyRule : INterfacePrescriptionRule
{
   public RuleType Type => RuleType.Allergies;
   public IEnumerable<Alert> Check(Prescription prescription, PatientContext patientContext, Formulary formulary)
   {
      List<Alert> alerts = new List<Alert>();
      var entry = formulary.FindEntry(prescription.DrugName);
      
      if (entry == null)
         return alerts;

      if (patientContext.DrugAllergies.Contains(entry.DrugName))
      {
         Alert drugAllergies = new Alert(Severity.Critical, Type,
            $"Patient has an allergy to the drug named {entry.DrugName}");
         alerts.Add(drugAllergies);
      }

      if(patientContext.ClassAllergies.Contains(entry.DrugClass.ToString()))
      {
         Alert classAllergies = new Alert(Severity.Critical, Type,
            $"Patient has an allergy to the class of drug: {entry.DrugClass}");
         alerts.Add(classAllergies);
      }
      
      return alerts;
   }
   

}

/*
Allergy — Critical alert if the drug's name or its therapeutic class appears in the patient's allergy list.
Match case-insensitively, and match on class as well as name — a patient allergic to a class must be flagged for any drug in it.
*/