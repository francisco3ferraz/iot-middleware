var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddPostgres("timescale")
    .WithImage("timescale/timescaledb", "2.32.1-pg18")
    .AddDatabase("climate");

// Generated once, then kept in the AppHost's user secrets
var sensorPassword = builder.AddParameter("mqtt-sensor-password",
    new GenerateParameterDefault { MinLength = 22, Special = false }, secret: true, persist: true);
var middlewarePassword = builder.AddParameter("mqtt-middleware-password",
    new GenerateParameterDefault { MinLength = 22, Special = false }, secret: true, persist: true);

var mosquitto = builder.AddContainer("mosquitto", "eclipse-mosquitto", "2.1.4-alpine")
    .WithEndpoint(targetPort: 1883, name: "mqtt", scheme: "tcp")
    .WithBindMount("mosquitto", "/mosquitto/config", isReadOnly: true)
    .WithEntrypoint("/bin/sh")
    .WithArgs("/mosquitto/config/entrypoint.sh")
    .WithEnvironment("MQTT_SENSOR_PASSWORD", sensorPassword)
    .WithEnvironment("MQTT_MIDDLEWARE_PASSWORD", middlewarePassword);
var mqtt = mosquitto.GetEndpoint("mqtt");

builder.AddDotnetProject("api", "../HomeClimate.Api/HomeClimate.Api.csproj")
    .WithHttpHealthCheck("/health")
    .WithReference(db)
    .WaitFor(db)
    .WithEnvironment("Mqtt__Host", mqtt.Property(EndpointProperty.Host))
    .WithEnvironment("Mqtt__Port", mqtt.Property(EndpointProperty.Port))
    .WithEnvironment("Mqtt__Username", "middleware")
    .WithEnvironment("Mqtt__Password", middlewarePassword)
    .WaitFor(mosquitto);

builder.AddDotnetProject("simulator", "../HomeClimate.Simulator/HomeClimate.Simulator.csproj");

builder.Build().Run();