using System.ComponentModel.DataAnnotations;

namespace Edu_Project.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<Course>? Courses { get; set; }
    }
}