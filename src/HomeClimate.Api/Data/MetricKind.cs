namespace HomeClimate.Api.Data;

public class MetricKind
{
    public required string Kind { get; set; }
    public required string Unit { get; set; }
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
}
