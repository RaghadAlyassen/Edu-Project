using System.ComponentModel.DataAnnotations;

namespace Edu_Project.Models
{
    public class FinalGrade
    {
        public FinalExam Finalexam { get; set; }
        public int FinalexamId { get; set; }
        public Student Student { get; set; }
        public string StudentId { get; set; }
        public int Grade { get; set; }


    }
}
