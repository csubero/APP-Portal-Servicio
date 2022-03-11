using System;

namespace PortalServicio.Models
{
    public class PaymentCondition
    {
        #region Properties
        public Guid InternalId { get; set; }
        public string Description { get; set; }
        #endregion

        #region Constructors
        public PaymentCondition(Guid internalid, string description)
        {
            InternalId = internalid;
            Description = description;
        }
        #endregion
    }
}
