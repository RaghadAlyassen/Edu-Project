using Microsoft.AspNetCore.Identity;

namespace Edu_Project.Models
{
    public class User : IdentityUser
    {
        public string? ProfileImg { get; set; }
    }
}