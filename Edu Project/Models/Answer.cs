using System.ComponentModel.DataAnnotations;

namespace Edu_Project.Models
{
    public class Answer
    {
        [Key]public int Id { get; set; }
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
        public ICollection<StudentAnswer> StudentAnswers { get; set; }

        public int QuestionId { get; set; }
        public Question Question { get; set; }


    }
}
