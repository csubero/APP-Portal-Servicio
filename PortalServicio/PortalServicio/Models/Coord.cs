using SQLite;

namespace PortalServicio.Models
{
    public class Coord
    {
        #region Properties
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        #endregion
    }
}
