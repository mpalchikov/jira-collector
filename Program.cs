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

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.MapGet("/", () =>
{
    return "Hello World!";
});

app.Run();
