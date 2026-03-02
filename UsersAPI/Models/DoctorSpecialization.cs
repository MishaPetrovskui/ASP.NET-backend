using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UsersAPI.Models
{
    public class DoctorSpecialization
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public int SpecializationId { get; set; }
    }
}
