namespace Edu_Project.Models
{
    public class Student : User
    {
        public DateTime RegistrationDate { get; set; }
        public ICollection<StudentAnswer> studentanswers { get; set; }
        public ICollection<FinalGrade> finalGrades { get; set; }
        public ICollection<QuizGrade> quizgrades { get; set; }
        public ICollection<Enrollment> enrollments { get; set; }
        public ICollection<LessonWatch> lessonwatches { get; set; }
        public Student()
        {
            RegistrationDate = DateTime.Now;
        }

    }
}
