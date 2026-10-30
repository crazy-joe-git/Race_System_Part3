using Microsoft.AspNetCore.Mvc;
using Race_System_2_.Models;
using System.Net.Http.Headers;
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

        // GET: /RaceCategories/Index (Real-time Categories)
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "Account");

            var categories = new List<CategoryViewModel>();
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7000";

            var token = HttpContext.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                var response = await client.GetAsync($"{baseUrl}/api/RaceCategories");
                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var data = await JsonSerializer.DeserializeAsync<List<CategoryViewModel>>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (data != null) categories = data;
                }
            }
            catch (Exception ex)
            {
                ViewBag.ApiError = $"Unable to load categories: {ex.Message}";
            }

            return View(categories);
        }
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Distance { get; set; } = string.Empty;
        public string AgeGroup { get; set; } = string.Empty;
    }
}