using Edu_Project.Data;
using Edu_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;




namespace Edu_Project.Controllers
{
    public class CourseController : Controller
    {

        private readonly Context _context;
        public CourseController (Context context)
        {
            _context = context;
           

        }
        
        public async Task<IActionResult>  Index()
        {
            var courses = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .ToArrayAsync();
            return View(courses);
        }

        public async Task<IActionResult> Details(int ? Id)
        {
            if (Id == null) return NotFound();

            var Course = await _context.Courses
            .Include (c => c.Instructor)
            .Include (c => c.Category)
            .FirstOrDefaultAsync( m => m.Id == Id);

            if (Course == null) return NotFound();

            return View(Course);

        }

        //[Authorize(Roles = "Instructor")]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.Instructors = new SelectList(_context.Instructors, "Id", "UserName");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
       
        public async Task<IActionResult> Create(Course course)
        {

            var defaultInstructor = _context.Users.FirstOrDefault();
            if (defaultInstructor != null)
            {
                course.InstructorId = defaultInstructor.Id;
            }


            if (course.CategoryId == 0)
            {
                ModelState.AddModelError("CategoryId", "Please select a valid category.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", course.CategoryId);
            ViewBag.Instructors = new SelectList(_context.Instructors, "Id", "UserName", course.InstructorId);
            return View(course);
        }


        //[Authorize(Roles ="Instructor")]
        [HttpGet]
        public async Task<IActionResult> Edit (int? Id)
        {

            if (Id == null) return NotFound();
            var Course = await _context.Courses.FindAsync(Id);
            if (Course == null) return NotFound();

            ViewBag.categoryId = new SelectList(_context.Categories, "Id", "Name", Course.CategoryId);
            ViewBag.InstructorId = new SelectList(_context.Instructors, "Id", "UserName", Course.InstructorId);

            return View(Course);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles ="Instructor")]
        public async Task<IActionResult> Edit (   int Id, Course course)
        {

           

            if (string.IsNullOrEmpty(course.InstructorId))
            {
                var existingCourse = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == Id);
                if (existingCourse != null)
                {
                    course.InstructorId = existingCourse.InstructorId;
                }
            }
            


            if (Id != course.Id) return NotFound();
            if (ModelState.IsValid)
            {

                try
                {
                _context.Update(course);
                await _context.SaveChangesAsync();



                }

                catch (DbUpdateConcurrencyException) 
                {
                    if (!_context.Courses.Any(e => e.Id == Id)) 
                    {
                        return NotFound();

                    }

                    else throw;

                    
                }
                return RedirectToAction(nameof(Index));

            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", course.CategoryId);
            ViewBag.Instructors = new SelectList(_context.Instructors, "Id", "UserName", course.InstructorId);
            return View(course);
            

        }

       
    }
}
