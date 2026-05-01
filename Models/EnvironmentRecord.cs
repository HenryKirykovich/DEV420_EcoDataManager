using System;

namespace EcoData_Manager.Models
{
    public class EnvironmentRecord
    {
        public int EnvId { get; set; }
        public string Location { get; set; }
        public DateTime Date { get; set; }
        public double? Temperature { get; set; }
        public double? Humidity { get; set; }
        public int? AirQualityIndex { get; set; }
    }
}
