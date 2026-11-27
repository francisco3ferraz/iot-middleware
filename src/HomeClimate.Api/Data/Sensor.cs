namespace HomeClimate.Api.Data;

public class Sensor
{
    public required string Id { get; set; }
    public required string Model { get; set; }
    public required string Firmware { get; set; }
    public int IntervalSeconds { get; set; }
    public List<SensorMetric> Metrics { get; set; } = [];
}
