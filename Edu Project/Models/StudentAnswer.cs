namespace Edu_Project.Models
{
    public class StudentAnswer
    {

        public Answer answer { get; set; }
        public int answerId { get; set; }
        public Question question { get; set; }
        public int questionId { get; set; }
        public Student student { get; set; }
        public string studentId { get; set; }
        public bool status { get; set; }

    }
}
