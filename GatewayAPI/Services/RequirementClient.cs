using System.Net.Http.Json;
using GatewayAPI.DTOs.Requirements;

namespace GatewayAPI.Services;

public class RequirementClient
{
    private readonly HttpClient _http;

    public RequirementClient(HttpClient http)
    {
        _http = http;
    }

    public Task<HttpResponseMessage> CreateAsync(CreateRequirementRequest request)
    {
        return _http.PostAsJsonAsync("/api/requirements", request);
    }

    public Task<HttpResponseMessage> GetAllAsync()
    {
        return _http.GetAsync("/api/requirements");
    }

    public Task<HttpResponseMessage> GetByIdAsync(int id)
    {
        return _http.GetAsync($"/api/requirements/{id}");
    }
}
