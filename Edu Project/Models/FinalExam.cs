namespace Edu_Project.Models
{
    public class FinalExam : Exam
    {
        public Course course { get; set; }
        public int courseId { get; set; }

        public ICollection<Question> Question { get; set; }
        public ICollection<FinalGrade> finalgrades { get; set; }



    }
}
