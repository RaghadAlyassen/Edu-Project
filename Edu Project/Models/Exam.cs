using System.ComponentModel.DataAnnotations;

namespace Edu_Project.Models
{
    public class Exam
    {
       [Key]public int Id { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public int TotalMarks { get; set; }
        public Instructor Instructor { get; set; }
        public string InstructorId { get; set; }

    }
}
