using Edu_Project.Data;
using Edu_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Edu_Project.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentDashboardController : Controller
    {
        private readonly Context context;
        private readonly UserManager<User> userManager;

        public StudentDashboardController(
            Context context,
            UserManager<User> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var studentId =
                userManager.GetUserId(User);

            if (studentId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var student =
                await context.Students
                    .FirstOrDefaultAsync(
                        s => s.Id == studentId);

            if (student == null)
            {
                return NotFound();
            }

            ViewBag.Student = student;

            ViewBag.EnrolledCoursesCount =
                await context.Enrollments
                    .CountAsync(e =>
                        e.StudentId == studentId);

            ViewBag.WatchedLessonsCount =
                await context.LessonsWatch
                    .CountAsync(lw =>
                        lw.StudentId == studentId &&
                        lw.Seen);

            ViewBag.QuizGradesCount =
                await context.Quizgrades
                    .CountAsync(qg =>
                        qg.StudentId == studentId);

            ViewBag.FinalGradesCount =
                await context.Finalgrades
                    .CountAsync(fg =>
                        fg.StudentId == studentId);

            return View();
        }
    }
}