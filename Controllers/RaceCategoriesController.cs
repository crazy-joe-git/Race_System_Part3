using Microsoft.AspNetCore.Mvc;
using Race_System_2_.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Race_System_2_.Controllers
{
    public class RaceCategoriesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public RaceCategoriesController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: /RaceCategories/Index
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "Account");

            var categories = new List<RaceCategoryViewModel>();
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7028";

            var token = HttpContext.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                var response = await client.GetAsync($"{baseUrl}/api/RaceCategoriesApi");
                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var fetched = await JsonSerializer.DeserializeAsync<List<RaceCategoryViewModel>>(
                        stream,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    if (fetched != null) categories = fetched;
                }
            }
            catch (Exception ex)
            {
                ViewBag.ApiError = $"Error loading categories: {ex.Message}";
            }

            return View(categories);
        }

        // GET: /RaceCategories/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /RaceCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RaceCategoryViewModel model)
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
                    categoryName = model.CategoryName,
                    distance = model.Distance,
                    category_description = model.CategoryDescription
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync($"{baseUrl}/api/RaceCategoriesApi", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Race category created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, !string.IsNullOrWhiteSpace(error) ? error : "Could not create category.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Connection error: {ex.Message}");
            }

            return View(model);
        }
    }
}