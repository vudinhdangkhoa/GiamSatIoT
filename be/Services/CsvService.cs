using System.Globalization;
using System.Text;
using be.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;

namespace be.Services;

public interface ICsvService
{
    Task<List<SensorDataCsvModel>> ReadCsvFileAsync(string filePath);
    Task<List<SensorDataCsvModel>> ReadCsvFromStreamAsync(Stream stream);
    Task<int> ProcessAndSaveDataAsync(List<SensorDataCsvModel> csvData);
    Task<bool> StartPeriodicScanning(string folderPath, int intervalMinutes = 15);
    Task StopPeriodicScanning();
}

public class CsvService : ICsvService
{
    private readonly MyDbContext _context;
    private readonly ILogger<CsvService> _logger;
    private Timer? _scanTimer;
    private string? _currentScanFolder;

    public CsvService(MyDbContext context, ILogger<CsvService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<SensorDataCsvModel>> ReadCsvFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            using var reader = new StreamReader(filePath, Encoding.UTF8);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            var records = csv.GetRecords<SensorDataCsvModel>().ToList();
            _logger.LogInformation($"Read {records.Count} records from {filePath}");
            
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error reading CSV file: {filePath}");
            throw;
        }
    }

    public async Task<List<SensorDataCsvModel>> ReadCsvFromStreamAsync(Stream stream)
    {
        try
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            var records = csv.GetRecords<SensorDataCsvModel>().ToList();
            _logger.LogInformation($"Read {records.Count} records from uploaded file");
            
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading CSV from stream");
            throw;
        }
    }

    public async Task<int> ProcessAndSaveDataAsync(List<SensorDataCsvModel> csvData)
    {
        if (!csvData.Any())
        {
            return 0;
        }

        var newRecordsCount = 0;
        
        foreach (var csvRecord in csvData)
        {
            // Kiểm tra xem record đã tồn tại chưa
            var existingRecord = await _context.SensorData
                .FirstOrDefaultAsync(sd => sd.SensorId == csvRecord.SensorId 
                                         && sd.Timestamp == csvRecord.Timestamp);

            if (existingRecord != null)
            {
                continue; // Bỏ qua record đã tồn tại
            }

            // Lấy thông tin sensor để kiểm tra ngưỡng
            var sensor = await _context.Sensors
                .FirstOrDefaultAsync(s => s.SensorId == csvRecord.SensorId && s.IsActive);

            bool isAlert = false;
            if (sensor != null && csvRecord.Value > sensor.Threshold)
            {
                isAlert = true;
                _logger.LogWarning($"Alert! Sensor {csvRecord.SensorId} value {csvRecord.Value} exceeds threshold {sensor.Threshold}");
            }

            // Tạo record mới
            var sensorData = new SensorDatum
            {
                Timestamp = csvRecord.Timestamp,
                SensorId = csvRecord.SensorId,
                Value = csvRecord.Value,
                Unit = csvRecord.Unit,
                IsAlert = isAlert,
                CreatedAt = DateTime.Now
            };

            _context.SensorData.Add(sensorData);
            newRecordsCount++;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Saved {newRecordsCount} new records to database");
        
        return newRecordsCount;
    }

    public async Task<bool> StartPeriodicScanning(string folderPath, int intervalMinutes = 15)
    {
        if (!Directory.Exists(folderPath))
        {
            _logger.LogError($"Folder not found: {folderPath}");
            return false;
        }

        _currentScanFolder = folderPath;
        var interval = TimeSpan.FromMinutes(intervalMinutes);

        _scanTimer = new Timer(async _ => await ScanFolderAsync(), null, TimeSpan.Zero, interval);
        
        _logger.LogInformation($"Started periodic scanning of folder: {folderPath} every {intervalMinutes} minutes");
        return true;
    }

    public async Task StopPeriodicScanning()
    {
        _scanTimer?.Dispose();
        _scanTimer = null;
        _currentScanFolder = null;
        _logger.LogInformation("Stopped periodic scanning");
    }

    private async Task ScanFolderAsync()
    {
        if (string.IsNullOrEmpty(_currentScanFolder))
            return;

        try
        {
            var csvFiles = Directory.GetFiles(_currentScanFolder, "*.csv");
            
            foreach (var csvFile in csvFiles)
            {
                try
                {
                    var csvData = await ReadCsvFileAsync(csvFile);
                    await ProcessAndSaveDataAsync(csvData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing file: {csvFile}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error scanning folder: {_currentScanFolder}");
        }
    }
}