using Edu_Project.Data;
using Edu_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Edu_Project.Controllers
{
    [Authorize(Roles = "Instructor,Admin")]
    public class CourseController : Controller
    {
        private readonly Context _context;
        private readonly UserManager<User> _userManager;

        public CourseController(
            Context context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId =
                _userManager.GetUserId(User);

            if (User.IsInRole("Admin"))
            {
                var allCourses =
                    await _context.Courses
                        .Include(c => c.Instructor)
                        .Include(c => c.Category)
                        .ToListAsync();

                return View(allCourses);
            }

            var courses =
                await _context.Courses
                    .Where(c =>
                        c.InstructorId ==
                        currentUserId)
                    .Include(c => c.Instructor)
                    .Include(c => c.Category)
                    .ToListAsync();

            return View(courses);
        }

        public async Task<IActionResult> Details(
            int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUserId =
                _userManager.GetUserId(User);

            var course =
                await _context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.Category)
                    .Include(c => c.Finalexam)
                    .FirstOrDefaultAsync(
                        c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") &&
                course.InstructorId !=
                currentUserId)
            {
                return Forbid();
            }

            return View(course);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var instructorId =
                _userManager.GetUserId(User);

            if (instructorId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var instructor =
                await _context.Instructors
                    .FirstOrDefaultAsync(
                        i => i.Id ==
                        instructorId);

            if (instructor == null)
            {
                return NotFound();
            }

            var specialization =
                instructor.Specialization
                    ?.Trim()
                    .ToLower();

            var categories =
                await _context.Categories
                    .Where(c =>
                        c.Name
                            .Trim()
                            .ToLower() ==
                        specialization)
                    .ToListAsync();

            ViewBag.Categories =
                new SelectList(
                    categories,
                    "Id",
                    "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Course course)
        {
            var instructorId =
                _userManager.GetUserId(User);

            if (instructorId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var instructor =
                await _context.Instructors
                    .FirstOrDefaultAsync(
                        i => i.Id ==
                        instructorId);

            if (instructor == null)
            {
                return NotFound();
            }

            course.InstructorId =
                instructorId;

            ModelState.Remove(
                nameof(Course.InstructorId));

            var specialization =
                instructor.Specialization
                    ?.Trim()
                    .ToLower();

            var category =
                await _context.Categories
                    .FirstOrDefaultAsync(
                        c =>
                            c.Id ==
                            course.CategoryId &&
                            c.Name
                                .Trim()
                                .ToLower() ==
                            specialization);

            if (category == null)
            {
                ModelState.AddModelError(
                    nameof(Course.CategoryId),
                    "Please select a category that matches your specialization.");
            }

            if (!ModelState.IsValid)
            {
                var categories =
                    await _context.Categories
                        .Where(c =>
                            c.Name
                                .Trim()
                                .ToLower() ==
                            specialization)
                        .ToListAsync();

                ViewBag.Categories =
                    new SelectList(
                        categories,
                        "Id",
                        "Name",
                        course.CategoryId);

                return View(course);
            }

            _context.Courses.Add(course);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(
            int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructorId =
                _userManager.GetUserId(User);

            var course =
                await _context.Courses
                    .FirstOrDefaultAsync(
                        c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") &&
                course.InstructorId !=
                instructorId)
            {
                return Forbid();
            }

            if (User.IsInRole("Admin"))
            {
                var allCategories =
                    await _context.Categories
                        .ToListAsync();

                ViewBag.Categories =
                    new SelectList(
                        allCategories,
                        "Id",
                        "Name",
                        course.CategoryId);

                return View(course);
            }

            var instructor =
                await _context.Instructors
                    .FirstOrDefaultAsync(
                        i => i.Id ==
                        instructorId);

            if (instructor == null)
            {
                return NotFound();
            }

            var specialization =
                instructor.Specialization
                    ?.Trim()
                    .ToLower();

            var categories =
                await _context.Categories
                    .Where(c =>
                        c.Name
                            .Trim()
                            .ToLower() ==
                        specialization)
                    .ToListAsync();

            ViewBag.Categories =
                new SelectList(
                    categories,
                    "Id",
                    "Name",
                    course.CategoryId);

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            var instructorId =
                _userManager.GetUserId(User);

            var courseInDb =
                await _context.Courses
                    .FirstOrDefaultAsync(
                        c => c.Id == id);

            if (courseInDb == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") &&
                courseInDb.InstructorId !=
                instructorId)
            {
                return Forbid();
            }

            if (User.IsInRole("Admin"))
            {
                var categoryExists =
                    await _context.Categories
                        .AnyAsync(
                            c =>
                                c.Id ==
                                course.CategoryId);

                if (!categoryExists)
                {
                    ModelState.AddModelError(
                        nameof(Course.CategoryId),
                        "Please select a valid category.");
                }
            }
            else
            {
                var instructor =
                    await _context.Instructors
                        .FirstOrDefaultAsync(
                            i => i.Id ==
                            instructorId);

                if (instructor == null)
                {
                    return NotFound();
                }

                var specialization =
                    instructor.Specialization
                        ?.Trim()
                        .ToLower();

                var categoryExists =
                    await _context.Categories
                        .AnyAsync(
                            c =>
                                c.Id ==
                                course.CategoryId &&
                                c.Name
                                    .Trim()
                                    .ToLower() ==
                                specialization);

                if (!categoryExists)
                {
                    ModelState.AddModelError(
                        nameof(Course.CategoryId),
                        "Please select a category that matches your specialization.");
                }
            }

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    ViewBag.Categories =
                        new SelectList(
                            await _context.Categories
                                .ToListAsync(),
                            "Id",
                            "Name",
                            course.CategoryId);
                }
                else
                {
                    var instructor =
                        await _context.Instructors
                            .FirstOrDefaultAsync(
                                i => i.Id ==
                                instructorId);

                    var specialization =
                        instructor?
                            .Specialization
                            ?.Trim()
                            .ToLower();

                    var categories =
                        await _context.Categories
                            .Where(c =>
                                c.Name
                                    .Trim()
                                    .ToLower() ==
                                specialization)
                            .ToListAsync();

                    ViewBag.Categories =
                        new SelectList(
                            categories,
                            "Id",
                            "Name",
                            course.CategoryId);
                }

                return View(course);
            }

            courseInDb.Title =
                course.Title;

            courseInDb.Descciption =
                course.Descciption;

            courseInDb.CategoryId =
                course.CategoryId;

            courseInDb.Price =
                course.Price;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index));
        }
    }
}