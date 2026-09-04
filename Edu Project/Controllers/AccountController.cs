using Edu_Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Edu_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string email,
            string password,
            string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage =
                    "Please enter all required fields.";

                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.ErrorMessage =
                    "Passwords do not match.";

                return View();
            }

            var existingUser =
                await userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                ViewBag.ErrorMessage =
                    "This email is already registered.";

                return View();
            }

            var student = new Student
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                ProfileImg = "",
                RegistrationDate = DateTime.Now
            };

            var result =
                await userManager.CreateAsync(
                    student,
                    password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(
                    student,
                    "Student");

                await signInManager.SignInAsync(
                    student,
                    isPersistent: false);

                return RedirectToAction(
                    "Index",
                    "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    "",
                    error.Description);
            }

            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null &&
                User.Identity.IsAuthenticated)
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string password,
            bool rememberMe)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage =
                    "Please enter email and password.";

                return View();
            }

            var user =
                await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ViewBag.ErrorMessage =
                    "Invalid email or password.";

                return View();
            }

            var result =
                await signInManager.PasswordSignInAsync(
                    user,
                    password,
                    rememberMe,
                    false);

            if (!result.Succeeded)
            {
                ViewBag.ErrorMessage =
                    "Invalid email or password.";

                return View();
            }

            return RedirectToAction(
                "Index",
                "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();

            return RedirectToAction(
                "Login",
                "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}