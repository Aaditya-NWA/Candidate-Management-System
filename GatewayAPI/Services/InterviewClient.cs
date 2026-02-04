using System.Net.Http.Json;
using GatewayAPI.DTOs.Interviews;

namespace GatewayAPI.Services;

public class InterviewClient
{
    private readonly HttpClient _http;

    public InterviewClient(HttpClient http)
    {
        _http = http;
    }

    public Task<HttpResponseMessage> CreateAsync(CreateInterviewRequest request)
    {
        return _http.PostAsJsonAsync("/api/interviews", request);
    }

    public Task<HttpResponseMessage> GetByCandidateAsync(int candidateId)
    {
        return _http.GetAsync($"/api/interviews/candidate/{candidateId}");
    }
}
