using System;
using System.Collections.Generic;

namespace be.Models;

public partial class Sensor
{
    public int Id { get; set; }

    public string SensorId { get; set; } = null!;

    public double Threshold { get; set; }

    public string Unit { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<SensorDatum> SensorData { get; set; } = new List<SensorDatum>();
}
