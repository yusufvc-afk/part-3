using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

public class AccountController : Controller
{
    private readonly RaceDayApiClient _api;
    public AccountController(RaceDayApiClient api) => _api = api;

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        var result = await _api.LoginAsync(email, password);
        if (!result.Ok || string.IsNullOrWhiteSpace(result.Cookie))
        {
            ModelState.AddModelError("", "Login failed. Check your email and password.");
            return View();
        }

        using var json = JsonDocument.Parse(result.Message);
        var data = json.RootElement;
        HttpContext.Session.SetString("ApiAuthCookie", result.Cookie);
        if (data.TryGetProperty("userId", out var userId))
            HttpContext.Session.SetString("UserId", userId.ToString());
        // API login response must be extended to return role and display name,
        // or MVC should request GET /api/profile/me immediately after storing the cookie.
        var profile = await _api.GetAsync<JsonElement>("api/profile/me");
        if (profile.ValueKind == JsonValueKind.Object)
        {
            if (profile.TryGetProperty("role", out var role))
                HttpContext.Session.SetString("Role", role.GetString() ?? "");
            if (profile.TryGetProperty("firstName", out var first))
                HttpContext.Session.SetString("FirstName", first.GetString() ?? "");
        }
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _api.PostAsync("api/auth/register", model);
        if (!result.Ok)
        {
            ModelState.AddModelError("", "Registration was not successful. " + result.Message);
            return View(model);
        }
        TempData["Message"] = "Registration successful. Please sign in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
