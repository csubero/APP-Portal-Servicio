using System;

namespace PortalServicio.Models
{
    public class ContractorWorker
    {
        #region Properties
        public Guid InternalId { get; set; }
        public string Name { get; set; }
        public string Identification { get; set; }
        public bool RT { get; set; }
        public bool OP { get; set; }
        public bool HD { get; set; }
        public bool Accepted {
            get
            {
                return (RT && OP && HD);
            }
        }
        #endregion

        #region Constructors
        public ContractorWorker(Guid internalId, string name, string id, bool rt = false, bool op = false, bool hd = false)
        {
            InternalId = internalId;
            Name = name;
            Identification = id;
            RT = rt;
            OP = op;
            HD = hd;
        }
        #endregion
    }
}
