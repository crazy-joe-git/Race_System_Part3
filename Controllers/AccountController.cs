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

        [HttpGet]
        public IActionResult Register() => View();

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

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Local organiser shortcut override for local testing
            if (!string.IsNullOrEmpty(model.Email) && model.Email.Contains("@organiser.com"))
            {
                var derivedName = model.Email.Split('@')[0];
                var formattedName = char.ToUpper(derivedName[0]) + derivedName.Substring(1);

                HttpContext.Session.SetString("UserRole", "Organiser");
                HttpContext.Session.SetString("UserName", formattedName);
                HttpContext.Session.SetString("UserEmail", model.Email);
                HttpContext.Session.SetString("JWTToken", "local-test-token");

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
                    string userName = root.TryGetProperty("fullName", out var nameProp) ? nameProp.GetString() ?? model.Email.Split('@')[0] : model.Email.Split('@')[0];

                    HttpContext.Session.SetString("JWTToken", token);
                    HttpContext.Session.SetString("UserRole", role);
                    HttpContext.Session.SetString("UserEmail", model.Email);
                    HttpContext.Session.SetString("UserName", userName);

                    return RedirectToAction("Index", "Events");
                }

                ModelState.AddModelError(string.Empty, "Invalid login credentials.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Connection error: {ex.Message}");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}