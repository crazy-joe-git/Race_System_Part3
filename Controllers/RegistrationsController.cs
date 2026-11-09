using Microsoft.AspNetCore.Mvc;
using Race_System_2_.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Race_System_2_.Controllers
{
    public class RegistrationsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public RegistrationsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: /Registrations/MyEvents (Participant Enrolled Races View)
        public async Task<IActionResult> MyEvents()
        {
            var registrations = new List<RegistrationViewModel>();
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7028";

            var token = HttpContext.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                var response = await client.GetAsync($"{baseUrl}/api/RegistrationsApi");
                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync();
                    var fetched = await JsonSerializer.DeserializeAsync<List<RegistrationViewModel>>(
                        responseStream,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    if (fetched != null) registrations = fetched;
                }
            }
            catch (Exception ex)
            {
                ViewBag.ApiError = $"Unable to load registrations: {ex.Message}";
            }

            return View(registrations);
        }

        // Alias Index to MyEvents so both URLs resolve correctly
        public async Task<IActionResult> Index() => await MyEvents();

        // POST: /Registrations/Enrol
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(int eventId, int categoryId = 1)
        {
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
                    eventRaceID = eventId,
                    categoryID = categoryId,
                    reg_Date = DateTime.UtcNow,
                    paymentStatus = "Confirmed",
                    bidNumber = new Random().Next(100, 999)
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync($"{baseUrl}/api/RegistrationsApi", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Successfully enrolled in the race event!";
                    return RedirectToAction(nameof(MyEvents));
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = !string.IsNullOrWhiteSpace(errorMsg)
                        ? $"Enrolment Error: {errorMsg}"
                        : "Could not complete enrolment. You may already be enrolled.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Connection error: {ex.Message}";
            }

            return RedirectToAction("Index", "Events");
        }
    }
}