namespace Edu_Project.Models
{
    public class Instructor :User
    {
        public string Specialization { get; set; }
        public ICollection<Lesson>? lessons { get; set; }
        public ICollection<Course>? courses { get; set; }
        public ICollection<Exam>? Exams { get; set; }
        public ICollection<Category>? categories { get; set; }

    }
}
