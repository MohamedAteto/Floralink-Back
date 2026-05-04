using FloraLink.Domain.Entities;

namespace FloraLink.Application.Services;

/// <summary>
/// Calculates plant health score and status based on sensor readings vs ideal ranges.
/// </summary>
public static class HealthCalculator
{
    public static double Calculate(double moisture, double temperature, PlantType plantType)
    {
        double moistureDeviation = ComputeDeviation(moisture, plantType.MinMoisture, plantType.MaxMoisture);
        double tempDeviation = ComputeDeviation(temperature, plantType.MinTemperature, plantType.MaxTemperature);

        double score = 100 - moistureDeviation - tempDeviation;
        return Math.Max(0, Math.Min(100, score));
    }

    public static string GetStatus(double healthScore) => healthScore switch
    {
        >= 80 => "Happy",
        >= 60 => "OK",
        >= 40 => "Thirsty",
        _ => "Stressed"
    };

    private static double ComputeDeviation(double value, double min, double max)
    {
        if (value < min) return (min - value) / min * 50;
        if (value > max) return (value - max) / max * 50;
        return 0;
    }
}
