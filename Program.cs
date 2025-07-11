using System.Net.Http.Headers;
using System.Text;
using JiraCollector.Jira;
using JiraCollector.Projects;
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
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"));

builder.Services
    .AddHttpClient<JiraClient>((sp, http) =>
    {
        var baseUrl = "";
        var username = "";
        var password = "";

        http.BaseAddress = new Uri(baseUrl);

        var authHeaderValue = $"{username}:{password}";
        var authHeaderValueEncoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(authHeaderValue));

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValueEncoded);
    })
    .AddHttpMessageHandler<JiraLoggingDelegatingHandler>();

builder.Services.AddTransient<JiraLoggingDelegatingHandler>();

builder.Services.AddTransient<SyncProjectsCommandHandler>();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.MapGet(
    "api/users",
    async (JiraClient jiraClient, CancellationToken ct) =>
    {
        return await jiraClient.GetUsers(["alenacolour", "ymalakhova"], ct);
    });

app.MapPost(
    "api/projects/rpc/sync",
    async (SyncProjectsCommandHandler handler, CancellationToken ct) =>
    {
        await handler.Handle(new SyncProjectsCommand());
    });

app.Run();
