using Edu_Project.Data;
using Edu_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Edu_Project.Controllers
{
    [Authorize]
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

        [Authorize(Roles = "Instructor")]
        [HttpGet]
        public async Task<IActionResult> CreateLesson(
            int cid)
        {
            var instructorId =
                userManager.GetUserId(User);

            if (instructorId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var course =
                await context.Courses
                    .FirstOrDefaultAsync(
                        c => c.Id == cid);

            if (course == null)
            {
                return NotFound();
            }

            if (course.InstructorId !=
                instructorId)
            {
                return Forbid();
            }

            ViewBag.courseid = cid;

            return View();
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLesson(
            Lesson lesson)
        {
            var instructorId =
                userManager.GetUserId(User);

            if (instructorId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var course =
                await context.Courses
                    .FirstOrDefaultAsync(
                        c =>
                            c.Id ==
                            lesson.CourseId);

            if (course == null)
            {
                return NotFound();
            }

            if (course.InstructorId !=
                instructorId)
            {
                return Forbid();
            }

            lesson.InstructorId =
                instructorId;

            if (!ModelState.IsValid)
            {
                ViewBag.courseid =
                    lesson.CourseId;

                return View(lesson);
            }

            context.Lessons.Add(
                lesson);

            await context.SaveChangesAsync();

            return RedirectToAction(
                "CreateQuiz",
                "Exam",
                new
                {
                    lid = lesson.Id
                });
        }

        [Authorize(Roles = "Instructor")]
        [HttpGet]
        public async Task<IActionResult>
            LessonDetailsForInstructor(
                int id)
        {
            var instructorId =
                userManager.GetUserId(User);

            if (instructorId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var course =
                await context.Courses
                    .FirstOrDefaultAsync(
                        c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            if (course.InstructorId !=
                instructorId)
            {
                return Forbid();
            }

            var lessons =
                await context.Lessons
                    .Where(l =>
                        l.CourseId == id &&
                        l.InstructorId ==
                        instructorId)
                    .Include(l =>
                        l.quiz)
                    .OrderBy(l =>
                        l.Order)
                    .ToListAsync();

            ViewBag.cid = id;

            return View(lessons);
        }

        [Authorize(Roles = "Instructor")]
        [HttpGet]
        public async Task<IActionResult> EditLesson(
            int id)
        {
            var instructorId =
                userManager.GetUserId(User);

            if (instructorId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var lesson =
                await context.Lessons
                    .FirstOrDefaultAsync(
                        l => l.Id == id);

            if (lesson == null)
            {
                return NotFound();
            }

            if (lesson.InstructorId !=
                instructorId)
            {
                return Forbid();
            }

            var courseBelongsToInstructor =
                await context.Courses
                    .AnyAsync(c =>
                        c.Id ==
                            lesson.CourseId &&
                        c.InstructorId ==
                            instructorId);

            if (!courseBelongsToInstructor)
            {
                return Forbid();
            }

            return View(lesson);
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLesson(
            Lesson lesson)
        {
            var instructorId =
                userManager.GetUserId(User);

            if (instructorId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var lessonInDb =
                await context.Lessons
                    .FirstOrDefaultAsync(
                        l => l.Id ==
                            lesson.Id);

            if (lessonInDb == null)
            {
                return NotFound();
            }

            if (lessonInDb.InstructorId !=
                instructorId)
            {
                return Forbid();
            }

            var courseBelongsToInstructor =
                await context.Courses
                    .AnyAsync(c =>
                        c.Id ==
                            lessonInDb.CourseId &&
                        c.InstructorId ==
                            instructorId);

            if (!courseBelongsToInstructor)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(lesson);
            }

            lessonInDb.Title =
                lesson.Title;

            lessonInDb.Description =
                lesson.Description;

            lessonInDb.VideoURL =
                lesson.VideoURL;

            lessonInDb.Order =
                lesson.Order;

            await context.SaveChangesAsync();

            return RedirectToAction(
                nameof(
                    LessonDetailsForInstructor),
                new
                {
                    id =
                        lessonInDb.CourseId
                });
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLesson(
            int id)
        {
            var instructorId =
                userManager.GetUserId(User);

            if (instructorId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var lesson =
                await context.Lessons
                    .Include(l => l.quiz)
                        .ThenInclude(q =>
                            q.Questions)
                            .ThenInclude(q =>
                                q.Answers)
                    .FirstOrDefaultAsync(
                        l => l.Id == id);

            if (lesson == null)
            {
                return NotFound();
            }

            if (lesson.InstructorId !=
                instructorId)
            {
                return Forbid();
            }

            var courseBelongsToInstructor =
                await context.Courses
                    .AnyAsync(c =>
                        c.Id ==
                            lesson.CourseId &&
                        c.InstructorId ==
                            instructorId);

            if (!courseBelongsToInstructor)
            {
                return Forbid();
            }

            var courseId =
                lesson.CourseId;

            var lessonWatches =
                await context.LessonsWatch
                    .Where(lw =>
                        lw.LessonId ==
                        lesson.Id)
                    .ToListAsync();

            if (lessonWatches.Any())
            {
                context.LessonsWatch
                    .RemoveRange(
                        lessonWatches);
            }

            if (lesson.quiz != null)
            {
                var quizId =
                    lesson.quiz.Id;

                var quizGrades =
                    await context.Quizgrades
                        .Where(qg =>
                            qg.quizId ==
                            quizId)
                        .ToListAsync();

                if (quizGrades.Any())
                {
                    context.Quizgrades
                        .RemoveRange(
                            quizGrades);
                }

                var questions =
                    await context.Questions
                        .Where(q =>
                            q.QuizId ==
                            quizId)
                        .Include(q =>
                            q.Answers)
                        .ToListAsync();

                foreach (var question
                         in questions)
                {
                    var studentAnswers =
                        await context.StudentAnswers
                            .Where(sa =>
                                sa.questionId ==
                                question.Id)
                            .ToListAsync();

                    if (studentAnswers.Any())
                    {
                        context.StudentAnswers
                            .RemoveRange(
                                studentAnswers);
                    }

                    if (question.Answers != null &&
                        question.Answers.Any())
                    {
                        context.Answers
                            .RemoveRange(
                                question.Answers);
                    }
                }

                if (questions.Any())
                {
                    context.Questions
                        .RemoveRange(
                            questions);
                }

                context.Quizzes.Remove(
                    lesson.quiz);
            }

            context.Lessons.Remove(
                lesson);

            await context.SaveChangesAsync();

            return RedirectToAction(
                nameof(
                    LessonDetailsForInstructor),
                new
                {
                    id = courseId
                });
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> CourseLessons(
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

            var isEnrolled =
                await context.Enrollments
                    .AnyAsync(e =>
                        e.StudentId ==
                            studentId &&
                        e.CourseId ==
                            courseId);

            if (!isEnrolled)
            {
                return Forbid();
            }

            var lessons =
                await context.Lessons
                    .Where(l =>
                        l.CourseId ==
                        courseId)
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
                        lw.StudentId ==
                            studentId &&
                        lw.Seen &&
                        lessons
                            .Select(l => l.Id)
                            .Contains(
                                lw.LessonId))
                    .Select(lw =>
                        lw.LessonId)
                    .ToListAsync();

            ViewBag.CourseId =
                courseId;

            ViewBag.WatchedLessonIds =
                watchedLessonIds;

            return View(lessons);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            MarkAsWatched(
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
                        l => l.Id ==
                            lessonId);

            if (lesson == null)
            {
                return NotFound();
            }

            var isEnrolled =
                await context.Enrollments
                    .AnyAsync(e =>
                        e.StudentId ==
                            studentId &&
                        e.CourseId ==
                            lesson.CourseId);

            if (!isEnrolled)
            {
                return Forbid();
            }

            var lessonWatch =
                await context.LessonsWatch
                    .FirstOrDefaultAsync(
                        lw =>
                            lw.StudentId ==
                                studentId &&
                            lw.LessonId ==
                                lessonId);

            if (lessonWatch == null)
            {
                lessonWatch =
                    new LessonWatch
                    {
                        StudentId =
                            studentId,

                        LessonId =
                            lessonId,

                        Seen = true
                    };

                context.LessonsWatch.Add(
                    lessonWatch);
            }
            else
            {
                lessonWatch.Seen =
                    true;
            }

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Lesson marked as watched.";

            return RedirectToAction(
                nameof(CourseLessons),
                new
                {
                    courseId =
                        lesson.CourseId
                });
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            MarkAsNotWatched(
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
                        l => l.Id ==
                            lessonId);

            if (lesson == null)
            {
                return NotFound();
            }

            var isEnrolled =
                await context.Enrollments
                    .AnyAsync(e =>
                        e.StudentId ==
                            studentId &&
                        e.CourseId ==
                            lesson.CourseId);

            if (!isEnrolled)
            {
                return Forbid();
            }

            var lessonWatch =
                await context.LessonsWatch
                    .FirstOrDefaultAsync(
                        lw =>
                            lw.StudentId ==
                                studentId &&
                            lw.LessonId ==
                                lessonId);

            if (lessonWatch != null)
            {
                lessonWatch.Seen =
                    false;

                await context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] =
                "Lesson marked as not watched.";

            return RedirectToAction(
                nameof(CourseLessons),
                new
                {
                    courseId =
                        lesson.CourseId
                });
        }
    }
}