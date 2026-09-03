using System.ComponentModel.DataAnnotations;

namespace Edu_Project.Models
{
    public class Course
    {
        [Key] public int Id { get; set; }
        public string Title { get; set; }
        public string Descciption { get; set; }
        public int Price { get; set; }
        public Instructor? Instructor { get; set; }
        public string? InstructorId { get; set; }
        public Category? Category { get; set; }
        public int? CategoryId { get; set; }
        public ICollection<Lesson>? Lessons { get; set; }
        public FinalExam? Finalexam { get; set; }
        public ICollection<Enrollment>? Enrollments { get; set; }








    }
}
