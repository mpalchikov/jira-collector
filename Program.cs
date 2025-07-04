using System.Net.Http.Headers;
using System.Text;
using JiraCollector.Jira;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog(
    (services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

builder.Services.AddHttpClient<JiraClient>((sp, http) =>
{
    var baseUrl = "";
    var username = "";
    var password = "";


    http.BaseAddress = new Uri(baseUrl);

    var authHeaderValue = $"{username}:{password}";
    var authHeaderValueEncoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(authHeaderValue));

    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValueEncoded);
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.MapGet("/", async (JiraClient jiraClient, CancellationToken ct) =>
{
    return await jiraClient.GetUsers([], ct);
});

app.Run();
