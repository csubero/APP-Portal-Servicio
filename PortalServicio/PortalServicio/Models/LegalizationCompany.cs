using SQLiteNetExtensions.Attributes;

namespace PortalServicio.Models
{
    public class LegalizationCompany
    {
        [ForeignKey(typeof(Legalization))]
        public int LegalizationId { get; set; }
        [ForeignKey(typeof(Company))]
        public int CompanyId { get; set; }
    }
}
