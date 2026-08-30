namespace Edu_Project.Models
{
    public class Quiz : Exam 
    {


        public Lesson Lesson { get; set; }
        public int LessonId { get; set; }
        public ICollection<Question> Questions { get; set; }
        public ICollection<QuizGrade> Quizgrades { get; set; }



    }
}
