using Edu_Project.Data;
using Edu_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Edu_Project.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class ExamController : Controller
    {
        private readonly Context context;
        private readonly UserManager<User> userManager;

        public ExamController(
            Context context,
            UserManager<User> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public IActionResult CreateQuiz(int lid)
        {
            var vm = new QuizVM
            {
                lessonid = lid,
                numberOfQuestions = 5
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateQuiz(QuizVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var instructorId =
                userManager.GetUserId(User);

            if (instructorId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var lesson =
                context.Lessons
                    .FirstOrDefault(
                        l => l.Id == vm.lessonid);

            if (lesson == null)
            {
                return NotFound();
            }

            if (lesson.InstructorId != instructorId)
            {
                return Forbid();
            }

            var existingQuiz =
                context.Quizzes
                    .FirstOrDefault(
                        q => q.LessonId == vm.lessonid);

            if (existingQuiz != null)
            {
                return RedirectToAction(
                    nameof(QuizDetails),
                    new
                    {
                        id = vm.lessonid
                    });
            }

            var quiz = new Quiz
            {
                Title = vm.Title,
                Duration = vm.duration,
                TotalMarks = vm.totalmarks,
                LessonId = vm.lessonid,
                InstructorId = instructorId
            };

            context.Quizzes.Add(quiz);

            context.SaveChanges();

            return RedirectToAction(
                nameof(InsertQuestions),
                new
                {
                    examtype = "quiz",
                    examid = quiz.Id,
                    count = vm.numberOfQuestions
                });
        }

        [HttpGet]
        public IActionResult CreateFinal(int cid)
        {
            var vm = new FinalVM
            {
                courseid = cid,
                numberOfQuestions = 10
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateFinal(FinalVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var instructorId =
                userManager.GetUserId(User);

            if (instructorId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var course =
                context.Courses
                    .FirstOrDefault(
                        c => c.Id == vm.courseid);

            if (course == null)
            {
                return NotFound();
            }

            if (course.InstructorId != instructorId)
            {
                return Forbid();
            }

            var existingFinal =
                context.FinalExams
                    .FirstOrDefault(
                        f => f.courseId == vm.courseid);

            if (existingFinal != null)
            {
                return RedirectToAction(
                    nameof(FinalDetails),
                    new
                    {
                        id = vm.courseid
                    });
            }

            var finalExam = new FinalExam
            {
                Title = vm.Title,
                Duration = vm.duration,
                TotalMarks = vm.totalmarks,
                courseId = vm.courseid,
                InstructorId = instructorId
            };

            context.FinalExams.Add(finalExam);

            context.SaveChanges();

            return RedirectToAction(
                nameof(InsertQuestions),
                new
                {
                    examtype = "final",
                    examid = finalExam.Id,
                    count = vm.numberOfQuestions
                });
        }

        [HttpGet]
        public IActionResult InsertQuestions(
            string examtype,
            int examid,
            int count = 1)
        {
            if (examtype != "quiz" &&
                examtype != "final")
            {
                return NotFound();
            }

            var instructorId =
                userManager.GetUserId(User);

            if (instructorId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            if (examtype == "quiz")
            {
                var quiz =
                    context.Quizzes
                        .FirstOrDefault(
                            q => q.Id == examid);

                if (quiz == null)
                {
                    return NotFound();
                }

                if (quiz.InstructorId != instructorId)
                {
                    return Forbid();
                }
            }
            else
            {
                var finalExam =
                    context.FinalExams
                        .FirstOrDefault(
                            f => f.Id == examid);

                if (finalExam == null)
                {
                    return NotFound();
                }

                if (finalExam.InstructorId != instructorId)
                {
                    return Forbid();
                }
            }

            if (count < 1)
            {
                count = 1;
            }

            if (count > 50)
            {
                count = 50;
            }

            ViewBag.Examtype = examtype;
            ViewBag.examid = examid;
            ViewBag.QuestionCount = count;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InsertQuestions(
            List<QuestionVM> models,
            string examtype,
            int examid)
        {
            if (examtype != "quiz" &&
                examtype != "final")
            {
                return NotFound();
            }

            var instructorId =
                userManager.GetUserId(User);

            if (instructorId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            if (examtype == "quiz")
            {
                var quizOwner =
                    context.Quizzes
                        .FirstOrDefault(
                            q => q.Id == examid);

                if (quizOwner == null)
                {
                    return NotFound();
                }

                if (quizOwner.InstructorId != instructorId)
                {
                    return Forbid();
                }
            }
            else
            {
                var finalOwner =
                    context.FinalExams
                        .FirstOrDefault(
                            f => f.Id == examid);

                if (finalOwner == null)
                {
                    return NotFound();
                }

                if (finalOwner.InstructorId != instructorId)
                {
                    return Forbid();
                }
            }

            if (models == null ||
                models.Count == 0)
            {
                ViewBag.Examtype = examtype;
                ViewBag.examid = examid;
                ViewBag.QuestionCount = 1;

                ModelState.AddModelError(
                    "",
                    "Please add at least one question.");

                return View(models);
            }

            foreach (var item in models)
            {
                if (string.IsNullOrWhiteSpace(item.text))
                {
                    continue;
                }

                if (item.answers == null ||
                    item.answers.Count < 4)
                {
                    continue;
                }

                if (item.correct < 1 ||
                    item.correct > 4)
                {
                    continue;
                }

                var question = new Question
                {
                    Text = item.text,
                    Answers = new List<Answer>()
                };

                if (examtype == "final")
                {
                    question.FinalexamId =
                        examid;
                }
                else
                {
                    question.QuizId =
                        examid;
                }

                context.Questions.Add(question);

                for (var j = 0; j < 4; j++)
                {
                    question.Answers.Add(
                        new Answer
                        {
                            Text = item.answers[j],
                            IsCorrect =
                                j + 1 == item.correct,
                            Question = question
                        });
                }
            }

            context.SaveChanges();

            if (examtype == "final")
            {
                var finalExam =
                    context.FinalExams
                        .FirstOrDefault(
                            f => f.Id == examid);

                if (finalExam == null)
                {
                    return NotFound();
                }

                return RedirectToAction(
                    nameof(FinalDetails),
                    new
                    {
                        id = finalExam.courseId
                    });
            }

            var quiz =
                context.Quizzes
                    .Include(q => q.Lesson)
                    .FirstOrDefault(
                        q => q.Id == examid);

            if (quiz == null ||
                quiz.Lesson == null)
            {
                return NotFound();
            }

            return RedirectToAction(
                nameof(QuizDetails),
                new
                {
                    id = quiz.LessonId
                });
        }

        [HttpGet]
        public IActionResult QuizDetails(int id)
        {
            var instructorId =
                userManager.GetUserId(User);

            var quiz =
                context.Quizzes
                    .Include(q => q.Lesson)
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.Answers)
                    .FirstOrDefault(
                        q => q.LessonId == id);

            if (quiz == null)
            {
                var lesson =
                    context.Lessons
                        .FirstOrDefault(
                            l => l.Id == id);

                if (lesson == null)
                {
                    return NotFound();
                }

                if (lesson.InstructorId != instructorId)
                {
                    return Forbid();
                }

                return RedirectToAction(
                    nameof(CreateQuiz),
                    new
                    {
                        lid = id
                    });
            }

            if (quiz.InstructorId != instructorId)
            {
                return Forbid();
            }

            return View(quiz);
        }

        [HttpGet]
        public IActionResult EditQuiz(int id)
        {
            var quiz =
                context.Quizzes
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.Answers)
                    .FirstOrDefault(
                        q => q.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }

            var instructorId =
                userManager.GetUserId(User);

            if (quiz.InstructorId != instructorId)
            {
                return Forbid();
            }

            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditQuiz(Quiz quiz)
        {
            var instructorId =
                userManager.GetUserId(User);

            var quizInDb =
                context.Quizzes
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.Answers)
                    .FirstOrDefault(
                        q => q.Id == quiz.Id);

            if (quizInDb == null)
            {
                return NotFound();
            }

            if (quizInDb.InstructorId != instructorId)
            {
                return Forbid();
            }

            quizInDb.Title =
                quiz.Title;

            quizInDb.Duration =
                quiz.Duration;

            quizInDb.TotalMarks =
                quiz.TotalMarks;

            if (quiz.Questions != null)
            {
                foreach (var postedQuestion
                         in quiz.Questions)
                {
                    var questionInDb =
                        quizInDb.Questions?
                            .FirstOrDefault(
                                q =>
                                    q.Id ==
                                    postedQuestion.Id);

                    if (questionInDb == null)
                    {
                        continue;
                    }

                    questionInDb.Text =
                        postedQuestion.Text;

                    if (postedQuestion.Answers != null)
                    {
                        foreach (var postedAnswer
                                 in postedQuestion.Answers)
                        {
                            var answerInDb =
                                questionInDb.Answers?
                                    .FirstOrDefault(
                                        a =>
                                            a.Id ==
                                            postedAnswer.Id);

                            if (answerInDb == null)
                            {
                                continue;
                            }

                            answerInDb.Text =
                                postedAnswer.Text;

                            answerInDb.IsCorrect =
                                postedAnswer.IsCorrect;
                        }
                    }
                }
            }

            context.SaveChanges();

            return RedirectToAction(
                nameof(QuizDetails),
                new
                {
                    id = quizInDb.LessonId
                });
        }

        [HttpGet]
        public IActionResult FinalDetails(int id)
        {
            var instructorId =
                userManager.GetUserId(User);

            var finalExam =
                context.FinalExams
                    .Include(f => f.Question)
                        .ThenInclude(q => q.Answers)
                    .FirstOrDefault(
                        f => f.courseId == id);

            if (finalExam == null)
            {
                var course =
                    context.Courses
                        .FirstOrDefault(
                            c => c.Id == id);

                if (course == null)
                {
                    return NotFound();
                }

                if (course.InstructorId != instructorId)
                {
                    return Forbid();
                }

                return RedirectToAction(
                    nameof(CreateFinal),
                    new
                    {
                        cid = id
                    });
            }

            if (finalExam.InstructorId != instructorId)
            {
                return Forbid();
            }

            return View(finalExam);
        }

        [HttpGet]
        public IActionResult EditFinal(int id)
        {
            var finalExam =
                context.FinalExams
                    .Include(f => f.Question)
                        .ThenInclude(q => q.Answers)
                    .FirstOrDefault(
                        f => f.Id == id);

            if (finalExam == null)
            {
                return NotFound();
            }

            var instructorId =
                userManager.GetUserId(User);

            if (finalExam.InstructorId != instructorId)
            {
                return Forbid();
            }

            return View(finalExam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditFinal(
            FinalExam finalExam)
        {
            var instructorId =
                userManager.GetUserId(User);

            var finalInDb =
                context.FinalExams
                    .Include(f => f.Question)
                        .ThenInclude(q => q.Answers)
                    .FirstOrDefault(
                        f => f.Id == finalExam.Id);

            if (finalInDb == null)
            {
                return NotFound();
            }

            if (finalInDb.InstructorId != instructorId)
            {
                return Forbid();
            }

            finalInDb.Title =
                finalExam.Title;

            finalInDb.Duration =
                finalExam.Duration;

            finalInDb.TotalMarks =
                finalExam.TotalMarks;

            if (finalExam.Question != null)
            {
                foreach (var postedQuestion
                         in finalExam.Question)
                {
                    var questionInDb =
                        finalInDb.Question?
                            .FirstOrDefault(
                                q =>
                                    q.Id ==
                                    postedQuestion.Id);

                    if (questionInDb == null)
                    {
                        continue;
                    }

                    questionInDb.Text =
                        postedQuestion.Text;

                    if (postedQuestion.Answers != null)
                    {
                        foreach (var postedAnswer
                                 in postedQuestion.Answers)
                        {
                            var answerInDb =
                                questionInDb.Answers?
                                    .FirstOrDefault(
                                        a =>
                                            a.Id ==
                                            postedAnswer.Id);

                            if (answerInDb == null)
                            {
                                continue;
                            }

                            answerInDb.Text =
                                postedAnswer.Text;

                            answerInDb.IsCorrect =
                                postedAnswer.IsCorrect;
                        }
                    }
                }
            }

            context.SaveChanges();

            return RedirectToAction(
                nameof(FinalDetails),
                new
                {
                    id = finalInDb.courseId
                });
        }
    }
}