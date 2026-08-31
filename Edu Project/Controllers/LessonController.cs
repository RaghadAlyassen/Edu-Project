using Edu_Project.Data;
using Edu_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Edu_Project.Controllers
{
    [Authorize(Roles = "Student")]
    public class LessonController : Controller
    {
        private readonly Context context;
        private readonly UserManager<User> userManager;

        public LessonController(
            Context context,
            UserManager<User> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> CourseLessons(int courseId)
        {
            var studentId =
                userManager.GetUserId(User);

            if (studentId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var isEnrolled =
                await context.Enrollments
                    .AnyAsync(e =>
                        e.StudentId == studentId &&
                        e.CourseId == courseId);

            if (!isEnrolled)
            {
                return Forbid();
            }

            var lessons =
                await context.Lessons
                    .Where(l =>
                        l.CourseId == courseId)
                    .Include(l =>
                        l.Instructor)
                    .Include(l =>
                        l.quiz)
                    .OrderBy(l =>
                        l.Order)
                    .ToListAsync();

            var watchedLessonIds =
                await context.LessonsWatch
                    .Where(lw =>
                        lw.StudentId == studentId &&
                        lw.Seen)
                    .Select(lw =>
                        lw.LessonId)
                    .ToListAsync();

            ViewBag.CourseId = courseId;
            ViewBag.WatchedLessonIds =
                watchedLessonIds;

            return View(lessons);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsWatched(
            int lessonId)
        {
            var studentId =
                userManager.GetUserId(User);

            if (studentId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var lesson =
                await context.Lessons
                    .FirstOrDefaultAsync(
                        l => l.Id == lessonId);

            if (lesson == null)
            {
                return NotFound();
            }

            var isEnrolled =
                await context.Enrollments
                    .AnyAsync(e =>
                        e.StudentId == studentId &&
                        e.CourseId ==
                            lesson.CourseId);

            if (!isEnrolled)
            {
                return Forbid();
            }

            var lessonWatch =
                await context.LessonsWatch
                    .FirstOrDefaultAsync(lw =>
                        lw.StudentId == studentId &&
                        lw.LessonId == lessonId);

            if (lessonWatch == null)
            {
                lessonWatch =
                    new LessonWatch
                    {
                        StudentId = studentId,
                        LessonId = lessonId,
                        Seen = true
                    };

                context.LessonsWatch.Add(
                    lessonWatch);
            }
            else
            {
                lessonWatch.Seen = true;
            }

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Lesson marked as watched.";

            return RedirectToAction(
                nameof(CourseLessons),
                new
                {
                    courseId = lesson.CourseId
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsNotWatched(
            int lessonId)
        {
            var studentId =
                userManager.GetUserId(User);

            if (studentId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var lesson =
                await context.Lessons
                    .FirstOrDefaultAsync(
                        l => l.Id == lessonId);

            if (lesson == null)
            {
                return NotFound();
            }

            var lessonWatch =
                await context.LessonsWatch
                    .FirstOrDefaultAsync(lw =>
                        lw.StudentId == studentId &&
                        lw.LessonId == lessonId);

            if (lessonWatch != null)
            {
                lessonWatch.Seen = false;

                await context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] =
                "Lesson marked as not watched.";

            return RedirectToAction(
                nameof(CourseLessons),
                new
                {
                    courseId = lesson.CourseId
                });
        }
    }
}