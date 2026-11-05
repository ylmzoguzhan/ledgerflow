var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithImage("pgvector/pgvector", "pg17");

var database = postgres.AddDatabase("ledgerflow");

var simulators = builder.AddProject<Projects.LedgerFlow_Simulators>("simulators");

builder.AddProject<Projects.LedgerFlow_Api>("api")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(simulators);

builder.Build().Run();
