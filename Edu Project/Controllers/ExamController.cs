using AspNetCoreGeneratedDocument;
using Edu_Project.Data;
using Edu_Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Edu_Project.Controllers
{
    public class ExamController : Controller
    {
        Context context;
        UserManager<User> usermanager;
        [HttpGet]
        public IActionResult CreateQuiz(int lid)
        {
            var vm = new QuizVM()
            {
                lessonid = lid
            };

            return View(vm);
        }
        [HttpPost]
        public IActionResult CreateQuiz(QuizVM vm)
        {
           if (ModelState.IsValid)
            {
                var quiz = new Quiz()
                {
                    Title = vm.Title,
                    Duration = vm.duration,
                    TotalMarks = vm.totalmarks,
                    LessonId = vm.lessonid,
                    InstructorId = usermanager.GetUserId(User)
                };
                context.Quizzes.Add(quiz);
                context.SaveChanges();
                return RedirectToAction("InsertQuestions", new { examtype = "quiz", examid = quiz.Id });
            }
           return View(vm);
        }


        [HttpGet]
        public IActionResult CreateFinal(int cid)
        {
            var vm = new FinalVM()
            {
                courseid = cid
            };
            return View(vm);
        }
        [HttpPost]
        public IActionResult CreateFinal(FinalVM vm)
        {
            if (ModelState.IsValid)
            {
                var final = new FinalExam()
                {
                    TotalMarks = vm.totalmarks,
                    Title = vm.Title,
                    Duration = vm.duration,
                    courseId = vm.courseid,
                    InstructorId = usermanager.GetUserId(User)
                };
                context.FinalExams.Add(final);
                context.SaveChanges();
                return RedirectToAction("InsertQuestions", new { examtype = "final", examid = final.Id });
            }
            return View(vm);
           
            
        }


        [HttpGet]
        public IActionResult InsertQuestions(string examtype, int examid)
        {

            ViewBag.Examtype = examtype;
            ViewBag.examid = examid;
            return View();
        }
        [HttpPost]
        public IActionResult InsertQuestions(List<QuestionVM> models, string examtype, int examid)
        {
            foreach (var i in models)
            {
                var ques = new Question()
                {
                    Text = i.text,
                    Answers = new List<Answer>()
                };
                if (examtype == "final")
                {
                    ques.FinalexamId = examid;
                }
                else
                {
                    ques.QuizId = examid;
                }
                context.Questions.Add(ques);

                for (int j = 0; j < 4; j++)
                {
                    ques.Answers.Add(new Answer
                    {
                        Text = i.answers[j],
                        IsCorrect = (j + 1) == i.correct,
                        Question = ques
                    });
                }
            }
            context.SaveChanges();
            return View();
        }
        //لسه محطتش هي راحة فين
        [HttpGet]
        public IActionResult TakeQuiz(int lessonid)
        {
            var studentid = usermanager.GetUserId(User);
            var quiz = context.Quizzes.FirstOrDefault(q => q.LessonId == lessonid);
            var watched = context.LessonsWatch.Any(lw => lw.StudentId == studentid && lw.LessonId == quiz.LessonId && lw.Seen == true);
            if (!watched)
            {
                return Content("you can not enter this quiz before seenig the lesson");
            }
            else
            {
                return View(quiz);
            }
        }
        [HttpPost]
        public IActionResult TakeQuiz(int quizid, List<int> answers)
        {
            var studentid = usermanager.GetUserId(User);
            var quiz = context.Quizzes.FirstOrDefault(q => q.Id == quizid);
            int grade = 0;
            for (int i = 0; i < quiz.Questions.Count; i++)
            {
                var selectedAnswerId = answers[i];
                var correctAnswer = quiz.Questions.ToList()[i].Answers.FirstOrDefault(a => a.IsCorrect);

                if (correctAnswer != null && correctAnswer.Id == selectedAnswerId)
                {
                    grade++;
                }
            }

            var grd = new QuizGrade
            {
                quizId = quizid,
                StudentId = studentid,
                Grade = grade,
            };

            context.Quizgrades.Add(grd);
            context.SaveChanges();
            return RedirectToAction("Result", "Quiz", new {quizId=grd.quizId});
        }


        [HttpGet]
        public IActionResult TakeFinal(int courseid)
        {
            var final = context.FinalExams.FirstOrDefault(f => f.courseId == courseid);
            var studentid = usermanager.GetUserId(User);
            var lessons=context.Lessons.Where(l => l.CourseId==courseid).ToList();
            var watched = context.LessonsWatch.Where(lw => lw.StudentId == studentid && lw.Seen == true && lessons.Select(l => l.Id).Contains(lw.LessonId)).Select(lw=> lw.LessonId).ToList();
            bool allseen = lessons.All(l => watched.Contains(l.Id));
            if (!allseen)
            {
                return Content("you can not enter this exam before seenig all lessons in this course");
            }
            else
            {
                return View(final);
            }
        }
        [HttpPost]
        public IActionResult TakeFinal(int finalid,List<int> answers)
        {
            var studentid = usermanager.GetUserId(User);
            var final = context.FinalExams.FirstOrDefault(q => q.Id == finalid);
            int grade = 0;
            for (int i = 0; i < final.Question.Count; i++)
            {
                var selectedAnswerId = answers[i];
                var correctAnswer = final.Question.ToList()[i].Answers.FirstOrDefault(a => a.IsCorrect);

                if (correctAnswer != null && correctAnswer.Id == selectedAnswerId)
                {
                    grade++;
                }
            }
            var grd = new FinalGrade()
            {
                StudentId = studentid,
                FinalexamId = finalid,
                Grade = grade,
            };
            context.Finalgrades.Add(grd);
            context.SaveChanges();
            return RedirectToAction("Result", "FinalExam", new {finalExamId=grd.FinalexamId});
        }
        public IActionResult QuizDetails(int id)
        {
            var quiz = context.Quizzes.FirstOrDefault(q => q.LessonId == id);
            if (quiz != null)
            {
                return View(quiz);
            }
            else
            {
                return NotFound();
            }
           
        }
        [HttpGet]
        public IActionResult EditQuiz(int id)
        {
            var quiz = context.Quizzes.FirstOrDefault(q => q.Id==id);
            return View(quiz);
        }
        [HttpPost]
        public IActionResult EditQuiz(Quiz quiz)
        {
            if (ModelState.IsValid)
            {
                context.Quizzes.Update(quiz);
                context.SaveChanges();
                return RedirectToAction("QuizDetails", new { id = quiz.Id });
            }
            return View(quiz);
        }
        public IActionResult FinalDetails(int id)
        {
            var final = context.FinalExams.FirstOrDefault(q => q.courseId == id);
            if (final != null)
            {
                return View(final);
            }
            else
            {
                return NotFound();
            }
        }
        [HttpGet]
        public IActionResult EditFinal(int id)
        {
            var final = context.FinalExams.FirstOrDefault(f => f.Id == id);
            return View(final);
        }
        [HttpPost]
        public IActionResult EditFinal(FinalExam final)
        {
            if (ModelState.IsValid)
            {
                context.FinalExams.Update(final);
                context .SaveChanges();
                return RedirectToAction("FinalDetails", new {id =final.courseId});
            }
            else
            {
                return View(final);
            }
        }
    }
}
