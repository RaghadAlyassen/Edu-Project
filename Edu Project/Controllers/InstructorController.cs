
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Edu_Project.Data;
using Edu_Project.Models;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;


namespace Edu_Project.Controllers
{
    public class InstructorController : Controller
    {

        private readonly Context _context;
        
        public InstructorController(Context context) {

            _context = context;

        }

        public async Task<IActionResult> Index()
        {
           
            var Instructors = await _context.Instructors.ToListAsync();
            
             ViewBag.Courses = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .ToListAsync();


            ViewBag.Categories = await _context.Categories
            .Include(c => c.Instructor)
            .ToListAsync();

            return View(Instructors);


        }

        public async Task<IActionResult> Details(string? Id)
        {
            if (Id == null) return NotFound();
            var instructor = await _context.Instructors
            .Include(i => i.categories)
            .Include(i => i.courses)
            .Include(i => i.lessons)
            .Include(i => i.Exams)
            .FirstOrDefaultAsync(m => m.Id == Id);
            if (instructor == null) return NotFound();
            return View(instructor);

        }


        // [Authorize(Roles ="Admin")]
        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles ="Admin")]
        public async Task<IActionResult> Create( Instructor instructor)
        {

            
            if (ModelState.IsValid)
            {
                _context.Add(instructor);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(instructor);

        }



        //[Authorize(Roles ="Admin")]
        public async Task<IActionResult> Edit(string? Id)
        {
            if (Id == null) return NotFound();
            var instructor = await _context.Instructors.FindAsync(Id);
            if (instructor == null) return NotFound();
            return View(instructor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Instructor instructor)
        {
            if (id != instructor.Id)
            {
                return NotFound();
            }

            try
            {
                
                var instructorInDb = await _context.Instructors.FindAsync(id);
                if (instructorInDb == null)
                {
                    return NotFound();
                }

                
                instructorInDb.UserName = instructor.UserName;

                
                if (!string.IsNullOrEmpty(instructor.ProfileImg) && !instructor.ProfileImg.Contains(":\\"))
                {
                    instructorInDb.ProfileImg = instructor.ProfileImg;
                }

                _context.Update(instructorInDb);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "حدث خطأ أثناء الحفظ، يجدر المحاولة مجدداً.");
            }

            return View(instructor);
        }


        //[Authorize(Roles ="Admin")]
        public async Task<IActionResult> Delete(string Id)
        {

            if (Id == null ) return NotFound();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync (m => m.Id == Id);
                if (instructor == null) return NotFound();

            return View(instructor);
        }

        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles ="Admin")]
        public async Task<IActionResult> DeleteConfirm(string Id)
        {
            var instructor = await _context.Instructors
            .FirstOrDefaultAsync(m => m.Id == Id);
            if (instructor != null)
            {
                _context.Instructors.Remove(instructor);
                await _context.SaveChangesAsync();


            }
            return RedirectToAction(nameof(Index));
        }
        
        private bool InstructorExists(string Id)
        {

            return _context.Instructors.Any(e => e.Id == Id);
        }

    }
}
