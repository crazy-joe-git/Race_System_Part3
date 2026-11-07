using Microsoft.AspNetCore.Mvc;
using Race_System_2_.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Race_System_2_.Controllers
{
    public class EventsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public EventsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: /Events/Index
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "Account");

            var eventsList = new List<EventViewModel>();
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7028";

            var token = HttpContext.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                var response = await client.GetAsync($"{baseUrl}/api/ApiEvents");
                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync();
                    var fetchedEvents = await JsonSerializer.DeserializeAsync<List<EventViewModel>>(
                        responseStream,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (fetchedEvents != null) eventsList = fetchedEvents;
                }
            }
            catch (Exception ex)
            {
                ViewBag.ApiError = $"Could not connect to backend API ({ex.Message}).";
            }

            return View(eventsList);
        }

        // GET: /Events/Create
        [HttpGet]
        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Organiser" && role != "Organizer")
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel model, IFormFile? bannerImage)
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
                var organizerEmail = HttpContext.Session.GetString("UserEmail") ?? "Organiser";

                // JSON payload structured to match backend API schema exactly
                var apiPayload = new
                {
                    eventName = model.Name,
                    description = model.Description ?? "",
                    eventDate = model.Date,
                    eventLocation = model.Location,
                    distance = model.Distance,
                    eventType = model.EventType ?? "Road",
                    organizerName = organizerEmail
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(apiPayload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync($"{baseUrl}/api/ApiEvents", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Event created and published in real-time!";
                    return RedirectToAction(nameof(Index));
                }

                var apiErrorText = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, !string.IsNullOrWhiteSpace(apiErrorText)
                    ? $"API Error: {apiErrorText}"
                    : "API rejected event creation.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error connecting to API: {ex.Message}");
            }

            return View(model);
        }

        // GET: /Events/Browse
        public async Task<IActionResult> Browse()
        {
            var eventsList = new List<EventViewModel>();
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7028";

            try
            {
                var response = await client.GetAsync($"{baseUrl}/api/ApiEvents");
                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync();
                    var fetchedEvents = await JsonSerializer.DeserializeAsync<List<EventViewModel>>(
                        responseStream,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (fetchedEvents != null) eventsList = fetchedEvents;
                }
            }
            catch (Exception) { }

            return View(eventsList);
        }
    }
}