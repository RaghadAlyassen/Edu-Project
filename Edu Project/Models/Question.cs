using System.ComponentModel.DataAnnotations;

namespace Edu_Project.Models
{
    public class Question
    {
       [ Key] public int Id { get; set; }
        public string Text { get; set; }
        public ICollection<StudentAnswer> Studentanswers { get; set; }
        public FinalExam Finalexam { get; set; }
        public int? FinalexamId { get; set; }
        public Quiz Quiz { get; set; }
        public int? QuizId { get; set; }
        public ICollection<Answer> Answers { get; set; }




    }
}
