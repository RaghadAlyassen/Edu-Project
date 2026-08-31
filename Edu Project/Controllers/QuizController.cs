using Edu_Project.Data;
using Edu_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Edu_Project.Controllers
{
    [Authorize(Roles = "Student")]
    public class QuizController : Controller
    {
        private readonly Context context;
        private readonly UserManager<User> userManager;

        public QuizController(
            Context context,
            UserManager<User> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Take(int quizId)
        {
            var studentId =
                userManager.GetUserId(User);

            if (studentId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var quiz =
                await context.Quizzes
                    .Include(q => q.Lesson)
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.Answers)
                    .FirstOrDefaultAsync(
                        q => q.Id == quizId);

            if (quiz == null)
            {
                return NotFound();
            }

            var isEnrolled =
                await context.Enrollments
                    .AnyAsync(e =>
                        e.StudentId == studentId &&
                        e.CourseId ==
                            quiz.Lesson.CourseId);

            if (!isEnrolled)
            {
                return Forbid();
            }

            var alreadyTaken =
                await context.Quizgrades
                    .AnyAsync(qg =>
                        qg.StudentId == studentId &&
                        qg.quizId == quizId);

            if (alreadyTaken)
            {
                return RedirectToAction(
                    nameof(Result),
                    new
                    {
                        quizId
                    });
            }

            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            int quizId,
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

            var quiz =
                await context.Quizzes
                    .Include(q => q.Lesson)
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.Answers)
                    .FirstOrDefaultAsync(
                        q => q.Id == quizId);

            if (quiz == null)
            {
                return NotFound();
            }

            var isEnrolled =
                await context.Enrollments
                    .AnyAsync(e =>
                        e.StudentId == studentId &&
                        e.CourseId ==
                            quiz.Lesson.CourseId);

            if (!isEnrolled)
            {
                return Forbid();
            }

            var alreadyTaken =
                await context.Quizgrades
                    .AnyAsync(qg =>
                        qg.StudentId == studentId &&
                        qg.quizId == quizId);

            if (alreadyTaken)
            {
                return RedirectToAction(
                    nameof(Result),
                    new
                    {
                        quizId
                    });
            }

            int correctAnswers = 0;

            foreach (var question in quiz.Questions)
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

                context.StudentAnswers.Add(
                    new StudentAnswer
                    {
                        studentId = studentId,
                        questionId = question.Id,
                        answerId = selectedAnswer.Id,
                        status = isCorrect
                    });
            }

            int grade = 0;

            if (quiz.Questions.Any())
            {
                grade =
                    (int)Math.Round(
                        (double)correctAnswers /
                        quiz.Questions.Count *
                        quiz.TotalMarks);
            }

            context.Quizgrades.Add(
                new QuizGrade
                {
                    StudentId = studentId,
                    quizId = quiz.Id,
                    Grade = grade
                });

            await context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Result),
                new
                {
                    quizId
                });
        }

        [HttpGet]
        public async Task<IActionResult> Result(int quizId)
        {
            var studentId =
                userManager.GetUserId(User);

            if (studentId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var quiz =
                await context.Quizzes
                    .Include(q => q.Lesson)
                    .FirstOrDefaultAsync(
                        q => q.Id == quizId);

            if (quiz == null)
            {
                return NotFound();
            }

            var grade =
                await context.Quizgrades
                    .FirstOrDefaultAsync(qg =>
                        qg.StudentId == studentId &&
                        qg.quizId == quizId);

            if (grade == null)
            {
                return RedirectToAction(
                    nameof(Take),
                    new
                    {
                        quizId
                    });
            }

            ViewBag.Quiz = quiz;

            return View(grade);
        }

        [HttpGet]
        public async Task<IActionResult> MyGrades()
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
                await context.Quizgrades
                    .Where(qg =>
                        qg.StudentId == studentId)
                    .Include(qg =>
                        qg.quiz)
                    .ThenInclude(q =>
                        q.Lesson)
                    .ToListAsync();

            return View(grades);
        }
    }
}