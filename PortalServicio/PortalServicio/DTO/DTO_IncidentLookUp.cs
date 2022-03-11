using PortalAPI.Contracts;
using System;

namespace PortalAPI.DTO
{
    public class DTO_IncidentLookUp
    {
        public DTO_ClientPartial Client { get; set; }
        public Guid InternalId { get; set; }
        public string TicketNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public Types.SPCINCIDENT_CONTROLOPTION Control { get; set; }
    }
}