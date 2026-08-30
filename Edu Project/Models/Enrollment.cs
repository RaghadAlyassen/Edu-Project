namespace Edu_Project.Models
{
    public class Enrollment
    {
        public Course Course { get; set; }
        public int CourseId { get; set; }
        public Student Student { get; set; }
        public string StudentId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public Enrollment()
        {
            EnrollmentDate = DateTime.Now;
        }



    }
}
