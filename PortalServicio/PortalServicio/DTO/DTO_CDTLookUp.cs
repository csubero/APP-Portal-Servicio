using PortalAPI.DTO;
using System;

namespace PortalServicio.DTO
{
    public class DTO_CDTLookUp
    {
        public DTO_ClientPartial Client { get; set; }
        public Guid InternalId { get; set; }
        public string Number { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
