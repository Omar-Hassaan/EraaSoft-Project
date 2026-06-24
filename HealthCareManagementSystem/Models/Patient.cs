namespace HealthCareManagementSystem.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public string? Description { get; set; }
        public DateOnly DateOfBirth { get; set; }
        
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<PatientDoctor> PatientDoctors { get; set; }
        public ICollection<ClinicPatient> ClinicPatients { get; set; }
        public ICollection<MedicalFile> MedicalFiles { get; set; }
    }
}
