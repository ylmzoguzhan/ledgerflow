using LedgerFlow.Modules.Ledger;
using LedgerFlow.Modules.Payments;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddPaymentsModule();
builder.Services.AddLedgerModule();

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();

public partial class Program;
