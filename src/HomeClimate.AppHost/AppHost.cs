var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddPostgres("timescale")
    .WithImage("timescale/timescaledb", "2.32.1-pg18")
    .AddDatabase("climate");

builder.AddDotnetProject("api", "../HomeClimate.Api/HomeClimate.Api.csproj")
    .WithHttpHealthCheck("/health")
    .WithReference(db)
    .WaitFor(db);

builder.AddDotnetProject("simulator", "../HomeClimate.Simulator/HomeClimate.Simulator.csproj");

builder.Build().Run();