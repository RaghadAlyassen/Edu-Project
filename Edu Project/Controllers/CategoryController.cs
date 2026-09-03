using Edu_Project.Data;
using Edu_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;



namespace Edu_Project.Controllers
{
    public class CategoryController : Controller
    {
        private readonly Context _context;

        public CategoryController(Context context)
        {

            _context = context;
            
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
            .Include(c => c.Instructor)
            .Include(c => c.Courses)
            .ToListAsync();
            return View(categories);

        }


        public async Task<IActionResult> Details(int? Id)
        {

            if (Id == null) return NotFound();

            var Category = await _context.Categories
            .Include(c => c.Instructor)
            .Include(c => c.Courses)
            .FirstOrDefaultAsync(m => m.Id == Id);

            if (Category == null) return NotFound();

            return View(Category);
        }


        [HttpGet]
        public IActionResult Create()
        {

            
            ViewBag.Instructors = new SelectList(_context.Instructors, "Id", "UserName");
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)

        {

            var defaultInstructor = _context.Users.FirstOrDefault();
            if (defaultInstructor != null)
            {
                category.InstructorId = defaultInstructor.Id;
            }



            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        //[Authorize(Roles = "Instructor")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? Id)

        {

            if (Id == null) return NotFound();
            var Category = await _context.Categories.FindAsync(Id);
            if (Category == null) return NotFound();
            var instructorsCount = _context.Instructors.Count();
            System.Diagnostics.Debug.WriteLine("Total Instructors found: " + instructorsCount);

            ViewBag.Instructors = new SelectList(_context.Instructors, "Id", "UserName", Category.InstructorId);
            return View(Category);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  Category category)

        {
            
            if (id != category.Id)
            {
                return NotFound();
            }

            
            System.Diagnostics.Debug.WriteLine("POST InstructorId: " + category.InstructorId);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(e => e.Id ==category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

     
            ViewBag.Instructors = new SelectList(_context.Instructors, "Id", "UserName", category.InstructorId);
            return View(category);
        }


    }
}
