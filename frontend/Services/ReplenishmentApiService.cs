using System.Net.Http.Json;
using frontend.Models;

namespace frontend.Services;

public class ReplenishmentApiService(HttpClient http)
{
    private void SetRoleHeader(string role)
    {
        http.DefaultRequestHeaders.Remove("X-Simulated-Role");
        http.DefaultRequestHeaders.Add("X-Simulated-Role", role);
    }

    public async Task<PagedResultDto<ReplenishmentRequestDto>?> GetRequestsAsync(int page = 1, int pageSize = 10)
    {
        return await http.GetFromJsonAsync<PagedResultDto<ReplenishmentRequestDto>>(
            $"/api/replenishmentrequests?page={page}&pageSize={pageSize}");
    }

    public async Task<ReplenishmentRequestDto?> GetRequestByIdAsync(int id)
    {
        return await http.GetFromJsonAsync<ReplenishmentRequestDto>($"/api/replenishmentrequests/{id}");
    }

    public async Task<HttpResponseMessage> SubmitRequestAsync(int id, string role)
    {
        SetRoleHeader(role);
        return await http.PostAsync($"/api/replenishmentrequests/{id}/submit", null);
    }

    public async Task<HttpResponseMessage> ApproveRequestAsync(int id, string role)
    {
        SetRoleHeader(role);
        return await http.PostAsync($"/api/replenishmentrequests/{id}/approve", null);
    }

    public async Task<HttpResponseMessage> RejectRequestAsync(int id, string reason, string role)
    {
        SetRoleHeader(role);
        var payload = new { Reason = reason };
        return await http.PostAsJsonAsync($"/api/replenishmentrequests/{id}/reject", payload);
    }

    public async Task<HttpResponseMessage> FulfillAsync(int id, object fulfilledItems, string role)
    {
        SetRoleHeader(role);
        return await http.PostAsJsonAsync($"/api/replenishmentrequests/{id}/fulfill", fulfilledItems);
    }

    public async Task<HttpResponseMessage> CreateDraftAsync(ReplenishmentRequestDto request, string role)
    {
        SetRoleHeader(role);
        return await http.PostAsJsonAsync("/api/replenishmentrequests", request);
    }
}
