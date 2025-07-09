using be.Models;
using Microsoft.EntityFrameworkCore;

namespace be.Services;

public interface ISensorService
{
    Task<List<Sensor>> GetAllSensorsAsync();
    Task<Sensor?> GetSensorByIdAsync(string sensorId);
    Task<bool> UpdateThresholdAsync(string sensorId, double threshold);
    Task<List<SensorDatum>> GetLatestDataAsync(int limit = 50);
    Task<List<SensorDatum>> GetAllDataAsync();
    Task<bool> HasActiveAlertsAsync();
    Task<List<SensorDatum>> GetAlertDataAsync();
    Task<Sensor> CreateOrUpdateSensorAsync(string sensorId, string unit, string? description = null);
}

public class SensorService : ISensorService
{
    private readonly MyDbContext _context;
    private readonly ILogger<SensorService> _logger;

    public SensorService(MyDbContext context, ILogger<SensorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Sensor>> GetAllSensorsAsync()
    {
        return await _context.Sensors
            .Where(s => s.IsActive)
            .Select(s => new Sensor
            {
                Id = s.Id,
                SensorId = s.SensorId,
                Threshold = s.Threshold,
                Unit = s.Unit,
                Description = s.Description,
                IsActive = s.IsActive
            })
            .ToListAsync();
    }

    public async Task<Sensor?> GetSensorByIdAsync(string sensorId)
    {
        return await _context.Sensors
            .FirstOrDefaultAsync(s => s.SensorId == sensorId && s.IsActive);
    }

    public async Task<bool> UpdateThresholdAsync(string sensorId, double threshold)
    {
        var sensor = await _context.Sensors
            .FirstOrDefaultAsync(s => s.SensorId == sensorId);

        if (sensor == null)
        {
            _logger.LogWarning($"Sensor not found: {sensorId}");
            return false;
        }

        sensor.Threshold = threshold;

        // Lấy tất cả các bản ghi sensor_data liên quan
        var sensorDataRecords = await _context.SensorData
            .Where(sd => sd.SensorId == sensorId)
            .ToListAsync();

        foreach (var data in sensorDataRecords)
        {
            data.IsAlert = data.Value > threshold;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Updated threshold for sensor {sensorId} to {threshold} and re-evaluated {sensorDataRecords.Count} data records.");
        return true;
    }


    public async Task<List<SensorDatum>> GetLatestDataAsync(int limit = 50)
    {
        return await _context.SensorData
            .OrderByDescending(sd => sd.Timestamp)
            .Take(limit)
            .Select(sd => new SensorDatum
            {
                Id = sd.Id,
                Timestamp = sd.Timestamp,
                SensorId = sd.SensorId,
                Value = sd.Value,
                Unit = sd.Unit,
                IsAlert = sd.IsAlert,
                CreatedAt = sd.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<SensorDatum>> GetAllDataAsync()
    {
        return await _context.SensorData
            .OrderByDescending(sd => sd.Timestamp)
            .Select(sd => new SensorDatum
            {
                Id = sd.Id,
                Timestamp = sd.Timestamp,
                SensorId = sd.SensorId,
                Value = sd.Value,
                Unit = sd.Unit,
                IsAlert = sd.IsAlert,
                CreatedAt = sd.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<bool> HasActiveAlertsAsync()
    {
        return await _context.SensorData
            .AnyAsync(sd => sd.IsAlert && sd.Timestamp >= DateTime.Now.AddHours(-1));
    }

    public async Task<List<SensorDatum>> GetAlertDataAsync()
    {
        return await _context.SensorData
            .Where(sd => sd.IsAlert)
            .OrderByDescending(sd => sd.Timestamp)
            .Take(100)
            .Select(sd => new SensorDatum
            {
                Id = sd.Id,
                Timestamp = sd.Timestamp,
                SensorId = sd.SensorId,
                Value = sd.Value,
                Unit = sd.Unit,
                IsAlert = sd.IsAlert,
                CreatedAt = sd.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<Sensor> CreateOrUpdateSensorAsync(string sensorId, string unit, string? description = null)
    {
        var existingSensor = await _context.Sensors
            .FirstOrDefaultAsync(s => s.SensorId == sensorId);

        if (existingSensor != null)
        {
            existingSensor.Unit = unit;
            if (!string.IsNullOrEmpty(description))
            {
                existingSensor.Description = description;
            }
            await _context.SaveChangesAsync();
            return existingSensor;
        }

        var newSensor = new Sensor
        {
            SensorId = sensorId,
            Unit = unit,
            Description = description ?? $"Sensor {sensorId}",
            Threshold = 100.0, // Default threshold
            IsActive = true
        };

        _context.Sensors.Add(newSensor);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Created new sensor: {sensorId}");
        return newSensor;
    }
}