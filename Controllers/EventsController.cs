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
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Organiser" && role != "Organizer")
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7028";

            var token = HttpContext.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                var multiPartContent = new MultipartFormDataContent();

                multiPartContent.Add(new StringContent(model.Name ?? ""), "EventName");
                multiPartContent.Add(new StringContent(model.Description ?? ""), "Description");
                multiPartContent.Add(new StringContent(model.Date.ToString("yyyy-MM-ddTHH:mm:ss")), "EventDate");
                multiPartContent.Add(new StringContent(model.Location ?? ""), "EventLocation");
                multiPartContent.Add(new StringContent(model.Distance.ToString()), "Distance");
                multiPartContent.Add(new StringContent(model.EventType ?? "Road"), "EventType");

                var organizerName = HttpContext.Session.GetString("UserName") ?? HttpContext.Session.GetString("UserEmail") ?? "Organiser";
                multiPartContent.Add(new StringContent(organizerName), "OrganizerName");

                if (bannerImage != null && bannerImage.Length > 0)
                {
                    var fileStreamContent = new StreamContent(bannerImage.OpenReadStream());
                    fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(bannerImage.ContentType);
                    multiPartContent.Add(fileStreamContent, "bannerImage", bannerImage.FileName);
                }

                var response = await client.PostAsync($"{baseUrl}/api/ApiEvents", multiPartContent);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Event published successfully with Azure storage integration.";
                    return RedirectToAction(nameof(Index));
                }

                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, !string.IsNullOrWhiteSpace(error) ? error : "Could not create event.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Connection error: {ex.Message}");
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