namespace Edu_Project.Models
{
    public class LessonWatch
    {
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }
        public string StudentId { get; set; }
        public Student Student { get; set; }
        public bool Seen { get; set; }



    }
}
