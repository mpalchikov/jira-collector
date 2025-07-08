using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace JiraCollector.Jira;

public class JiraClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public JiraClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public async Task<IReadOnlyCollection<Project>> GetProjects(CancellationToken ct)
    {
        var response = await _httpClient.GetAsync("rest/api/3/project", ct);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsByteArrayAsync(ct);
        string json = Encoding.UTF8.GetString(content);

        return JsonSerializer.Deserialize<IReadOnlyCollection<Project>>(json, _jsonSerializerOptions) ?? [];
    }

    public async Task<IReadOnlyCollection<UserAccount>> GetUsers(
        IReadOnlyCollection<string> userKeys,
        CancellationToken ct)
    {
        var uri = "rest/api/3/user/bulk/migration";

        var queryParameters = new List<KeyValuePair<string, string?>>
        {
            new("maxResults", "100")
        };

        foreach (var key in userKeys)
        {
            queryParameters.Add(new("key", key));
        }

        var uriWithQuery = QueryHelpers.AddQueryString(uri, queryParameters);

        var response = await _httpClient.GetAsync(uriWithQuery, ct);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsByteArrayAsync(ct);
        string json = Encoding.UTF8.GetString(content);

        return JsonSerializer.Deserialize<IReadOnlyCollection<UserAccount>>(json, _jsonSerializerOptions) ?? [];
    }
}

public record Project(
    int Id,
    string Self,
    string Key,
    string Name,
    bool Simplified,
    string Style,
    ProjectCategory ProjectCategory);

public record ProjectCategory(string Name);

public record UserAccount(string Key, string AccountId);