using PortalAPI.Contracts;
using PortalServicio.Models;
using System;

namespace PortalServicio.DTO
{
    public class DTO_LegalizationLookUp
    {
        public string LegalizationNumber { get; set; }
        public Guid InternalId { get; set; }
        public Types.SPCLEGALIZATION_TYPE LegalizationType { get; set; }
        public Types.SPCLEGALIZATION_SIGNSTATE SignState { get; set; }
        public decimal MoneyRequested { get; set; }
        public decimal MoneyPaid { get; set; }
        public Currency MoneyCurrency { get; set; }
    }
}
