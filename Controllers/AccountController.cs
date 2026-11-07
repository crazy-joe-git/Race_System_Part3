using Microsoft.AspNetCore.Mvc;
using Race_System_2_.Models;
using System.Text;
using System.Text.Json;

namespace Race_System_2_.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7028";

            try
            {
                var payload = new
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password,
                    Role = model.Role
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{baseUrl}/api/AccountApi/register", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Account created successfully! Please log in.";
                    return RedirectToAction(nameof(Login));
                }

                var errorResponse = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, !string.IsNullOrWhiteSpace(errorResponse)
                    ? errorResponse
                    : "Registration failed. Check details or email availability.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Connection error: {ex.Message}");
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Organiser shortcut override for local testing
            if (!string.IsNullOrEmpty(model.Email) && model.Email.Contains("@organiser.com"))
            {
                HttpContext.Session.SetString("UserRole", "Organiser");
                HttpContext.Session.SetString("UserEmail", model.Email);
                return RedirectToAction("Index", "Events");
            }

            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7028";

            try
            {
                var payload = new { Email = model.Email, Password = model.Password };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{baseUrl}/api/AccountApi/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseData);
                    var root = doc.RootElement;

                    string token = root.TryGetProperty("token", out var tokenProp) ? tokenProp.GetString() ?? "" : "";
                    string role = root.TryGetProperty("role", out var roleProp) ? roleProp.GetString() ?? "Participant" : "Participant";

                    HttpContext.Session.SetString("JWTToken", token);
                    HttpContext.Session.SetString("UserRole", role);
                    HttpContext.Session.SetString("UserEmail", model.Email);

                    if (role.Equals("Organiser", StringComparison.OrdinalIgnoreCase) || role.Equals("Organizer", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("Index", "Events");
                    }

                    return RedirectToAction("Browse", "Events");
                }

                ModelState.AddModelError(string.Empty, "Invalid login credentials.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Connection error: {ex.Message}");
            }

            return View(model);
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}