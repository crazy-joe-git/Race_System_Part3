using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace Race_System_2_.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        private async Task<IdentityUser> GetCurrentUserAsync()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var userName = HttpContext.Session.GetString("UserName");

            IdentityUser? user = null;
            if (!string.IsNullOrEmpty(email))
            {
                user = await _userManager.FindByEmailAsync(email);
            }
            if (user == null && !string.IsNullOrEmpty(userName))
            {
                user = await _userManager.FindByNameAsync(userName);
            }

            // Fallback for local test bypass accounts (like organizer shortcuts) that aren't in the DB
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = userName ?? email ?? "Organiser",
                    Email = email ?? "organiser@test.com",
                    PhoneNumber = string.Empty
                };
            }

            return user;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "Account");

            var user = await GetCurrentUserAsync();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string phoneNumber, IFormFile profilePicture)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "Account");

            var email = HttpContext.Session.GetString("UserEmail");
            var user = !string.IsNullOrEmpty(email) ? await _userManager.FindByEmailAsync(email) : null;

            if (user != null)
            {
                user.PhoneNumber = phoneNumber;
                await _userManager.UpdateAsync(user);
            }

            if (profilePicture != null && profilePicture.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await profilePicture.CopyToAsync(memoryStream);
                    var base64 = Convert.ToBase64String(memoryStream.ToArray());
                    var imgSrc = $"data:{profilePicture.ContentType};base64,{base64}";
                    HttpContext.Session.SetString("UserProfileImage", imgSrc);
                }
            }

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "Account");

            var email = HttpContext.Session.GetString("UserEmail");
            var user = !string.IsNullOrEmpty(email) ? await _userManager.FindByEmailAsync(email) : null;

            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Password changed successfully.";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Password changes require a registered database account.");
            }

            var currentUser = await GetCurrentUserAsync();
            return View("Index", currentUser);
        }
    }
}