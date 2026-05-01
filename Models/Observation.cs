using System;

namespace EcoData_Manager.Models
{
    public class Observation
    {
        public int ObservationId { get; set; }
        public int SpeciesId { get; set; }
        public string Location { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
    }
}
