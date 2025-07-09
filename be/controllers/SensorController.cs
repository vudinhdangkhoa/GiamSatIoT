using be.Models;
using be.Services;
using Microsoft.AspNetCore.Mvc;

namespace be.Controllers;

[ApiController]
[Route("api")]
public class SensorController : ControllerBase
{
    private readonly ICsvService _csvService;
    private readonly ISensorService _sensorService;
    private readonly ILogger<SensorController> _logger;

    public SensorController(ICsvService csvService, ISensorService sensorService, ILogger<SensorController> logger)
    {
        _csvService = csvService;
        _sensorService = sensorService;
        _logger = logger;
    }

    [HttpPost("upload-csv")]
    public async Task<IActionResult> UploadCsv(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file uploaded" });
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { success = false, message = "Invalid file format. Please upload a CSV file." });
            }

            using var stream = file.OpenReadStream();
            var csvData = await _csvService.ReadCsvFromStreamAsync(stream);

            if (!csvData.Any())
            {
                return BadRequest(new { success = false, message = "CSV file is empty or has invalid format" });
            }

            // Tạo hoặc cập nhật sensor nếu chưa tồn tại
            var sensorIds = csvData.Select(cd => cd.SensorId).Distinct();
            foreach (var sensorId in sensorIds)
            {
                var sampleData = csvData.First(cd => cd.SensorId == sensorId);
                await _sensorService.CreateOrUpdateSensorAsync(sensorId, sampleData.Unit);
            }

            var newRecordsCount = await _csvService.ProcessAndSaveDataAsync(csvData);

            return Ok(new { 
                success = true, 
                message = $"Successfully processed {newRecordsCount} new records from {csvData.Count} total records",
                totalRecords = csvData.Count,
                newRecords = newRecordsCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading CSV file");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [HttpPost("set-threshold")]
    public async Task<IActionResult> SetThreshold([FromBody] SetThresholdRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SensorId) || request.Threshold <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid sensor ID or threshold value" });
            }

            var result = await _sensorService.UpdateThresholdAsync(request.SensorId, request.Threshold);
            
            if (!result)
            {
                return NotFound(new { success = false, message = "Sensor not found" });
            }

            return Ok(new { success = true, message = "Threshold updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting threshold");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    // [HttpPost("start-reading")]
    // public async Task<IActionResult> StartReading([FromBody] StartReadingRequest request)
    // {
    //     try
    //     {
    //         if (string.IsNullOrEmpty(request.FolderPath))
    //         {
    //             return BadRequest(new { success = false, message = "Folder path is required" });
    //         }

    //         var result = await _csvService.StartPeriodicScanning(request.FolderPath, request.IntervalMinutes);
            
    //         if (!result)
    //         {
    //             return BadRequest(new { success = false, message = "Invalid folder path" });
    //         }

    //         return Ok(new { success = true, message = $"Started periodic scanning every {request.IntervalMinutes} minutes" });
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error starting periodic reading");
    //         return StatusCode(500, new { success = false, message = "Internal server error" });
    //     }
    // }

    // [HttpPost("stop-reading")]
    // public async Task<IActionResult> StopReading()
    // {
    //     try
    //     {
    //         await _csvService.StopPeriodicScanning();
    //         return Ok(new { success = true, message = "Stopped periodic scanning" });
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error stopping periodic reading");
    //         return StatusCode(500, new { success = false, message = "Internal server error" });
    //     }
    // }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestData([FromQuery] int limit = 50)
    {
        try
        {
            var data = await _sensorService.GetLatestDataAsync(limit);
            return Ok(new { success = true, data = data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest data");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [HttpGet("status/alert")]
    public async Task<IActionResult> GetAlertStatus()
    {
        try
        {
            var hasAlerts = await _sensorService.HasActiveAlertsAsync();
            var alertData = await _sensorService.GetAlertDataAsync();
            
            return Ok(new { 
                success = true, 
                hasAlerts = hasAlerts,
                alertCount = alertData.Count,
                alerts = alertData.Take(50) // Chỉ trả về 10 alerts mới nhất
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting alert status");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [HttpGet("data/all")]
    public async Task<IActionResult> GetAllData()
    {
        try
        {
            var data = await _sensorService.GetAllDataAsync();
            return Ok(new { success = true, data = data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all data");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [HttpGet("sensors")]
    public async Task<IActionResult> GetSensors()
    {
        try
        {
            var sensors = await _sensorService.GetAllSensorsAsync();
            return Ok(new { success = true, data = sensors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sensors");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }
}

// Request models
public class SetThresholdRequest
{
    public string SensorId { get; set; } = string.Empty;
    public double Threshold { get; set; }
}

public class StartReadingRequest
{
    public string FolderPath { get; set; } = string.Empty;
    public int IntervalMinutes { get; set; } = 15;
}