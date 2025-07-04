using Microsoft.AspNetCore.WebUtilities;

namespace JiraCollector.Jira;

public class JiraClient
{
    private readonly HttpClient _httpClient;

    public JiraClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<Project>> GetProjects(CancellationToken ct)
    {
        var response = await _httpClient.GetAsync("rest/api/3/project", ct);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<Project>>(ct) ?? [];
    }

    public async Task<IReadOnlyCollection<UserAccount>> GetUsers(
        IReadOnlyCollection<string> userKeys,
        CancellationToken ct)
    {
        var uri = "rest/api/3/user/bulk/migration";

        var queryParameters = new Dictionary<string, string?>
        {
            { "maxResults", "100" }
        };

        foreach (var key in userKeys)
        {
            queryParameters.Add("key", key);
        }

        var uriWithQuery = QueryHelpers.AddQueryString(uri, queryParameters);

        var response = await _httpClient.GetAsync(uriWithQuery, ct);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<UserAccount>>(ct) ?? [];
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