using Edu_Project.Data;
using Edu_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Edu_Project.Controllers
{
    [Authorize]
    public class InstructorController : Controller
    {
        private readonly Context _context;
        private readonly UserManager<User> _userManager;

        public InstructorController(
            Context context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Index()
        {
            var currentUserId =
                _userManager.GetUserId(User);

            if (User.IsInRole("Admin"))
            {
                var instructors =
                    await _context.Instructors
                        .ToListAsync();

                ViewBag.Courses =
                    await _context.Courses
                        .Include(c => c.Instructor)
                        .Include(c => c.Category)
                        .ToListAsync();

                ViewBag.Categories =
                    await _context.Categories
                        .ToListAsync();

                return View(instructors);
            }

            var currentInstructor =
                await _context.Instructors
                    .Where(i =>
                        i.Id == currentUserId)
                    .ToListAsync();

            ViewBag.Courses =
                await _context.Courses
                    .Where(c =>
                        c.InstructorId ==
                        currentUserId)
                    .Include(c => c.Instructor)
                    .Include(c => c.Category)
                    .ToListAsync();

            var instructor =
                await _context.Instructors
                    .FirstOrDefaultAsync(
                        i => i.Id ==
                        currentUserId);

            if (instructor == null)
            {
                return NotFound();
            }

            var specialization =
                instructor.Specialization
                    ?.Trim()
                    .ToLower();

            ViewBag.Categories =
                await _context.Categories
                    .Where(c =>
                        c.Name
                            .Trim()
                            .ToLower() ==
                        specialization)
                    .ToListAsync();

            return View(currentInstructor);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var instructors =
                await _context.Instructors
                    .Include(i => i.courses)
                    .OrderBy(i => i.UserName)
                    .ToListAsync();

            return View(instructors);
        }

        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Details(
            string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor =
                await _context.Instructors
                    .Include(i => i.courses)
                    .Include(i => i.lessons)
                    .Include(i => i.Exams)
                    .FirstOrDefaultAsync(
                        i => i.Id == id);

            if (instructor == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Instructor"))
            {
                var currentUserId =
                    _userManager.GetUserId(User);

                if (currentUserId != id)
                {
                    return Forbid();
                }
            }

            return View(instructor);
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
            Instructor instructor,
            string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "password",
                    "Password is required.");
            }

            if (string.IsNullOrWhiteSpace(
                    instructor.Email))
            {
                ModelState.AddModelError(
                    "Email",
                    "Email is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(instructor);
            }

            var existingUser =
                await _userManager.FindByEmailAsync(
                    instructor.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "This email is already registered.");

                return View(instructor);
            }

            instructor.UserName =
                instructor.Email;

            instructor.EmailConfirmed =
                true;

            var result =
                await _userManager.CreateAsync(
                    instructor,
                    password);

            if (!result.Succeeded)
            {
                foreach (var error
                         in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View(instructor);
            }

            await _userManager.AddToRoleAsync(
                instructor,
                "Instructor");

            return RedirectToAction(
                nameof(Manage));
        }

        [Authorize(Roles = "Admin,Instructor")]
        [HttpGet]
        public async Task<IActionResult> Edit(
            string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor =
                await _context.Instructors
                    .FindAsync(id);

            if (instructor == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Instructor"))
            {
                var currentUserId =
                    _userManager.GetUserId(User);

                if (currentUserId !=
                    instructor.Id)
                {
                    return Forbid();
                }
            }

            return View(instructor);
        }

        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            Instructor instructor)
        {
            if (id != instructor.Id)
            {
                return NotFound();
            }

            var instructorInDb =
                await _context.Instructors
                    .FindAsync(id);

            if (instructorInDb == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Instructor"))
            {
                var currentUserId =
                    _userManager.GetUserId(User);

                if (currentUserId != id)
                {
                    return Forbid();
                }
            }

            instructorInDb.UserName =
                instructor.UserName;

            instructorInDb.Email =
                instructor.Email;

            instructorInDb.Specialization =
                instructor.Specialization;

            if (!string.IsNullOrWhiteSpace(
                    instructor.ProfileImg))
            {
                instructorInDb.ProfileImg =
                    instructor.ProfileImg;
            }

            var result =
                await _userManager.UpdateAsync(
                    instructorInDb);

            if (!result.Succeeded)
            {
                foreach (var error
                         in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View(instructor);
            }

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(
                    nameof(Manage));
            }

            return RedirectToAction(
                nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(
            string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor =
                await _context.Instructors
                    .FirstOrDefaultAsync(
                        i => i.Id == id);

            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            DeleteConfirm(string id)
        {
            var instructor =
                await _context.Instructors
                    .FirstOrDefaultAsync(
                        i => i.Id == id);

            if (instructor == null)
            {
                return NotFound();
            }

            var result =
                await _userManager.DeleteAsync(
                    instructor);

            if (!result.Succeeded)
            {
                foreach (var error
                         in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View(
                    "Delete",
                    instructor);
            }

            return RedirectToAction(
                nameof(Manage));
        }
    }
}