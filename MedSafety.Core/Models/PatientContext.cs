namespace MedSafety.Core.Models;

public class PatientContext
{
    public int PatientId { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }

    public List<string> DrugAllergies { get; set; }
    public List<string> ClassAllergies { get; set; }
    public List<string> ActiveMeds { get; set; }

    public PatientContext (int patientid, string firstname, string lastname, List<string> drugallergies, List<string> classallergies,List<string> activemeds)
    {
        this.PatientId = patientid;
        this.Firstname = firstname;
        this.Lastname = lastname;
        this.DrugAllergies = drugallergies;
        this.ClassAllergies = classallergies;
        this.ActiveMeds = activemeds;
    }

}