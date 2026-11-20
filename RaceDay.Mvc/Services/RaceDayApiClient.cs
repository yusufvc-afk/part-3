using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public sealed class RaceDayApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _context;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    { PropertyNameCaseInsensitive = true };

    public RaceDayApiClient(HttpClient http, IHttpContextAccessor context)
    { _http = http; _context = context; }

    private HttpRequestMessage Create(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);
        var cookie = _context.HttpContext?.Session.GetString("ApiAuthCookie");
        if (!string.IsNullOrWhiteSpace(cookie))
            request.Headers.Add("Cookie", cookie);
        return request;
    }

    public async Task<T?> GetAsync<T>(string path)
    {
        using var request = Create(HttpMethod.Get, path);
        using var response = await _http.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.Unauthorized) return default;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    public async Task<(bool Ok, string Message)> PostAsync<T>(string path, T body)
    {
        using var request = Create(HttpMethod.Post, path);
        request.Content = JsonContent.Create(body);
        using var response = await _http.SendAsync(request);
        var message = await response.Content.ReadAsStringAsync();
        return (response.IsSuccessStatusCode, message);
    }

    public async Task<(bool Ok, string Message)> PutAsync<T>(string path, T body)
    {
        using var request = Create(HttpMethod.Put, path);
        request.Content = JsonContent.Create(body);
        using var response = await _http.SendAsync(request);
        return (response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
    }

    public async Task<(bool Ok, string Message)> DeleteAsync(string path)
    {
        using var request = Create(HttpMethod.Delete, path);
        using var response = await _http.SendAsync(request);
        return (response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
    }

    public async Task<(bool Ok, string Message, string? Cookie)> LoginAsync(string email, string password)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login")
        { Content = JsonContent.Create(new { email, password }) };
        using var response = await _http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        var setCookie = response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.FirstOrDefault()?.Split(';')[0] : null;
        return (response.IsSuccessStatusCode, body, setCookie);
    }

    public async Task<(bool Ok, string Message)> UploadAsync(string path, IFormFile file)
    {
        using var request = Create(HttpMethod.Post, path);
        using var multipart = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        using var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        multipart.Add(content, "file", Path.GetFileName(file.FileName));
        request.Content = multipart;
        using var response = await _http.SendAsync(request);
        return (response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
    }
}
