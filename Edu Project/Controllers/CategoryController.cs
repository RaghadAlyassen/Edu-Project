using Edu_Project.Data;
using Edu_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Edu_Project.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    public class CategoryController : Controller
    {
        private readonly Context _context;

        public CategoryController(Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories =
                await _context.Categories
                    .Include(c => c.Courses)
                    .ToListAsync();

            return View(categories);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category =
                await _context.Categories
                    .Include(c => c.Courses)
                    .FirstOrDefaultAsync(
                        c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category =
                await _context.Categories
                    .FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            var categoryInDb =
                await _context.Categories
                    .FirstOrDefaultAsync(
                        c => c.Id == id);

            if (categoryInDb == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            categoryInDb.Name =
                category.Name;

            categoryInDb.Description =
                category.Description;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index));
        }
    }
}