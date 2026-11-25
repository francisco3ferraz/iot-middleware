var builder = DistributedApplication.CreateBuilder(args);

builder.AddDotnetProject("api", "../HomeClimate.Api/HomeClimate.Api.csproj")
    .WithHttpHealthCheck("/health");

builder.AddDotnetProject("simulator", "../HomeClimate.Simulator/HomeClimate.Simulator.csproj");

builder.Build().Run();