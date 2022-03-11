using SQLiteNetExtensions.Attributes;

namespace PortalServicio.Models
{
    public class IncidentTechnician
    {
        [ForeignKey(typeof(Incident))]
        public int IncidentId { get; set; }
        [ForeignKey(typeof(Technician))]
        public int TechnicianId { get; set; }
    }
}
