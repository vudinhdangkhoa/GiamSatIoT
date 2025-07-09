using System;
using System.Collections.Generic;

namespace be.Models;

public partial class SensorDatum
{
    public int Id { get; set; }

    public DateTime Timestamp { get; set; }

    public string SensorId { get; set; } = null!;

    public double Value { get; set; }

    public string Unit { get; set; } = null!;

    public bool IsAlert { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Sensor Sensor { get; set; } = null!;
}
