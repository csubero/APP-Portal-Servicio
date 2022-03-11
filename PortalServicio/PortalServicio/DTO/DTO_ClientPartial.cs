using System;

namespace PortalAPI.DTO
{
    public class DTO_ClientPartial
    {
        public Guid InternalId { get; set; }
        public Guid PriceList { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
    }
}