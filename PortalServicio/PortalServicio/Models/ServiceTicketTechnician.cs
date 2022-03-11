using SQLiteNetExtensions.Attributes;

namespace PortalServicio.Models
{
    public class ServiceTicketTechnician
    {
        [ForeignKey(typeof(ServiceTicket))]
        public int IncidentId { get; set; }
        [ForeignKey(typeof(Technician))]
        public int TechnicianId { get; set; }
    }
}
