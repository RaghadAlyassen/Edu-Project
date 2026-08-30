namespace Edu_Project.Models
{
    public class QuizGrade
    {
        public Quiz quiz { get; set; }
        public int quizId { get; set; }
        public Student Student { get; set; }
        public string StudentId { get; set; }
        public int Grade { get; set; }


    }
}
