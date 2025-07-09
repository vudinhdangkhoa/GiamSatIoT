using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace be.Models
{
    public class SensorDataCsvModel
    {
        public DateTime Timestamp { get; set; }
        public string SensorId { get; set; } = string.Empty;
        public float Value { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}