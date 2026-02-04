using System.Net.Http.Json;
using GatewayAPI.DTOs.Candidates;

namespace GatewayAPI.Services;

public class CandidateClient
{
    private readonly HttpClient _http;

    public CandidateClient(HttpClient http)
    {
        _http = http;
    }

    public Task<HttpResponseMessage> CreateAsync(CreateCandidateRequest request)
    {   
        return _http.PostAsJsonAsync("/api/candidates", request);
    }

    public Task<HttpResponseMessage> GetAllAsync()
    {
        return _http.GetAsync("/api/candidates");
    }
}
