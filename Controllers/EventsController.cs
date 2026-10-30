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

        // GET: /Events/Index (Real-time Organiser Dashboard)
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role))
            {
                TempData["ErrorMessage"] = "Please log in to access the organiser dashboard.";
                return RedirectToAction("Login", "Account");
            }

            var eventsList = new List<EventViewModel>();
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7000";

            // Attach JWT Token to authorization header if present
            var token = HttpContext.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                // Fetch real-time events from API
                var response = await client.GetAsync($"{baseUrl}/api/Events");
                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync();
                    var fetchedEvents = await JsonSerializer.DeserializeAsync<List<EventViewModel>>(
                        responseStream,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (fetchedEvents != null)
                    {
                        eventsList = fetchedEvents;
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ApiError = $"Could not connect to backend API ({ex.Message}). Ensure Part 2 API is running.";
            }

            return View(eventsList);
        }

        // GET: /Events/Create
        [HttpGet]
        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Organiser" && role != "Organizer")
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Events/Create (Posts real-time data to backend API)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel model, IFormFile? bannerImage)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7000";

            var token = HttpContext.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                using var multipartContent = new MultipartFormDataContent();

                // Add text fields
                multipartContent.Add(new StringContent(model.Name ?? ""), "Name");
                multipartContent.Add(new StringContent(model.Description ?? ""), "Description");
                multipartContent.Add(new StringContent(model.Date.ToString("o")), "Date");
                multipartContent.Add(new StringContent(model.Location ?? ""), "Location");
                multipartContent.Add(new StringContent(model.Distance ?? ""), "Distance");
                multipartContent.Add(new StringContent(model.EventType ?? ""), "EventType");

                // Add image file if uploaded (for Azure Blob Storage integration on API)
                if (bannerImage != null && bannerImage.Length > 0)
                {
                    var streamContent = new StreamContent(bannerImage.OpenReadStream());
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(bannerImage.ContentType);
                    multipartContent.Add(streamContent, "bannerImage", bannerImage.FileName);
                }

                var response = await client.PostAsync($"{baseUrl}/api/Events", multipartContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Event created and published in real-time!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "API rejected the event creation. Please check required fields.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error connecting to API: {ex.Message}");
            }

            return View(model);
        }

        // GET: /Events/Browse (Real-time Participant Discovery)
        public async Task<IActionResult> Browse()
        {
            var eventsList = new List<EventViewModel>();
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7000";

            try
            {
                var response = await client.GetAsync($"{baseUrl}/api/Events");
                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync();
                    var fetchedEvents = await JsonSerializer.DeserializeAsync<List<EventViewModel>>(
                        responseStream,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (fetchedEvents != null)
                    {
                        eventsList = fetchedEvents;
                    }
                }
            }
            catch (Exception)
            {
                // Silently return empty list if API unreachable
            }

            return View(eventsList);
        }
    }
}