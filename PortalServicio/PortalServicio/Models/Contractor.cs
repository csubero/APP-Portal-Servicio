using System;
using System.Collections.Generic;
using System.Text;

namespace PortalServicio.Models
{
    public class Contractor
    {
        #region Properties
        public Guid InternalId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Identification { get; set; }
        //public Guid Userid { get; set; }
        #endregion

        #region Constructors
        public Contractor(Guid id, string name, string address, string phone, string identification, Guid sysuser=default(Guid))
        {
            InternalId = id;
            Name = name;
            Address = address;
            Phone = phone;
            Identification = identification;
        }
        #endregion

    }
}
