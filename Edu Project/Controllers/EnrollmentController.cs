using Edu_Project.Data;
using Edu_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Edu_Project.Controllers
{
    [Authorize(Roles = "Student")]
    public class EnrollmentController : Controller
    {
        private readonly Context context;
        private readonly UserManager<User> userManager;

        public EnrollmentController(
            Context context,
            UserManager<User> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Courses()
        {
            var studentId =
                userManager.GetUserId(User);

            var enrolledCourseIds =
                await context.Enrollments
                    .Where(e =>
                        e.StudentId == studentId)
                    .Select(e =>
                        e.CourseId)
                    .ToListAsync();

            ViewBag.EnrolledCourseIds =
                enrolledCourseIds;

            var courses =
                await context.Courses
                    .Include(c =>
                        c.Instructor)
                    .Include(c =>
                        c.Category)
                    .ToListAsync();

            return View(courses);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(
            int courseId)
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

            var course =
                await context.Courses
                    .FirstOrDefaultAsync(
                        c => c.Id == courseId);

            if (course == null)
            {
                return NotFound();
            }

            var alreadyEnrolled =
                await context.Enrollments
                    .AnyAsync(e =>
                        e.StudentId == studentId &&
                        e.CourseId == courseId);

            if (alreadyEnrolled)
            {
                TempData["ErrorMessage"] =
                    "You are already enrolled in this course.";

                return RedirectToAction(
                    nameof(Courses));
            }

            var enrollment =
                new Enrollment
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    EnrollmentDate = DateTime.Now
                };

            context.Enrollments.Add(
                enrollment);

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Course enrolled successfully.";

            return RedirectToAction(
                nameof(MyCourses));
        }

        [HttpGet]
        public async Task<IActionResult> MyCourses()
        {
            var studentId =
                userManager.GetUserId(User);

            if (studentId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var enrollments =
                await context.Enrollments
                    .Where(e =>
                        e.StudentId == studentId)
                    .Include(e =>
                        e.Course)
                    .ThenInclude(c =>
                        c.Instructor)
                    .Include(e =>
                        e.Course)
                    .ThenInclude(c =>
                        c.Category)
                    .Include(e =>
                        e.Course)
                    .ThenInclude(c =>
                        c.Finalexam)
                    .OrderByDescending(e =>
                        e.EnrollmentDate)
                    .ToListAsync();

            return View(enrollments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unenroll(
            int courseId)
        {
            var studentId =
                userManager.GetUserId(User);

            if (studentId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var enrollment =
                await context.Enrollments
                    .FirstOrDefaultAsync(e =>
                        e.StudentId == studentId &&
                        e.CourseId == courseId);

            if (enrollment == null)
            {
                return NotFound();
            }

            context.Enrollments.Remove(
                enrollment);

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Course removed successfully.";

            return RedirectToAction(
                nameof(MyCourses));
        }
    }
}