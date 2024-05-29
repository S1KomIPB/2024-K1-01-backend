using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Models
{
    public class Schedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("User")]
        [Required]
        public int? TeacherId { get; set; }

        [ForeignKey("CourseClass")]
        [Required]
        public int CourseClassId { get; set; }

        [Required]
        public int MeetNumber { get; set; }

        public required CourseClass CourseClass { get; set; }
        public required User User { get; set; }
    }
}