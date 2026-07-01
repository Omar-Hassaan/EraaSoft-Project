using System.ComponentModel.DataAnnotations;

namespace HealthCareManagementSystem.ViewModels
{
    public class DoctorVM
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Specialization { get; set; }

        [Required]
        public int ClinicId { get; set; }

        public IFormFile? Image { get; set; }
    }
}