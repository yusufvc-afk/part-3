using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

public class HomeController : Controller
{
    private readonly RaceDayApiClient _api;
    public HomeController(RaceDayApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var events = await _api.GetAsync<List<JsonElement>>("api/events") ?? new();
        return View(events);
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _api.GetAsync<JsonElement>($"api/events/{id}");
        if (item.ValueKind != JsonValueKind.Object) return NotFound();
        ViewBag.Categories = await _api.GetAsync<List<JsonElement>>($"api/events/{id}/categories") ?? new();
        return View(item);
    }
}
