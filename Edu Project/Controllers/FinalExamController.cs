using Edu_Project.Data;
using Edu_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Edu_Project.Controllers
{
    [Authorize(Roles = "Student")]
    public class FinalExamController : Controller
    {
        private readonly Context context;
        private readonly UserManager<User> userManager;

        public FinalExamController(
            Context context,
            UserManager<User> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Take(int finalExamId)
        {
            var studentId =
                userManager.GetUserId(User);

            if (studentId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var finalExam =
                await context.FinalExams
                    .Include(f => f.course)
                    .Include(f => f.Question)
                        .ThenInclude(q => q.Answers)
                    .FirstOrDefaultAsync(
                        f => f.Id == finalExamId);

            if (finalExam == null)
            {
                return NotFound();
            }

            var isEnrolled =
                await context.Enrollments
                    .AnyAsync(e =>
                        e.StudentId == studentId &&
                        e.CourseId == finalExam.courseId);

            if (!isEnrolled)
            {
                return Forbid();
            }

            var alreadyTaken =
                await context.Finalgrades
                    .AnyAsync(fg =>
                        fg.StudentId == studentId &&
                        fg.FinalexamId == finalExamId);

            if (alreadyTaken)
            {
                return RedirectToAction(
                    nameof(Result),
                    new
                    {
                        finalExamId
                    });
            }

            return View(finalExam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            int finalExamId,
            Dictionary<int, int> answers)
        {
            var studentId =
                userManager.GetUserId(User);

            if (studentId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var finalExam =
                await context.FinalExams
                    .Include(f => f.course)
                    .Include(f => f.Question)
                        .ThenInclude(q => q.Answers)
                    .FirstOrDefaultAsync(
                        f => f.Id == finalExamId);

            if (finalExam == null)
            {
                return NotFound();
            }

            var isEnrolled =
                await context.Enrollments
                    .AnyAsync(e =>
                        e.StudentId == studentId &&
                        e.CourseId == finalExam.courseId);

            if (!isEnrolled)
            {
                return Forbid();
            }

            var alreadyTaken =
                await context.Finalgrades
                    .AnyAsync(fg =>
                        fg.StudentId == studentId &&
                        fg.FinalexamId == finalExamId);

            if (alreadyTaken)
            {
                return RedirectToAction(
                    nameof(Result),
                    new
                    {
                        finalExamId
                    });
            }

            int correctAnswers = 0;

            foreach (var question in finalExam.Question)
            {
                if (!answers.ContainsKey(question.Id))
                {
                    continue;
                }

                var selectedAnswerId =
                    answers[question.Id];

                var selectedAnswer =
                    question.Answers
                        .FirstOrDefault(a =>
                            a.Id == selectedAnswerId);

                if (selectedAnswer == null)
                {
                    continue;
                }

                var isCorrect =
                    selectedAnswer.IsCorrect;

                if (isCorrect)
                {
                    correctAnswers++;
                }

                var existingStudentAnswer =
                    await context.StudentAnswers
                        .FirstOrDefaultAsync(sa =>
                            sa.studentId == studentId &&
                            sa.questionId == question.Id &&
                            sa.answerId == selectedAnswer.Id);

                if (existingStudentAnswer == null)
                {
                    context.StudentAnswers.Add(
                        new StudentAnswer
                        {
                            studentId = studentId,
                            questionId = question.Id,
                            answerId = selectedAnswer.Id,
                            status = isCorrect
                        });
                }
            }

            int grade = 0;

            if (finalExam.Question.Any())
            {
                grade =
                    (int)Math.Round(
                        (double)correctAnswers /
                        finalExam.Question.Count *
                        finalExam.TotalMarks);
            }

            context.Finalgrades.Add(
                new FinalGrade
                {
                    StudentId = studentId,
                    FinalexamId = finalExam.Id,
                    Grade = grade
                });

            await context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Result),
                new
                {
                    finalExamId
                });
        }

        [HttpGet]
        public async Task<IActionResult> Result(
            int finalExamId)
        {
            var studentId =
                userManager.GetUserId(User);

            if (studentId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var finalExam =
                await context.FinalExams
                    .Include(f =>
                        f.course)
                    .FirstOrDefaultAsync(
                        f => f.Id == finalExamId);

            if (finalExam == null)
            {
                return NotFound();
            }

            var grade =
                await context.Finalgrades
                    .FirstOrDefaultAsync(fg =>
                        fg.StudentId == studentId &&
                        fg.FinalexamId == finalExamId);

            if (grade == null)
            {
                return RedirectToAction(
                    nameof(Take),
                    new
                    {
                        finalExamId
                    });
            }

            ViewBag.FinalExam = finalExam;

            return View(grade);
        }

        [HttpGet]
        public async Task<IActionResult> MyFinalGrades()
        {
            var studentId =
                userManager.GetUserId(User);

            if (studentId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var grades =
                await context.Finalgrades
                    .Where(fg =>
                        fg.StudentId == studentId)
                    .Include(fg =>
                        fg.Finalexam)
                    .ThenInclude(f =>
                        f.course)
                    .ToListAsync();

            return View(grades);
        }
    }
}