using Edu_Project.Data;
using Edu_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Edu_Project.Controllers
{
    public class LesonController : Controller
    {
        Context context;
        [HttpGet]
        public IActionResult CreateLesson(int cid)
        {
            ViewBag.courseid = cid;
            return View();
        }
        [HttpPost]
        public IActionResult CreateLesson(Lesson l)
        {
            if (ModelState.IsValid)
            {
                context.Lessons.Add(l);
                context.SaveChanges();
                return RedirectToAction("CreateQuiz", "Exam", new { lid = l.Id });
                
            }
            return View();
        }
        public IActionResult LessonDetailsForStudent(int id )
        {
            var lessons = context.Lessons.Where(l => l.CourseId == id).ToList();
            return View(lessons);
        }
        public IActionResult LessonDetailsForInstructor(int id)
        {
            var lessons = context.Lessons.Where(l => l.CourseId == id).ToList();
            ViewBag.cid = id;
            return View(lessons);
        }
        public IActionResult DeleteLesson(int id)
        {
            var lesson = context.Lessons.FirstOrDefault(l => l.Id == id);
            context.Lessons.Remove(lesson);
            context.SaveChanges();
            return RedirectToAction("LessonDetailsForInstructor",new {id=lesson.CourseId});
        }
        [HttpGet]
        public IActionResult EditLesson(int id)
        {
            var lesson = context.Lessons.FirstOrDefault(l => l.Id == id);
            return View(lesson);
        }
        [HttpPost]
        public IActionResult EditLesson(Lesson lesson)
        {
            if (ModelState.IsValid)
            {
                context.Lessons.Update(lesson);
                context.SaveChanges();
                return RedirectToAction("LessonDetailsForInstructor", new { id = lesson.CourseId });
            }
            return View(lesson);
        }
       
    }
}
