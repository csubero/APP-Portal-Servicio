using PortalServicio.Models;

namespace PortalServicio.ViewModels
{
    public class CoordViewModel : BaseViewModel
    {
        #region Properties
        private int _Id;
        public int Id
        {
            get { return _Id; }
            set { SetValue(ref _Id, value); }
        }
        private double _Latitude;
        public double Latitude
        {
            get { return _Latitude; }
            set { SetValue(ref _Latitude, value); }
        }
        private double _Longitude;
        public double Longitude
        {
            get { return _Longitude; }
            set { SetValue(ref _Longitude, value); }
        }
        #endregion

        public CoordViewModel(Coord coord)
        {
            if (coord == null)
                return;
            Id = coord.Id;
            Latitude = coord.Latitude;
            Longitude = coord.Longitude;
        }

        public Coord ToModel() =>
            new Coord
            {
                Id = Id,
                Latitude = Latitude,
                Longitude = Longitude
            };
    }
}
