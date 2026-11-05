var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGet("/simulators/status", () => Results.Ok(new { psp = "ready", bank = "ready", fx = "ready" }));

app.Run();
