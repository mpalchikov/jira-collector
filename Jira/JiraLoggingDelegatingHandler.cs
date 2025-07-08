using System.Diagnostics;
using System.Text;

namespace JiraCollector.Jira;

public class JiraLoggingDelegatingHandler(ILogger<JiraLoggingDelegatingHandler> logger) : DelegatingHandler
{
    private readonly ILogger<JiraLoggingDelegatingHandler> _logger = logger;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var start = Stopwatch.GetTimestamp();

        try
        {
            var response = await base.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
            {
                var requestId = Guid.NewGuid();
                var content = await response.Content.ReadAsByteArrayAsync(ct);

                _logger.LogInformation(
                    "Success Jira Api call {RequestId}: {StatusCode} {Method} {Uri} in {Elapsed} ms",
                    requestId,
                    (int)response.StatusCode,
                    request.RequestUri,
                    request.Method,
                    Stopwatch.GetElapsedTime(start).Milliseconds);

                _logger.LogDebug(
                    "Jira response: {RequestId} {ResponseContent}",
                    requestId,
                    Encoding.UTF8.GetString(content));                                
            }
            else
            {
                var content = await response.Content.ReadAsByteArrayAsync(ct);

                _logger.LogError(
                    "Failed Jira Api call: {StatusCode} {Method} {Uri} with reason {ErrorReason}",
                    (int)response.StatusCode,
                    request.RequestUri,
                    request.Method,
                    Encoding.UTF8.GetString(content));
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Jira Api Client Call Failed");
            throw;
        }
    }
}
