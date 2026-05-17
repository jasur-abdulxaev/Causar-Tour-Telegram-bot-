using System.Net.Http.Json;
using AdminPanel.Web.Models;

namespace AdminPanel.Web.Services;

public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<AdResponse>> GetAdsAsync()
    {
        return await _http.GetFromJsonAsync<List<AdResponse>>("api/advertisements") ?? new();
    }

    public async Task<AdResponse?> GetAdAsync(int id)
    {
        return await _http.GetFromJsonAsync<AdResponse>($"api/advertisements/{id}");
    }

    public async Task<AdResponse?> CreateAdAsync(CreateAdRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/advertisements", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AdResponse>();
    }

    public async Task<AdResponse?> UpdateAdAsync(int id, UpdateAdRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/advertisements/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AdResponse>();
    }

    public async Task DeleteAdAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/advertisements/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<AnalyticsResponse>> GetAnalyticsSummaryAsync()
    {
        return await _http.GetFromJsonAsync<List<AnalyticsResponse>>("api/analytics") ?? new();
    }
}
