using Microsoft.AspNetCore.Mvc;
using Race_System_2_.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Race_System_2_.Controllers
{
    public class ResultsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ResultsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: /Results/Index (Participant Race History / My Results)
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "Account");

            var results = new List<ResultViewModel>();
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7028";

            var token = HttpContext.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                var response = await client.GetAsync($"{baseUrl}/api/ResultsApi");
                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var fetched = await JsonSerializer.DeserializeAsync<List<ResultViewModel>>(
                        stream,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    if (fetched != null) results = fetched;
                }
            }
            catch (Exception ex)
            {
                ViewBag.ApiError = $"Unable to load results: {ex.Message}";
            }

            return View(results);
        }

        // GET: /Results/Capture (Organiser Results Entry)
        [HttpGet]
        public IActionResult Capture()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Organiser" && role != "Organizer")
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpGet]
        public IActionResult MyResults()
        {
            // Return view showing participant race results aligned with POE requirements
            return View();
        }

        // POST: /Results/Capture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Capture(ResultViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7028";

            var token = HttpContext.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                var payload = new
                {
                    finishTime = model.FinishTime,
                    position = model.Position,
                    registrationID = model.RegistrationId
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync($"{baseUrl}/api/ResultsApi", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Race result captured and published successfully.";
                    return RedirectToAction(nameof(Capture));
                }

                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, !string.IsNullOrWhiteSpace(error) ? error : "Could not publish result.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Connection error: {ex.Message}");
            }

            return View(model);
        }
    }
}