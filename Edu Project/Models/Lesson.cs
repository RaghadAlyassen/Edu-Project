using System.ComponentModel.DataAnnotations;

namespace Edu_Project.Models
{
    public class Lesson
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }

        public int Order { get; set; }

        public string VideoURL { get; set; }

        public string Description { get; set; }

        public Course? Course { get; set; }

        public int CourseId { get; set; }

        public Instructor? Instructor { get; set; }

        public string? InstructorId { get; set; }

        public Quiz? quiz { get; set; }

        public ICollection<LessonWatch>? Lessonwatches { get; set; }
    }
}